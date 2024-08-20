# multiBatonOSC: マルチコメントビューアで受け取ったコメントや関連情報を OSC で転送するプラグイン
このプラグインを使うと、 マルチコメントビューアで受け取ったコメントや関連情報を VirtualCast に OSC (Open Sound Control) で送信することができるようになります。  
  
![multiBatonImage](/docs/multiBatonImage.png)

## 必要環境
1. マルチコメントビューア  
   マルチコメントビューアの取得は[こちら (コメビュとかツールとか)](https://ryu-s.github.io/app/multicommentviewer) から  

## インストール
1. 「multiBatonOSCPlugin.dll」をダウンロードするか、ソースからコンパイルします。  
2. 「multiBatonOSCPlugin.dll」を右クリックし、プロパティのセキュリティ項目「許可する」にチェックを入れます。  
   手順: 右クリック→プロパティ→セキュリティ:このファイルは…☑許可する(K)  
3. マルチコメントビューアの plugins フォルダに「multiBatonOSC」フォルダを作成 (※フォルダ名「multiBatonOSC」は固定)  
4. 上記「multiBatonOSC」フォルダに「multiBatonOSCPlugin.dll」を置きます。  
   例 C:\\...\\MultiCommentViewer\\plugins\\multiBatonOSC\\multiBatonOSCPlugin.dll  
5. マルチコメントビューアを起動します。  
   マルチコメントビューア起動後、設定ファイル「multiBatonOSC.txt」が「multiBatonOSC」フォルダ内に自動作成されます。  
   設定ファイルに関して、ユーザー側で特別な作業は必要ありません。  

## アンインストール
1. 「multiBatonOSCPlugin.dll」を削除  
2. 「multiBatonOSC.txt」を削除  
3. 「multiBatonOSC」フォルダを削除  

## 使用方法
1. マルチコメントビューアのメニュー「プラグイン」から「multiBatonOSC v0.0 設定 (Settings)」を選択します。  
2. 転送モードを選択し、設定画面を閉じます。設定画面を閉じると新しい設定が反映されます。  

## VirtualCastとの連携
1. VirtualCast のタイトル画面で「VCI」の中の「OSC受信機能」を「creator-only」または「enabled」に設定します。どちらを選ぶべきか不明な場合は、「creator-only」を選択して様子を見てみてください。  
   (参考: このプラグインでは VirtualCast の「OSC送信機能」は使用していません。)  
2. 「縦書きコメビュ (OSC) VCV」や「エモーション表示 (OSC) Show the Emotion」、「コメント短冊 (OSC) Comment tags」を [VirtualCastのページ](https://virtualcast.jp/users/100215#products) から入手します。  

## プラグインの挙動
1. コメント関連情報の送信  
   コメントを受信した際、転送モードが「1:スタジオ」または「2:ルーム」の時に、以下の形式で送信されます。    
   ```C#:Class1.cs
   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/comment", blob_comment, blob_name, str_commentSource, int_transferMode); 
   ```
2. 転送モードの送信  
   マルチコメントビューアの起動時、終了時、転送モード変更時に以下の形式で送信されます。 
   ```C#:Class1.cs
   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/mode", int_transferMode);  
   ```

## 引数の型及び具体例
ここでは OSC メッセージで使われる引数の型と具体例を紹介します。  
1. `/vci/baton/comment`, `/vci/baton/mode`  
   OSC メッセージの送信先や目的を識別するための OSC アドレスです。  

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
これは VCI 側でコメントを受信するスクリプトの例です。関数 `exampleComment` は OSC で受信した 4 つの引数 (コメント、名前、コメントソース、転送モード) を受け取ります。この例では、デバッグコンソールにコメントソースに応じたコメントが表示されます。  

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
これは VCI 側で転送モードを受信するスクリプトの例です。関数 `exampleMode` は OSC で受信した 1 つの引数 (転送モード) を受け取ります。この例では、デバッグコンソールに転送モードに応じたコメントが表示されます。  

詳細については [バーチャルキャスト公式Wiki: ExportOsc(外部との OSC 通信)](https://wiki.virtualcast.jp/wiki/vci/script/reference/exportosc) をご覧ください。  

## ライセンス
このプラグインは GNU General Public License v3.0 ライセンスのもとで公開されています。 

# multiBatonOSC: A Plugin for Sending Comments and Related Information Received by Multi-Comment Viewer via OSC
This plugin allows you to send comments and related information received by the Multi Comment Viewer to VirtualCast using OSC (Open Sound Control).  
  
![multiBatonImage](/docs/multiBatonImage.png)  

## Requirements
1. Multi Comment Viewer  
   You can obtain the Multi Comment Viewer from [here](https://ryu-s.github.io/app/multicommentviewer)  

## Installation
1. Download the `multiBatonOSCPlugin.dll` or compile it from the source.  
2. Right-click on `multiBatonOSCPlugin.dll`, go to Properties, and tick the checkbox named "Unblock" in Security.   
   Procedure: Right-click → Properties → Security: This file came... ☑ Unbloc<u>k</u>  
3. Create a `multiBatonOSC` folder in the plugins directory of the Multi Comment Viewer (The folder name `multiBatonOSC` is fixed).  
4. Place `multiBatonOSCPlugin.dll` into the `multiBatonOSC` folder.  
   e.g. C:\\...\MultiCommentViewer\\plugins\\multiBatonOSC\\multiBatonOSCPlugin.dll  
5. Start the Multi Comment Viewer.  
   After starting the Multi Comment Viewer, a configuration file `multiBatonOSC.txt` will be automatically created in the `multiBatonOSC` folder.  
   No special actions are required from the user for the configuration file.  

## Uninstallation
1. Delete `multiBatonOSCPlugin.dll`.  
2. Delete `multiBatonOSC.txt`.  
3. Delete `multiBatonOSC` folder.  

## Usage
1. From the Multi Comment Viewer menu, select `Plugins` → `multiBatonOSC v0.0 Settings`.  
2. Choose the transfer mode and close the settings window. The new settings will take effect after closing the settings window.  

## Integration with VirtualCast
1. In the VirtualCast title screen, set the "OSC Receive Function" in the "VCI" section to either "creator-only" or "enabled". If you're unsure which to choose, select "creator-only" and check the results.  
   (Note: This plugin does not use the "OSC Send Function" of VirtualCast.)  
2. Obtain `Vertical Comment Viewer (OSC) VCV`, `(OSC) Show the Emotion`, and `(OSC) Comment Tags` from the [VirtualCast's products page](https://virtualcast.jp/users/100215#products).  

## Plugin Behavior
1. Sending Comments and Related Information  
   When a comment is received and the transfer mode is "1: Studio" or "2: Room", it is sent in the following format:  
   ```C#:Class1.cs
   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/comment", blob_comment, blob_name, str_commentSource, int_transferMode);  
   ```
2. Sending Transfer Mode  
   When the Multi-Comment Viewer starts, ends, or when the transfer mode changes, it is sent in the following format:  
   ```C#:Class1.cs
   UDPSender("127.0.0.1", 19100);  
   OscMessage("/vci/baton/mode", `int_transferMode`);  
   ```
## Argument Types and Examples
This section introduces the types and examples of arguments used in OSC messages.  
1. `/vci/baton/comment`, `/vci/baton/mode`  
   OSC addresses used to identify the destination and purpose of OSC messages.  

2. `blob_comment`  
   Comment (BlobAsUtf8 byte sequence representing a UTF-8 string)  
   - Edited for control character conversions, etc.  
   - If not obtainable or empty, "Unknown" or "No body text".  
  
3. `blob_name`  
   Username (BlobAsUtf8 byte array representing a UTF-8 string)
   - If a fixed handle name (nickname) is registered, it will be that nickname.  
   - If no nickname is registered and the username can be obtained, it will be the username; otherwise, "Unknown".  
   - If no name is available, "Nameless".  
   - niconico Live: For administrative comments, "Admin".  
   - niconico live: If no nickname is registered for 184, it will be the ID for 184.  
  
4. `str_commentSource`  
   Comment Source (String ASCII string)
   - niconico Live  
      - General comments: Nicolive  
      - 184 comments: Nicolive184  
      - Operator comments (default): NicoliveOperator  
      - Specific operator comments: NicoliveAd, NicoliveInfo, NicoliveGift, NicoliveSpi, NicoliveEmotion  
   - SHOWROOM  
     - All Comments: Showroom  
   - YouTube Live  
     - General Comment: Youtubelive  
     - Super Chat: YoutubeliveSuperchat  
     - Membership: YoutubeliveMembership  
   - TwitCasting  
     - General Comment: Twitcasting  
     - Gift: TwitcastingGift  
   - Twitch  
     - All Comments: Twitch  
   - Openrec  
     - General Comment: Openrec  
     - Stamp: OpenrecStamp  
     - Yell (Gift): OpenrecYell  
   - Whowatch  
     - General Comment: Whowatch  
     - Gift: WhowatchGift  

5. `int_transferMode`  
   Transfer Mode (Int32 32-bit integer)
   - At Multi Comment Viewer start, it will be the mode number saved in the configuration file ("0", "1", or "2").  
   - When changing transfer modes, it will be the new mode number.  
    0: Off  
    1: Studio (niconico Live: Operator comments) (SHOWROOM: Off) (YouTube Live, TwitCasting, etc.: All comments)  
    2: Room (All comments)  
   - On Multi Comment Viewer shutdown, it sends "-1" 

## Example `main.lua` for VCI
Receiving Comments  
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
This is an example script for receiving comments on the VCI side. The `exampleComment` function receives four arguments (comment, name, comment source, and transfer mode) via OSC. This example prints the comments to the debug console based on the comment source.  

Receiving Transfer Modes  
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
This is an example script for receiving transfer modes on the VCI side. The `exampleMode` function receives one argument (transfer mode) via OSC. This example prints messages to the debug console based on the transfer mode.  

For more details, please refer to the [VirtualCast Official Wiki: ExportOsc(外部との OSC 通信)](https://wiki.virtualcast.jp/wiki/vci/script/reference/exportosc)  

## License
This plugin is licensed under the GNU General Public License v3.0.  
