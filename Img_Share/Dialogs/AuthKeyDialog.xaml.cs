using Img_Share.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Img_Share.Dialogs
{
    /// <summary>
    /// 添加AuthKey
    /// </summary>
    public sealed partial class AuthKeyDialog : ContentDialog
    {
        public AuthKeyDialog()
        {
            this.InitializeComponent();
            Title = AppTools.GetReswLanguage("AuthorizeTitle");
            TipContentBlock.Text = AppTools.GetReswLanguage("AuthKeyTip");
            PrimaryButtonText = AppTools.GetReswLanguage("Authorize");
            CloseButtonText = AppTools.GetReswLanguage("Cancel");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void AuthKeyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            // 关于获取AuthKey的说明
            await Launcher.LaunchUriAsync(new Uri("https://blog.richasy.cn/document/basic/onedrive_authkey.html"));
        }
    }
}
