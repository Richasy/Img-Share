using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// 显示提示内容的弹出框
    /// </summary>
    public sealed partial class TipDialog : ContentDialog
    {
        public TipDialog()
        {
            this.InitializeComponent();
        }

        public TipDialog(string title,string tip,bool hasPrimary = false):this()
        {
            Title = title;
            TipContentBlock.Text = tip;
            if (hasPrimary)
            {
                PrimaryButtonText = AppTools.GetReswLanguage("OK");
                CloseButtonText = AppTools.GetReswLanguage("Cancel");
            }
            else
            {
                CloseButtonText = AppTools.GetReswLanguage("OK");
            }
        }
        public TipDialog(string title, string tip, string pr,string sec) : this()
        {
            Title = title;
            TipContentBlock.Text = tip;
            PrimaryButtonText = pr;
            SecondaryButtonText = sec;
            CloseButtonText = AppTools.GetReswLanguage("Cancel");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
