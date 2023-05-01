using System;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;

namespace multiBatonOSCPlugin
{
    public partial class Form1 : Form
    {
        private Class1 _class1;
        public int tMode;

        /// <summary>
        /// 設定ウィンドウを開く
        /// </summary>
        public Form1(Class1 c)
        {
            _class1 = c; // メインへの参照
            tMode = _class1.transferMode;
            this.AutoSize = true;

            SetupControls();
        }

        TextBox textURL;
        Label labelVersionInfo, labelCheckResult;

        /// <summary>
        /// 設定ウィンドウの中身
        /// </summary>
        public void SetupControls()
        {
            GroupBox groupTransferMode, groupUpdateCheck;
            RadioButton radioOff, radioStudio, radioRoom;
            Button buttonApply, buttonCheckUptates;

            // グループ：転送モード
            groupTransferMode = new GroupBox();
            groupTransferMode.Font = new Font("MS UI Gothic", 12);
            groupTransferMode.AutoSize = true;
            groupTransferMode.Top = 8;
            groupTransferMode.Left = 4;
            groupTransferMode.Text = "転送モード Transfer mode";
            this.Controls.Add(groupTransferMode);

            // ラジオボタン：転送しない
            radioOff = new RadioButton();
            radioOff.Font = new Font("MS UI Gothic", 14);
            radioOff.Name = "0";
            radioOff.AutoSize = true;
            radioOff.Text = "0: 転送しない Off";
            radioOff.Top = 30;
            radioOff.Left = 15;
            if (tMode == 0)
            {
                radioOff.Checked = true;
            }
            else
            {
                radioOff.Checked = false;
            }
            radioOff.CheckedChanged += Check_changed;
            groupTransferMode.Controls.Add(radioOff);

            // ラジオボタン：スタジオ
            radioStudio = new RadioButton();
            radioStudio.Font = new Font("MS Gothic", 14);
            radioStudio.Name = "1";
            radioStudio.AutoSize = true;
            radioStudio.Text = "1: スタジオ Studio (ニコ生:運営コメント niconico: Operator comments)\n" +
                               "(SHOWROOM:無 Off)(YouTube, TwitCasting etc.:全コメント All comments)";
            radioStudio.Top = 70;
            radioStudio.Left = 15;
            if (tMode == 1)
            {
                radioStudio.Checked = true;
            }
            else
            {
                radioStudio.Checked = false;
            }
            radioStudio.CheckedChanged += Check_changed;
            groupTransferMode.Controls.Add(radioStudio);

            // ラジオボタン：ルーム
            radioRoom = new RadioButton();
            radioRoom.Font = new Font("MS Gothic", 14);
            radioRoom.Name = "2";
            radioRoom.AutoSize = true;
            radioRoom.Text = "2: ルーム Room (全コメント All comments)";
            radioRoom.Top = 140;
            radioRoom.Left = 15;
            if (tMode == 2)
            {
                radioRoom.Checked = true;
            }
            else
            {
                radioRoom.Checked = false;
            }
            radioRoom.CheckedChanged += Check_changed;
            groupTransferMode.Controls.Add(radioRoom);

            // 閉じる(適用)ボタン
            buttonApply = new Button();
            buttonApply.Font = new Font("MS Gothic", 14);
            buttonApply.Location = new Point(10, 190);
            buttonApply.AutoSize = true;
            buttonApply.Text = "閉じる (適用) / Close (Apply)";
            buttonApply.Click += new EventHandler(ButtonApply_Click);
            groupTransferMode.Controls.Add(buttonApply);


            // グループ：更新確認
            groupUpdateCheck = new GroupBox();
            groupUpdateCheck.Font = new Font("MS Gothic", 12);
            groupUpdateCheck.Width = groupTransferMode.Width;
            groupUpdateCheck.AutoSize = true;
            groupUpdateCheck.Top = 290;
            groupUpdateCheck.Left = 4;
            groupUpdateCheck.Text = "更新を確認 Check for updates";
            this.Controls.Add(groupUpdateCheck);

            // ラベル：バージョン情報
            labelVersionInfo = new Label();
            labelVersionInfo.Font = new Font("MS Gothic", 14);
            labelVersionInfo.Location = new Point(170, 35);
            labelVersionInfo.AutoSize = true;
            labelVersionInfo.Text = _class1.Version + " -> ";
            groupUpdateCheck.Controls.Add(labelVersionInfo);

            // ラベル：更新情報問い合わせ結果
            labelCheckResult = new Label();
            labelCheckResult.Font = new Font("MS Gothic", 14);
            labelCheckResult.Location = new Point(10, 70);
            labelCheckResult.AutoSize = true;
            labelCheckResult.Text = "";
            groupUpdateCheck.Controls.Add(labelCheckResult);

            // ボタン：更新確認
            buttonCheckUptates = new Button();
            buttonCheckUptates.Font = new Font("MS Gothic", 10);
            buttonCheckUptates.Location = new Point(10, 34);
            buttonCheckUptates.AutoSize = true;
            buttonCheckUptates.Text = "確認 (Check)";
            buttonCheckUptates.Click += new EventHandler(ButtonCheckUpdates_Click);
            groupUpdateCheck.Controls.Add(buttonCheckUptates);

            // テキストボックス：サイトURL
            textURL = new TextBox();
            textURL.Font = new Font("MS Gothic", 10);
            textURL.Location = new Point(10, 110);
            //textURL.AutoSize = true;  // 12文字程度の短いテキストボックスになる。
            textURL.Width = groupTransferMode.Width - 20;
            textURL.Text = "https://github.com/co1956457/multiBatonOSC/releases/latest";
            textURL.ReadOnly = true;
            textURL.MouseDown += TextURL_MouseDown;
            groupUpdateCheck.Controls.Add(textURL);
        }

        /// <summary>
        /// ラジオボタンが押された
        /// 転送モードを一時保有
        /// </summary>
        private void Check_changed(object sender, EventArgs e)
        {
            RadioButton btn = (RadioButton)sender;
            tMode = int.Parse(btn.Name);

        }

        /// <summary>
        /// 閉じる(適用)ボタンが押された
        /// ウィンドウを閉じる
        /// </summary>
        private void ButtonApply_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// URLがクリックされた
        /// コピー用に全選択
        /// </summary>
        private void TextURL_MouseDown(object sender, MouseEventArgs e)
        {
            this.textURL.SelectAll();
        }

        /// <summary>
        /// 確認ボタンが押された
        /// 問い合わせ結果に応じてテキスト表示
        /// </summary>
        private async void ButtonCheckUpdates_Click(object sender, EventArgs e)
        {
            string gotVersion = await GetLatestVersion();
            string versionText = _class1.Version + " -> " + gotVersion;
            string checkResultText = "新しいものが公開されています。 New version was released.";
            if (_class1.Version == gotVersion)
            {
                checkResultText = "最新版を使っています。 This is the latest.";
            }
            else if (gotVersion == "v?.?")
            {
                checkResultText = "情報が取得できませんでした。 Failed to retrieve information.";
            }
            labelVersionInfo.Text = versionText;
            labelCheckResult.Text = checkResultText;
        }

        /// <summary>
        /// Github に release latest を問い合わせて最新版の値を返す
        /// https://api.github.com/repos/co1956457/multiBatonOSC/releases/latest
        /// </summary>
        private static readonly HttpClient httpClient = new HttpClient();

        private async Task<string> GetLatestVersion()
        {
            string latestVersion = "v?.?";
            try
            {
                string url = "https://api.github.com/repos/co1956457/multiBatonOSC/releases/latest";
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "multiBatonOSC");
                string responseJson = await httpClient.GetStringAsync(url);
                // Github の API は現在 XML 形式でのレスには対応していないらしい(以前はフォーマット指定ができたらしい)。
                // System.Text.Json, System.Memory の関係か、JsonDocument.Parse, JsonSerializer 等
                // コンパイルは通るが実行環境でエラー(System.Memoryのバージョンの問題？)。
                // 今回は単純に string 操作とした。
                string editJson = responseJson.Remove(0, responseJson.IndexOf("/releases/tag/"));   //「/releases/tag/」の前まで削除
                editJson = editJson.Remove(0, 14);                                                  // 先頭14文字「/releases/tag/」削除
                editJson = editJson.Remove(editJson.IndexOf("\""));

                latestVersion = editJson;
            }
            catch (HttpRequestException)
            {
                // do nothing
            }
            return latestVersion;
        }
    }
}