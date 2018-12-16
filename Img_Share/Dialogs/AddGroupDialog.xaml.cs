using Img_Share.Controls;
using OneDriveShareImage.Model;
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
    /// 添加新组
    /// </summary>
    public sealed partial class AddGroupDialog : ContentDialog
    {
        public AddGroupDialog()
        {
            this.InitializeComponent();
            Title = AppTools.GetReswLanguage("AddGroupTitle");
            PrimaryButtonText = AppTools.GetReswLanguage("OK");
            SecondaryButtonText = AppTools.GetReswLanguage("Cancel");
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // 需要检查组名是否为空（或者只包含空格）
            // 检查组名是否重复

            args.Cancel = true;
            string groupName = GroupInputBox.Text.Trim();
            if (!string.IsNullOrEmpty(groupName))
            {
                bool isIn = App.Db.Groups.Any(p => p.GroupName.ToLower() == groupName.ToLower());
                if (!isIn)
                {
                    var group = new OneDriveImageGroup(groupName);
                    await App.OneDriveTools.AddGroupToDatabase(App.Db, group);
                    new PopupMaskTip(AppTools.GetReswLanguage("AddGroupSuccess")).Show();
                    MainPage.Current.GroupCollectionAdd(group);
                    this.Hide();
                    return;
                }
                else
                {
                    new PopupMaskTip(AppTools.GetReswLanguage("GroupNameRepeat")).Show();
                }
            }
            else
            {
                new PopupMaskTip(AppTools.GetReswLanguage("GroupNameEmpty")).Show();
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
