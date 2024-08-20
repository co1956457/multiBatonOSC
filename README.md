# multiBatonOSC: マルチコメントビューアで受け取ったコメントや関連情報を OSC で転送する プラグイン
このプラグインを使うと、 マルチコメントビューアで受け取ったコメントや関連情報を VirtualCast に OSC (Open Sound Control) で送信することができるようになります。  
  
![multiBatonImage](/docs/multiBatonImage.png)

## 必要環境
1. マルチコメントビューア  
   マルチコメントビューアの取得は[こちら (コメビュとかツールとか)](https://ryu-s.github.io/app/multicommentviewer) から  

## インストール
インストール手順は次の通り  
1. 「multiBatonOSCPlugin.dll」をダウンロード(または自分でコンパイル)  
2. 「multiBatonOSCPlugin.dll」を右クリックし、プロパティのセキュリティ項目「許可する」にチェック  
   右クリック→プロパティ→セキュリティ:このファイルは…☑許可する(K)  
3. マルチコメントビューアの plugins フォルダに「multiBatonOSC」フォルダを作成 (※フォルダ名「multiBatonOSC」は固定)  
4. 上記「multiBatonOSC」フォルダに「multiBatonOSCPlugin.dll」を置く  
   例 C:\\...\\MultiCommentViewer\\plugins\\multiBatonOSC\\multiBatonOSCPlugin.dll  
5. マルチコメントビューアを立ち上げる  
   マルチコメントビューアを立ち上げると設定ファイル「multiBatonOSC.txt」が「multiBatonOSC」フォルダ内に自動作成されます。  
   設定ファイルに関して、基本的にユーザー側での作業はありません。  

## アンインストール
アンインストール手順は次の通り  
1. 「multiBatonOSCPlugin.dll」を削除  
2. 「multiBatonOSC.txt」を削除  
3. 「multiBatonOSC」フォルダを削除  

## 使用方法
使用方法は次の通り  
1. マルチコメントビューアのメニュー「プラグイン」から「multiBatonOSC v\*.\* 設定 (Settings)」を選択  
2. 転送モードを選択し、設定画面を閉じる (設定画面を閉じると新しい設定が反映されます)  

## VirtualCastとの連携
VCI と連携させる手順は次の通り  
1. VirtualCast タイトル画面の「VCI」の中にある「OSC受信機能」を「creator-only」または「enabled」に設定 (どちらを選んだらよいかよくわからなかったら「creator-only」を選択して様子を見てみてください。)  
   (参考: このプラグインでは VirtualCast の「OSC送信機能」は利用していません。)  
2. 「コメントバトン (OSC) Comment baton」と「縦書きコメビュ VCV」を [VirtualCastのページ](https://virtualcast.jp/users/100215#products) から入手  

## プラグインの挙動
OSC 送信形式は2種類  
1. コメント関連情報の送信  
   コメントを受信した際、転送モードが「1:スタジオ」または「2:ルーム」の時に、下記形式で送信  

   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/comment", `blob_comment`, `blob_name`, `str_commentSource`, `int_transferMode`); 

2. 転送モードの送信  
   マルチコメントビューアの起動時、終了時、転送モード変更時に下記形式で送信  

   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/mode", `int_transferMode`);  

## 引数の型及び具体例
ここでは OSC メッセージで使われる引数の型と具体例を紹介します。  
1. "/vci/baton/comment", "/vci/baton/mode"  
   OSC メッセージの送信先や目的を識別するための OSC アドレス  

2. `blob_comment`  
   コメント (BlobAsUtf8 UTF-8 文字列を表すバイト列)  
   - 制御文字の全角変換等、一部編集あり  
   - 取得できないまたは本文がないときは「（不明／Unknown）」または「（本文なし／No body text）」  

3. `blob_name`  
   ユーザー名 (BlobAsUtf8 UTF-8 文字列を表すバイト列)  
   - 固定ハンドルネーム(以下コテハン)が登録されている場合はそのコテハン  
   - コテハンが登録されていない場合、ユーザー名が取得できればユーザー名、取得できなければ「（不明／Unknown）」  
   - 名前がない場合は、「（名前なし／Nameless）」  
   - ニコニコ生放送: 運営コメントの場合は「（運営）」  
   - ニコニコ生放送: コテハンが登録されていない 184 の場合、184 の ID  

4. `str_commentSource`  
   コメントソース (String ASCII 文字列)  
   - ニコニコ生放送  
     - 一般コメント: Nicolive  
     - 184 コメント: Nicolive184  
     - 運営コメント(既定値): NicoliveOperator  
     - 特定の運営コメント: NicoliveAd, NicoliveInfo, NicoliveGift, NicoliveSpi, NicoliveEmotion  
   - SHOWROOM  
     - 全コメント: Showroom  
   - YouTube ライブ  
     - 一般コメント: Youtubelive  
     - スーパーチャット: YoutubeliveSuperchat  
     - メンバーシップ: YoutubeliveMembership  
   - TwitCasting  
     -  一般コメント: Twitcasting  
     - ギフト: TwitcastingGift  
   - Twitch  
     - 全コメント: Twitch  
   - Openrec  
     - 一般コメント: Openrec  
     - スタンプ: OpenrecStamp  
     - エール(ギフト): OpenrecYell  
   - Whowatch  
     - 一般コメント: Whowatch  
     - ギフト: WhowatchGift  

5. `int_transferMode`  
   転送モード (Int32 32bit 整数)  
   - マルチコメントビューア起動時は設定ファイルに保存されているモードの数字(「0」「1」「2」 のどれか)  
   - 転送モード変更時はその新モードの数字  
    0: 転送しない  
    1: スタジオ (ニコ生: 運営コメント)(SHOWROOM: 転送しない)(YouTube等その他のサイト: 全コメント)  
    2: ルーム (全コメント)  
   - マルチコメントビューア終了時には「-1」を送信  

## VCI 側 main.lua の例
コメントの受信  
```lua:main.lua
function exampleComment(comment, senderName, senderCommentSource, mode)
  local isOperator = ((senderCommentSource == 'NicoliveOperator')
                   or (senderCommentSource == 'NicoliveAd') 
                   or (senderCommentSource == 'NicoliveInfo')
                   or (senderCommentSource == 'NicoliveGift') 
                   or (senderCommentSource == 'NicoliveSpi')
                   or (senderCommentSource == 'NicoliveEmotion'))
  if isOperator then
    print('ニコ生運営コメント: '..senderName..'「'..comment..'」')
  elseif (senderCommentSource == Nicolive) then
    print('ニコ生一般コメント: '..senderName..'「'..comment..'」')
  elseif (senderCommentSource == Nicolive184) then
    print('ニコ生184コメント: '..senderName..'「'..comment..'」')
  elseif (senderCommentSource == Youtubelive) then
    print('ユーチューブライブ一般コメント: '..senderName..'「'..comment..'」')
  elseif (senderCommentSource == YoutubeliveSuperchat) then
    print('ユーチューブライブスーパーチャット: '..senderName..'「'..comment..'」')
  end  
end  

-- OSC: コメント受信  
vci.osc.RegisterMethod('/vci/baton/comment', exampleComment, {vci.osc.types.BlobAsUtf8, vci.osc.types.BlobAsUtf8, vci.osc.types.String, vci.osc.types.Int32})  
```
これは VCI 側でコメントを受信するスクリプトの例です。関数 exampleComment は OSC で受信した 4 つの引数 (コメント、名前、コメントソース、転送モード) を受け取ります。この例では、デバッグコンソールにコメントソースに応じたコメントが表示されます。例）ニコ生一般コメント: Taki「わこつ」  

転送モードの受信  
```lua:main.lua
function exampleMode(mode)
  if (mode == 0) then
    print("転送しない")
  elseif (mode == 1) then
    print("スタジオモード")
  elseif (mode == 2) then
    print("ルームモード")
  elseif (mode == -1) then
    print("コメビュとの連携が切れました。")
  end  
end  

-- OSC: 転送モード受信  
vci.osc.RegisterMethod('/vci/baton/mode', exampleMode, {vci.osc.types.Int32})  
```
これは VCI 側で転送モードを受信するスクリプトの例です。関数 exampleMode は OSC で受信した 1 つの引数 (転送モード) を受け取ります。この例では、デバッグコンソールに転送モードに応じたコメントが表示されます。  

詳しくは [バーチャルキャスト公式Wiki: ExportOsc(外部との OSC 通信)](https://wiki.virtualcast.jp/wiki/vci/script/reference/exportosc) をご覧ください。  

## ライセンス
このプラグインは GNU General Public License v3.0 ライセンスです。  

# multiBatonOSC: MultiCommentViewer plugin for transferring comments and related information by OSC.
This is a plugin that allows you to send comments and related information received from MultiCommentViewer to VirtualcCast using OSC (Open Sound Control) protocol.
  
![multiBatonImage](/docs/multiBatonImage.png)

## Requirements
1. MultiCommentViewer  
   You can get MultiCommentViewer from [here](https://ryu-s.github.io/app/multicommentviewer)

## Installation
To install this plugin, please follow these steps:  
1. Download "multiBatonOSCPlugin.dll" (or compile it by yourself). 
2. Right-click on "multiBatonOSCPlugin.dll", select "Properties", and tick the checkbox named "Unblock" in Security.   
   Right click -> Properties -> "☑Unblock".  
3. Create "multiBatonOSC"  folder in the MultiCommentViewer's "pulgins" folder. (notice: don't change the folder name "multiBatonOSC")  
4. Put "multiBatonOSCPlugin.dll" in the "multiBatonOSC" folder.  
   e.g. C:\\...\MultiCommentViewer\\plugins\\multiBatonOSC\\multiBatonOSCPlugin.dll  
5. Boot MultiCommentViewer.  
   After booting MultiCommentViewer, the setting file "multiBatonOSC.txt" will be created automatically.  
   You don't have to change anything in the setting file.  

## Uninstallation
To uninstall this plugin, please follow these steps:
1. Delete "multiBatonOSCPlugin.dll".  
2. Delete "multiBatonOSC.txt".  
3. Delete "multiBatonOSC" folder.  

## Usage
To use this plugin, please follow these steps:
1. Select "multiBatonOSC v\*.\* (Settings)" from the MultiCommentViewer "plugin" menu.  
2. Select the transfer mode and then close the setting window (when the settings window is closed, the new transfer mode is applied).  

## Linking to VirtualCast
To link to the VCI in Virtualcast, please follow these steps:
1. Select "creator-only" or "enabled" from the "OSC Receive Function" drop-down menu in the "VCI" option of the VirtualCast title-window, (if you don't know which one is better for you, select “creator-only” and see how it goes).  
   (FYI, this plugin does not use the function of VirtualCast's "OSC Send".)
2. Get VCI "コメントバトン (OSC) Comment baton" and "縦書きコメビュ VCV" from [VirtualCast](https://virtualcast.jp/users/100215#products).  

## This plugin's behavior
There are two types of OSC sending.  

1. Sending comments and related information  
   When MultiCommentViewer receives a comment, if the transfer mode is "1: Studio" or "2: Room", then this plugin sends the comment in the following format.

   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/comment", `blob_comment`, `blob_name`, `str_commentSource`, `int_transferMode`); 
  
2. Sending the transfer mode  
   When MultiCommentViewer is booted or closed, when the transfer mode is changed, this plugin sends the transfer mode in the following format.  

   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/mode", `int_transferMode`);  

## Argument type and examples
This section explains the types and examples of the arguments that are used in the OSC messages.  
1. "/vci/baton/comment", "/vci/baton/mode"  
   These are the OSC addresses. An OSC address is a string that identifies the destination or the purpose of an OSC message.  

2. `blob_comment`  
   This is a comment (BlobAsUtf8 UTF-8 Blob type). Some examples are:
   - Full-width conversion of control characters, etc.  
   - if this plugin can not get the comment or the comment has no body text, this plugin adds the letters "（不明／Unknown）" or "（本文なし／No body text）"  
  
3. `blob_name`  
   This is a username (BlobAsUtf8 UTF-8 Blob type). Some examples are:  
   - If MultiCommentViewer has a nickname, it will be the nickname.  
   - if MultiCommentViewer does not have a nickname; when this plugin can get the username, it will be the username, otherwise, when this plugin can not get the username, it will be "（不明／Unknown）" .  
   - if the name parameter has no body text, it will be "（名前なし／Nameless）".  
   - niconico Live: If the comment was made by an operator, it will be "（運営）" (it means "(Operator)" in Japanese).  
   - niconico Live: If MultiCommentViewer does not have a nickname for the anonymous user, it will be the anonymous ID.  
  
4. `str_commentSource`  
   This is a comment source (String ASCII Letters). Some examples are:
   - niconico Live  
     - User comment: Nicolive
     - Anonymous user comment: Nicolive184
     - Operator comment (default): NicoliveOperator
     - Some specific operator comment: NicoliveAd, NicoliveInfo, NicoliveGift, NicoliveSpi, NicoliveEmotion
   - SHOWROOM  
     - All comments: Showroom  
   - YouTube Live  
     - User comment: Youtubelive  
     - Super chat: YoutubeliveSuperchat  
     - Membership: YoutubeliveMembership  
   - TwitCasting  
     - User comment: Twitcasting  
     - Gift: TwitcastingGift  
   - Twitch  
     - All comments: Twitch  
   - Openrec  
     - User comment: Openrec  
     - Stamp: OpenrecStamp  
     - Yell (Gift): OpenrecYell  
   - Whowatch  
     - User comment: Whowatch  
     - Gift: WhowatchGift  

5. `int_transferMode`  
   This is a Transfer mode (Int32 32bit Integer). Some examples are:
   - When MultiCommentViewer was booted, this plugin reads the transfer mode number ("0" or "1" or "2") from the setting file.  
   - When the transfer mode is changed, it will be the new mode number.  
    0: Off  
    1: Studio (niconico Live: Operator comments) (SHOWROOM: Off) (YouTube Live, TwitCasting, etc.: All comments)  
    2: Room (All comments)  
   - When MultiCommentViewer is closed, this plugin sends "-1".  

## VCI side: example for the main.lua
Receive comment  
```lua:main.lua
function exampleComment(comment, senderName, senderCommentSource, mode)
  local isOperator = ((senderCommentSource == 'NicoliveOperator')
                   or (senderCommentSource == 'NicoliveAd') 
                   or (senderCommentSource == 'NicoliveInfo')
                   or (senderCommentSource == 'NicoliveGift') 
                   or (senderCommentSource == 'NicoliveSpi')
                   or (senderCommentSource == 'NicoliveEmotion'))
  if isOperator then
    print('niconico Live operator comment: '..senderName..'\"'..comment..'\"')
  elseif (senderCommentSource == Nicolive) then
    print('niconico Live user comment: '..senderName..'\"'..comment..'\"')
  elseif (senderCommentSource == Nicolive184) then
    print('niconico Live anonymous comment: '..senderName..'\"'..comment..'\"')
  elseif (senderCommentSource == Youtubelive) then
    print('YouTube Live user comment: '..senderName..'\"'..comment..'\"')
  elseif (senderCommentSource == YoutubeliveSuperchat) then
    print('YouTube Live super chat: '..senderName..'\"'..comment..'\"')
  end  
end  

-- OSC: receive comment  
vci.osc.RegisterMethod('/vci/baton/comment', exampleComment, {vci.osc.types.BlobAsUtf8, vci.osc.types.BlobAsUtf8, vci.osc.types.String, vci.osc.types.Int32})  
```
This is an example of a script that receives comments from the VCI. The function exampleComment takes four arguments (comment, name, comment source, transfer mode) that are received by OSC. It prints a message that varies depending on the comment source to the debug console. e.g.: niconico Live user comment: Taki "hello".

Receive transfer mode  
```lua:main.lua
function exampleMode(mode)
  if (mode == 0) then
    print("Off")
  elseif (mode == 1) then
    print("Studio mode")
  elseif (mode == 2) then
    print("Room mode")
  elseif (mode == -1) then
    print("The connection with the comment viewer has been lost.")
  end  
end  

-- OSC: receive transfer mode  
vci.osc.RegisterMethod('/vci/baton/mode', exampleMode, {vci.osc.types.Int32})  
```
This is an example of a script that receives the transfer mode from VCI. The function exampleMode takes one argument (transfer mode) that is received by OSC. It prints a message that varies depending on the mode to the debug console.

For more details, please see [VirtualCast Official Wiki: ExportOsc(外部との OSC 通信)](https://wiki.virtualcast.jp/wiki/vci/script/reference/exportosc)  

## License
This plugin is licensed under the GNU General Public License v3.0 License.
