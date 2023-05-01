// https://github.com/CommentViewerCollection/MultiCommentViewer    BouyomiPlugin を参考に作成    GPL-3.0 License
// https://github.com/ValdemarOrn/SharpOSC                          SharpOSC から必要な部分だけ   MIT License
//
// SPDX-License-Identifier: GPL-3.0
// Since the source of BouyomiPlugin that I referred to is GPL-3.0.
// 参考ににした MultiCommentViewer の BouyomiPlugin が GPL-3.0 なので。
//
// 20230501 v1.0 Taki (co1956457)

using System;
using System.ComponentModel.Composition;    // [Export(typeof(IPlugin))]
using System.IO;                            // File, Directory
using System.Collections.Generic;           // IEnumerable<IMessagePart>, List
using System.Windows.Forms;                 // MessageBox
using System.Text;                          // Encoding

// Plugin共通
// \MultiCommentViewer\dll 内の Plugin.dll, SitePlugin.dll, PluginCommon.dll を参照追加
using Plugin;
using SitePlugin;
// using PluginCommon; // ToText

// 各サイトの dll の参照追加は \MultiCommentViewer\dll 内の ***IFと ***SitePlugin が必要
// 「NicoSitePlugin」ではなく「NicoSitePlugin2」 を参照追加すること
// NicoLiveIF.dll, NicoSitePlugin2.dll
// OpenrecIF.dll, OpenrecSitePlugin.dll
// ShowroomIF.dll, ShowroomSitePlugin.dll
// TwicasIF.dll, TwicasSitePlugin.dll
// TwitchIF.dll, TwitchSitePlugin.dll
// WhowatchIF.dll, WhowatchSitePlugin.dll
// YouTubeLifeIF.dll, YouTubeLiveSitePlugin.dll
using NicoSitePlugin;
using OpenrecSitePlugin;
using ShowRoomSitePlugin;
using TwicasSitePlugin;
using TwitchSitePlugin;
using WhowatchSitePlugin;
using YouTubeLiveSitePlugin;

// 対象外
// Mildom    確認できず(コメント取得にはログインが必要)
// LINE LIVE サービス終了
// Mirrativ  スマホ配信
// ミクチャ  スマホ配信
// BIGO LIVE スマホ配信

namespace multiBatonOSCPlugin
{
    // YouTube Live, TwitCasting, Twitch, Openrec
    // (ref. BouyomiPlugin の main.cs)
    static class MessageParts
    {
        public static string ToTextWithImageAlt(this IEnumerable<IMessagePart> parts)
        {
            string s = "";
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part is IMessageText text)
                    {
                        s += text;
                    }
                    else if (part is IMessageImage image)
                    {
                        s += image.Alt;
                    }
                    else if (part is IMessageRemoteSvg remoteSvg)
                    {
                        s += remoteSvg.Alt;
                    }
                    else
                    {

                    }
                }
            }
            return s;
        }
    }

    /// <summary>
    /// multiBatonOscPlugin 本体
    /// </summary>
    [Export(typeof(IPlugin))]
    public class Class1 : IPlugin, IDisposable
    {
        /// <summary>
        /// プラグインのバージョン
        /// </summary>
        // public string Version
        // {
        //     get { return "v1.0"; }
        // }
        public string Version => "v1.0";

        /// <summary>
        /// プラグインの名前
        /// </summary>
        // public string Name
        // {
        //     get { return "multiBatonOSC " + Version + " 設定 (Settings)"; }
        // }
        public string Name => "multiBatonOSC " + Version + " 設定 (Settings)";

        /// <summary>
        /// プラグインの説明
        /// </summary>
        // public string Description { get { return "コメントを VirtualCast へ OSC で転送"; } }
        public string Description => "MultiCommentViewer から VirtualCast へ OSC で送信";

        public IPluginHost Host { get; set; }

        // Form用
        private Form1 _form1;

        // ファイル存在確認エラー用
        int fileExist;

        // プラグインの状態
        // transferMode
        //  0: 転送しない OFF
        //  1: スタジオ Studio [Ni:運営コメント Operator comments] [SHOWROOM:無 N/A] [YouTube等:全転送 ALL]
        //  2: ルーム Room [全転送 ALL comments]
        public int transferMode;

        // 起動時にだけファイルから転送モードを読み込む
        private int initialRead = 0;

        // 設定ファイル のパス
        string readPath;

        /// <summary>
        /// 設定→常に一番手前に表示を選んだ時
        /// </summary>
        public void OnTopmostChanged(bool isTopmost)
        {
            // MessageBox.Show("OnTopmostChanged");
        }

        /// <summary>
        /// 起動時
        /// </summary>
        public virtual void OnLoaded()
        {
            // ファイルの存在確認
            fileExist = fileExistError();

            if (fileExist == 0) // 問題なし
            {
                initialRead = 1;
            }
            else // 問題あり
            {
                showFileExistError(fileExist);
            }
        }

        /// <summary>
        /// 閉じるボタン☒を押した時
        /// </summary>
        public void OnClosing()
        {
            int int_close = -1;
            SendOscMode(int_close);
        }

        /// <summary>
        /// プラグイン→ multiBatonOSC v*.* 設定 (Settings) を選んだ時
        /// </summary>
        public void ShowSettingView()
        {
            if (_form1 == null || _form1.IsDisposed)
            {
                //フォームの生成
                _form1 = new Form1(this);
                _form1.Text = Name;
                _form1.Show();
                _form1.FormClosed += new FormClosedEventHandler(_form1_FormClosed);
            }
            else
            {
                _form1.Focus();
            }
        }


        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // MessageBox.Show("Dispose");
        }


        // 最新コメント日時初期値
        // 起動時にローカルタイムを UTC (Coordinated Universal Time、世界協定時) に変換して取得
        DateTime lastDate = DateTime.UtcNow;

        /// <summary>
        /// コメント取得時にコメントの種類や時間等、関連データの判定を行う。
        /// </summary>
        public void OnMessageReceived(ISiteMessage message, IMessageMetadata messageMetadata)
        {
            if (transferMode > 0) // 稼働中
            {
                (string name, string comment, string commentSource, bool isSend, DateTime dt_lastDate) = GetData(message, lastDate);
                // 最新コメント日時更新
                lastDate = dt_lastDate;

                //nameがnullでは無い場合かつUser.Nicknameがある場合はNicknameを採用
                if (!string.IsNullOrEmpty(name) && messageMetadata.User != null && !string.IsNullOrEmpty(messageMetadata.User.Nickname))
                {
                    name = messageMetadata.User.Nickname;
                }

                if (isSend)
                {
                    // 転送モード 0 は転送しない。
                    // 転送モード 1 はニコ生運営コメント及び YouTube Live 等
                    // 転送モード 2 は全部転送
                    if (((transferMode == 1) && !((commentSource == "Nicolive") || (commentSource == "Nicolive184") || (commentSource == "Showroom"))) || (transferMode == 2))
                    {
                        // OSC で送るデータを引き渡す
                        SendOscComment(comment, name, commentSource, transferMode);
                    }
                }
                /*transferModeでの分岐 (※動作理解のためにコメントアウトで残しておく)
                if (transferMode == 0) // 転送しない
                { }
                else if ((transferMode == 1) && (commentSource == "Nicolive")) // スタジオ ニコ生一般コメント転送しない
                { }
                else if ((transferMode == 1) && (commentSource == "Nicolive184")) // スタジオ 184コメント転送しない
                { }
                else if ((transferMode == 1) && (commentSource == "Showroom")) // スタジオ ショールームコメント転送しない
                { }
                else // スタジオ：ニコ生運営コメント、YouTube等　 ルーム：全転送 (transferMode ==2)
                {
                    // OSC で送るデータを引き渡す
                    SendOscComment(comment, name, commentSource, transferMode);
                }
                */
            }
            else
            {
                // do nothing
            }
        }


        /// <summary>
        /// コメント等各種情報を OSC で送信
        /// SharpOSC (MIT license) を部分利用
        /// https://github.com/ValdemarOrn/SharpOSC
        /// </summary>
        /// 
        static void SendOscComment(string commentOSC, string nameOSC, string commentSourceOSC, int transferModeOSC)
        {
            try
            {
                Encoding utf8 = Encoding.UTF8;
                byte[] blob_comment = utf8.GetBytes(commentOSC);
                byte[] blob_name = utf8.GetBytes(nameOSC);
                string str_commentSource = commentSourceOSC;
                int int_transferMode = transferModeOSC;

                var message = new OscMessage("/vci/baton/comment", blob_comment, blob_name, str_commentSource, int_transferMode);
                var sender = new UDPSender("127.0.0.1", 19100);
                sender.Send(message);
            }
            //catch (Exception error)
            catch (Exception)
            {
                // MessageBox.Show(error.ToString());  // just in case
            }
        }

        static void SendOscMode(int modeOSC)
        {
            try
            {
                int int_transferMode = modeOSC;

                var message = new OscMessage("/vci/baton/mode", int_transferMode);
                var sender = new UDPSender("127.0.0.1", 19100);
                sender.Send(message);
            }
            //catch (Exception error)
            catch (Exception)
            {
                // MessageBox.Show(error.ToString());  // just in case
            }
        }

        /// <summay>
        /// 最新コメント日時より新しかったら最新コメント日時を更新
        /// 元最新コメント日時の1分前からのコメントは転送する
        /// </summay>
        private static (bool, DateTime) CompareTime(DateTime _lastDate, DateTime livesiteUTC)
        {
            bool _isSend = false;
            DateTime dt_lastDate = _lastDate;
            DateTime marginLastDate = _lastDate.AddMinutes(-1);

            int result = DateTime.Compare(dt_lastDate, livesiteUTC);
            if (result < 0)
            {
                dt_lastDate = livesiteUTC;
            }

            int isSendResult = DateTime.Compare(marginLastDate, livesiteUTC);
            if (isSendResult < 0)
            {
                _isSend = true;
            }
            return (_isSend, dt_lastDate);
        }

        /// <summary>
        /// 名前とコメントを取得
        /// 参考：BouyomiPlugin main.cs
        /// </summary>
        private static (string name, string comment, string cmtSource, bool isSend, DateTime dt_lastDate) GetData(ISiteMessage message, DateTime _lastDate)
        {
            string name = "（不明／Unknown）";
            string comment = "（不明／Unknown）";
            string cmntSource = "Unknown";
            bool isSend = true;
            DateTime dt_lastDate = _lastDate;

            DateTime postedAtUTC;

            // (ref. NicoLiveIF/Message.cs)
            if (message is INicoMessage NicoMessage)
            {
                name = "（運営）";
                // 今後の VCI 開発はコメントバトンを介さず直接データを受信する形式になることが予想される。
                // OSC 対応版を機に commentSource の文字列を変更。
                // NCV も MultiCommentViewer での分類に合わせた。
                // 一般コメント         : Nicolive
                // 184コメント          : Nicolive184
                // 運営コメント(既定値) : NicoliveOperator
                // 特定の運営コメント   : NicoliveAd, NicoliveInfo, NicoliveGift, NicoliveSpi, NicoliveEmotion
                //
                // ※以前のプラグインでは、運営コメントの commentSource は "NCV" としていた。
                // 　(当初は NCV から運営コメントを送るだけだった名残）。
                // 　既存のコメントバトンを利用した VCI に影響が出ないよう、
                // 　コメントバトン (OSC) の main.lua 側で "NCV" に戻す等対応。
                cmntSource = "NicoliveOperator";

                switch (NicoMessage.NicoMessageType)
                    {
                        case NicoMessageType.Connected:
                            comment = (NicoMessage as INicoConnected).Text;
                            break;
                        case NicoMessageType.Disconnected:
                            comment = (NicoMessage as INicoDisconnected).Text;
                            break;
                        case NicoMessageType.Ad:
                            // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                            postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((NicoMessage as INicoInfo).PostedAt);
                            (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                            cmntSource = "NicoliveAd";
                            comment = (NicoMessage as INicoAd).Text;
                            break;
                        case NicoMessageType.Info:
                            // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                            postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((NicoMessage as INicoInfo).PostedAt);
                            (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                            cmntSource = "NicoliveInfo";
                            comment = (NicoMessage as INicoInfo).Text;
                            break;
                        case NicoMessageType.Item:
                            // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                            postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((NicoMessage as INicoGift).PostedAt);
                            (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                            cmntSource = "NicoliveGift";
                            comment = (NicoMessage as INicoGift).Text;
                            break;
                        case NicoMessageType.Spi:
                            // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                            postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((NicoMessage as INicoSpi).PostedAt);
                            (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                            cmntSource = "NicoliveSpi";
                            comment = (NicoMessage as INicoSpi).Text;
                            // 「/spi 」の文字列はデフォルトで入る
                            // /spi "「***」がリクエストされました"
                            break;
                        case NicoMessageType.Emotion:
                            // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                            postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((NicoMessage as INicoEmotion).PostedAt);
                            (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                            cmntSource = "NicoliveEmotion";
                            comment = (NicoMessage as INicoEmotion).Content;
                            break;
                        case NicoMessageType.Comment:
                            // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                            postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((NicoMessage as INicoComment).PostedAt);
                            (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                            if ((NicoMessage as INicoComment).Is184 == true)
                            {
                                cmntSource = "Nicolive184";
                                name = (NicoMessage as INicoComment).UserId; // UserNameの値無し
                            }
                            else
                            {
                                cmntSource = "Nicolive";
                                name = (NicoMessage as INicoComment).UserName;
                            }
                            comment = (NicoMessage as INicoComment).Text;
                            break;
                    }
            }

            // SHOWROOM：全転送時 (ref. ShowRoomIF/Message.cs)
            // ShowRoomMessageTypeにItemがない。ギフトの判定できない？ Unknown でもできない…
            else if (message is IShowRoomMessage showroomMessage)
            {
                cmntSource = "Showroom";

                switch (showroomMessage.ShowRoomMessageType)
                {
                    case ShowRoomMessageType.Connected:
                        comment = (showroomMessage as IShowRoomConnected).Text;
                        break;
                    case ShowRoomMessageType.Disconnected:
                        comment = (showroomMessage as IShowRoomDisconnected).Text;
                        break;
                    case ShowRoomMessageType.Comment:
                        // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                        postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((showroomMessage as IShowRoomComment).PostedAt);
                        (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);
                        // postedAtUTC example
                        //「2023 /04/22 8:14:06」

                        name = (showroomMessage as IShowRoomComment).UserName;
                        comment = (showroomMessage as IShowRoomComment).Text;

                        break;
                        /* ギフト判定はできそうにない
                        case ShowRoomMessageType.Unknown:
                            if ((showroomMessage as IShowRoomItem).PostTime != null)
                            {
                                cmntSource = "ShowroomGift";
                                name = "ギフトGift";
                                comment = (showroomMessage as IShowRoomItem).PostTime;
                                // MessageBox.Show("name: " + name + "\ncomment: " + comment);
                            }
                            break;
                        */
                }
            }

            // YouTube Live (ref. YouTubeLiveIF/Message.cs)
            else if (message is IYouTubeLiveMessage youTubeLiveMessage)
            {
                cmntSource = "Youtubelive";

                switch (youTubeLiveMessage.YouTubeLiveMessageType)
                {
                    case YouTubeLiveMessageType.Connected:
                        comment = (youTubeLiveMessage as IYouTubeLiveConnected).Text;
                        break;
                    case YouTubeLiveMessageType.Disconnected:
                        comment = (youTubeLiveMessage as IYouTubeLiveDisconnected).Text;
                        break;
                    case YouTubeLiveMessageType.Comment:
                        // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                        postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((youTubeLiveMessage as IYouTubeLiveComment).PostedAt);
                        (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                        // name = (youTubeLiveMessage as IYouTubeLiveComment).NameItems.ToText();
                        name = (youTubeLiveMessage as IYouTubeLiveComment).NameItems.ToTextWithImageAlt();
                        comment = (youTubeLiveMessage as IYouTubeLiveComment).CommentItems.ToTextWithImageAlt(); // .ToText(); ではなくこちらを使う
                        break;
                    case YouTubeLiveMessageType.Superchat:
                        // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                        postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((youTubeLiveMessage as IYouTubeLiveSuperchat).PostedAt);
                        (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                        // コメントバトン (OSC) では既存 VCI への影響から YoutubeSC に変換
                        cmntSource = "YoutubeliveSuperchat";
                        name = (youTubeLiveMessage as IYouTubeLiveSuperchat).NameItems.ToTextWithImageAlt();
                        comment = (youTubeLiveMessage as IYouTubeLiveSuperchat).PurchaseAmount + " " + (youTubeLiveMessage as IYouTubeLiveSuperchat).CommentItems.ToTextWithImageAlt(); // .ToText(); ではなくこちらを使う
                        // Super Chat comment example
                        // 「名前」「\1,000 おめでとう🎉」

                        break;

                    // 一応実装しておく
                    case YouTubeLiveMessageType.Membership:
                        // コメントの時間を取得し UTC に変換して比較 直近1分から転送可
                        postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((youTubeLiveMessage as IYouTubeLiveMembership).PostedAt);
                        (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                        cmntSource = "YoutubeliveMembership";
                        name = (youTubeLiveMessage as IYouTubeLiveMembership).NameItems.ToTextWithImageAlt();
                        comment = (youTubeLiveMessage as IYouTubeLiveMembership).CommentItems.ToTextWithImageAlt(); // .ToText(); ではなくこちらを使う
                        // MessageBox.Show("name: " + name + "\ncomment: " + comment);

                        break;
                }
            }

            // TwitCasting (ref. TwicasIF/Message.cs)
            else if (message is ITwicasMessage twicasMessage)
            {
                cmntSource = "Twitcasting";

                switch (twicasMessage.TwicasMessageType)
                {
                    case TwicasMessageType.Connected:
                        comment = (twicasMessage as ITwicasConnected).Text;
                        break;
                    case TwicasMessageType.Disconnected:
                        comment = (twicasMessage as ITwicasDisconnected).Text;
                        break;
                    case TwicasMessageType.Comment:
                        // string postTime = (twicasMessage as ITwicasComment).PostTime;
                        // MessageBox.Show(postTime);
                        // 結果「01:51:39」
                        // 過去コメントは最大 20 という制限があるもよう。
                        // 時間判定は行わなくても良い許容範囲か。

                        name = (twicasMessage as ITwicasComment).UserName;
                        comment = (twicasMessage as ITwicasComment).CommentItems.ToTextWithImageAlt();
                        break;
                    case TwicasMessageType.Item:
                        cmntSource = "TwitcastingGift";
                        name = (twicasMessage as ITwicasItem).UserName;
                        comment = (twicasMessage as ITwicasItem).CommentItems.ToTextWithImageAlt();
                        // Gift comment example
                        //「名前」「がんばってね(+🍡2)　　お茶」
                        //「名前」「おめでとう！！！(+🍡200)　　お茶ｘ10」
                        //「名前」「(+🍡15)　　花火」
                        //「名前」「(+🍡✨)　　お茶爆100」
                        break;
                }
            }

            // Twitch (ref. TwitchIF/Message.cs Item コメントアウト)
            else if (message is ITwitchMessage twitchMessage)
            {
                cmntSource = "Twitch";

                switch (twitchMessage.TwitchMessageType)
                {
                    case TwitchMessageType.Connected:
                        comment = (twitchMessage as ITwitchConnected).Text;
                        break;
                    case TwitchMessageType.Disconnected:
                        comment = (twitchMessage as ITwitchDisconnected).Text;
                        break;
                    case TwitchMessageType.Comment:
                        // string postTime = (twitchMessage as ITwitchComment).PostTime;
                        // MessageBox.Show(postTime);
                        // 結果「02:26:01」
                        // 過去コメントを取得しないようなので、時間判定不要か。

                        name = (twitchMessage as ITwitchComment).DisplayName;
                        comment = (twitchMessage as ITwitchComment).CommentItems.ToTextWithImageAlt();
                        break;
                }
            }

            // Openrec (ref. OpenrecIF/Message.cs)
            else if (message is IOpenrecMessage openrecMessage)
            {
                cmntSource = "Openrec";

                switch (openrecMessage.OpenrecMessageType)
                {
                    case OpenrecMessageType.Connected:
                        comment = (openrecMessage as IOpenrecConnected).Text;
                        break;
                    case OpenrecMessageType.Disconnected:
                        comment = (openrecMessage as IOpenrecDisconnected).Text;
                        break;
                    case OpenrecMessageType.Comment:
                        // コメントの時間を取得し UTC に変換して比較 直近1分から転送可(サーバー時間が少し進んでいるかも)
                        postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((openrecMessage as IOpenrecComment).PostTime);
                        (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                        name = (openrecMessage as IOpenrecComment).NameItems.ToTextWithImageAlt();
                        comment = (openrecMessage as IOpenrecComment).MessageItems.ToTextWithImageAlt();
                        break;
                    case OpenrecMessageType.Stamp:
                        // コメントの時間を取得し UTC に変換して比較 直近1分から転送可(サーバー時間が少し進んでいるかも)
                        postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((openrecMessage as IOpenrecStamp).PostTime);
                        (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);

                        cmntSource = "OpenrecStamp";
                        name = (openrecMessage as IOpenrecStamp).NameItems.ToTextWithImageAlt();
                        // (openrecMessage as IOpenrecStamp).Message は値が無い "" 模様
                        comment = "Stamp " + (openrecMessage as IOpenrecStamp).Message;
                        // Stamp comment example
                        // 「名前」「Stamp 」
                        break;
                    case OpenrecMessageType.Yell:
                        // コメントの時間を取得し UTC に変換して比較 直近1分から転送可(サーバー時間が少し進んでいるかも)
                        postedAtUTC = TimeZoneInfo.ConvertTimeToUtc((openrecMessage as IOpenrecYell).PostTime);
                        (isSend, dt_lastDate) = CompareTime(dt_lastDate, postedAtUTC);
                        // PostTime という名前だが DateTime 形式
                        // 例 2023/04/21 20:13:44

                        cmntSource = "OpenrecYell";
                        name = (openrecMessage as IOpenrecYell).NameItems.ToTextWithImageAlt();
                        comment = (openrecMessage as IOpenrecYell).YellPoints + " " + (openrecMessage as IOpenrecYell).Message;
                        // Yell comment example
                        //「名前」「1000 」
                        //「名前」「1250 ナイス👍」
                        //「名前」「625 おにぎり🍙🍙🍙」
                        break;
                }
            }

            // Whowatch (ref. OpenrecIF/Message.cs)
            else if (message is IWhowatchMessage whowatchMessage)
            {
                cmntSource = "Whowatch";

                switch (whowatchMessage.WhowatchMessageType)
                {
                    case WhowatchMessageType.Connected:
                        comment = (whowatchMessage as IWhowatchConnected).Text;
                        break;
                    case WhowatchMessageType.Disconnected:
                        comment = (whowatchMessage as IWhowatchDisconnected).Text;
                        break;
                    case WhowatchMessageType.Comment:
                        // string postTime = (whowatchMessage as IWhowatchComment).PostTime;
                        // MessageBox.Show(postTime);
                        // 結果「14:20:36」

                        name = (whowatchMessage as IWhowatchComment).UserName;
                        comment = (whowatchMessage as IWhowatchComment).Comment;
                        break;
                    case WhowatchMessageType.Item:
                        // long postedAt = (whowatchMessage as IWhowatchItem).PostedAt;
                        // MessageBox.Show(postedAt.ToString());
                        // 結果「1682144564000」
                        // 結果「1682144319000」
                        // TimeSpan?

                        cmntSource = "WhowatchGift";
                        name = (whowatchMessage as IWhowatchItem).UserName;
                        comment = (whowatchMessage as IWhowatchItem).Comment + "[" + (whowatchMessage as IWhowatchItem).ItemName + "x" + (whowatchMessage as IWhowatchItem).ItemCount.ToString() + "]";
                        // Gift comment example
                        //「名前」「ふわブロック美しい✨[風船x1]」
                        //「名前」「こんぺいとうを3個プレゼントしました。[こんぺいとうx3]」
                        //「名前」「バイト先では？[イベント応援するゾウ！x1]」
                        break;
                }
            }

            // YouTube Live Super Chat 等改行が入ることがある \r 置換が有効
            // コメント中の「'」に要注意　's など英語コメントでよく入る
            comment = comment.Replace("\n", "　").Replace("\r", "　").Replace("\'", "’").Replace("\"", "”").Replace("\\", "＼");
            comment = comment.Replace("$", "＄").Replace("/", "／").Replace(",", "，");
            name = name.Replace("\n", "　").Replace("\r", "　").Replace("\'", "’").Replace("\"", "”").Replace("\\", "＼");
            name = name.Replace("$", "＄").Replace("/", "／").Replace(",", "，");

            // 念のため
            if ((name == null) || (name == ""))
            {
                name = "（名前なし／Nameless）";
            }
            if ((comment == null) || (comment == ""))
            {
                comment = "（本文なし／No body text）";
            }
            if ((cmntSource == null) || (cmntSource == ""))
            {
                cmntSource = "Unknown";
            }

            return (name, comment, cmntSource, isSend, dt_lastDate);
        }

        /// <summary>
        /// ファイルの存在確認
        /// </summary>
        int fileExistError()
        {
            // 値を返す用
            int returnInt;
            // カレントディレクトリ（MultiCommentViewer 実行ディレクトリ）
            string curDirectory = Environment.CurrentDirectory;
            // プラグインディレクトリ
            curDirectory = curDirectory + "\\plugins\\multiBatonOSC";
            // 設定ファイル名
            readPath = curDirectory + "\\multiBatonOSC.txt";

            // ファイルの存在確認
            if (File.Exists(readPath)) // 設定ファイルあり
            {
                // 行ごとの配列として、テキストファイルの中身をすべて読み込む
                string[] lines = File.ReadAllLines(readPath);
                if (initialRead == 0) // 起動時のみファイルから転送モード読み込み
                {
                    // transferMode
                    //  0: 転送しない OFF
                    //  1: スタジオ Studio [Ni:運営コメント Operator comments] [SHOWROOM:無 N/A] [YouTube等:全転送 ALL]
                    //  2: ルーム Room [全転送 ALL comments]
                    //
                    if (lines[0] == "0" || lines[0] == "1" || lines[0] == "2")
                    {
                        transferMode = int.Parse(lines[0]);
                    }
                    else
                    {
                        transferMode = 1; // initial setting
                    }
                }
                SendOscMode(transferMode);
                returnInt = 0;
            }
            else
            {
                try
                {
                    // 初導入時等、設定ファイルがない時
                    transferMode = 1;
                    File.WriteAllText(readPath, transferMode.ToString());
                    SendOscMode(transferMode);
                    returnInt = 0;
                }
                catch (Exception)
                {
                    returnInt = 1;
                }
            }
            return returnInt;
        }

        /// <summary>
        /// エラー表示
        /// </summary>
        void showFileExistError(int errorNumber)
        {
            if (errorNumber == 1)
            {
                MessageBox.Show("設定ファイルが作成できません。\nThis plugin couldnt make the setting file.\n\n1. …\\MultiCommentViewer\\plugins\\multiBatonOSC\\multiBatonOSC.txt を作成してください。\n   Please create 'multiBatonOSC.txt' file.\n\n2. multiBatonOSC.txt に 半角で「1」を書いて保存してください。\n   Please write the number '1' in the text file.\n\n3. MultiCommentViewer を立ち上げなおしてください。\n   Please reboot MultiCommentViewer.", "multiBatonOSC エラー error");
            }
        }

        //フォームが閉じられた時のイベントハンドラ
        void _form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            int old_transferMode = transferMode;
            transferMode = _form1.tMode;

            if (old_transferMode != transferMode)
            {
                // 設定ファイルにパスとモードを保存
                try
                {
                    File.WriteAllText(readPath, transferMode.ToString());
                }
                catch (Exception)
                {
                    int errorNumber = 1;
                    showFileExistError(errorNumber);
                }

                SendOscMode(transferMode);
            }

            //フォームが閉じられた時のイベントハンドラ削除
            _form1.FormClosed -= _form1_FormClosed;
            _form1 = null;
        }
    }
}