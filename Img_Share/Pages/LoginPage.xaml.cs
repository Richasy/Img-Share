using Img_Share.Controls;
using Img_Share.Dialogs;
using System;
using System.Linq;
using Tools;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Img_Share.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            AppTools.SetTitleBarColorInit(App.Current.RequestedTheme == ApplicationTheme.Dark);
            ToolTipService.SetToolTip(OpenSourceButton, AppTools.GetReswLanguage("OpenSourceButton"));
            ToolTipService.SetToolTip(UseInfoButton, AppTools.GetReswLanguage("UseInfoButton"));
        }

        private async void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            var tip = new HoldMaskTip(AppTools.GetReswLanguage("WaittingTip"));
            tip.Show();
            // 开始走OneDrive登录流程
            bool isAuth = await App.OneDriveTools.OneDriveAuthorize();
            
            // 如果用户成功登录并同意授权
            if (isAuth)
            {
                AppTools.WriteLocalSetting(AppSettings.IsOneDriveAuthorized, "True");
                // 检查用户云端内是否有图片存储
                bool hasCloudMeta = await App.OneDriveTools.EnsureCloudMetaExist();
                if (hasCloudMeta)
                {
                    // 有的话进行同步
                    await App.OneDriveTools.ReplaceDatabase();
                    App.Db = await App.OneDriveTools.GetImgMetaList();
                    using (App.Db)
                    {
                        if (App.Db.Images.Count() > 0)
                        {
                            string url = App.Db.Images.First().URL;
                            int authKeyIndex = url.IndexOf("key=");
                            string authkey = url.Substring(authKeyIndex + 4);
                            AppTools.WriteLocalSetting(AppSettings.AuthKey, authkey);
                        }
                    }
                }
                else
                {
                    // 没有就在应用文件夹里建一个
                    await App.OneDriveTools.DatabaseInit();
                    
                }
                tip.Close();

                // 要求用户输入授权码
                string ak = AppTools.GetLocalSetting(AppSettings.AuthKey, "");
                if (string.IsNullOrEmpty(ak))
                {
                    // 弹出授权码输入框
                    bool isCancel = false;
                    var authDialog = new AuthKeyDialog();
                    authDialog.PrimaryButtonClick += (_s, _e) =>
                    {
                        _e.Cancel = true;
                        authDialog.IsPrimaryButtonEnabled = false;
                        string key = authDialog.AuthKeyBox.Text.Trim();
                        if (string.IsNullOrEmpty(key))
                        {
                            var msg = AppTools.GetReswLanguage("AuthKeyEmpty");
                            new PopupMaskTip(msg).Show();
                        }
                        else
                        {
                            AppTools.WriteLocalSetting(AppSettings.AuthKey, key);
                            authDialog.Hide();
                        }
                        authDialog.IsPrimaryButtonEnabled = true;
                    };
                    authDialog.CloseButtonClick += (_s, _e) =>
                    {
                        isCancel = true;
                        AppTools.WriteLocalSetting(AppSettings.AuthKey, "");
                    };
                    await authDialog.ShowAsync();
                    if (!isCancel)
                    {
                        var tipDialog = new TipDialog(AppTools.GetReswLanguage("DefaultTipTitle"), AppTools.GetReswLanguage("AuthorizeSuccess"));
                        await tipDialog.ShowAsync();
                    }
                    else
                    {
                        var tipDialog = new TipDialog(AppTools.GetReswLanguage("DefaultTipTitle"), AppTools.GetReswLanguage("AuthorizeCancel"));
                        await tipDialog.ShowAsync();
                    }
                }
                // 跳转至首页
                Frame rootFrame = Window.Current.Content as Frame;
                AppTools.WriteLocalSetting(AppSettings.IsFirstRun, "False");
                rootFrame.Navigate(typeof(MainPage));
            }
            else
            {
                tip.Close();
                string title = AppTools.GetReswLanguage("AuthErrorTitle");
                string content = AppTools.GetReswLanguage("AuthErrorContent");
                var dialog = new TipDialog(title, content);
                await dialog.ShowAsync();
            }
            
        }

        private async void OpenSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // 开源地址
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Richasy/Img-Share"));
        }

        private async void UseInfoButton_Click(object sender, RoutedEventArgs e)
        {
            // 使用说明
            await Launcher.LaunchUriAsync(new Uri("https://blog.richasy.cn/document/imgshare/"));
        }
    }
}
