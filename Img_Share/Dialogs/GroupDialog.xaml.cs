using OneDriveShareImage.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// 用来选择分组的一个弹出框
    /// </summary>
    public sealed partial class GroupDialog : ContentDialog
    {
        private ObservableCollection<OneDriveImageGroup> GroupCollection = new ObservableCollection<OneDriveImageGroup>();
        public OneDriveImageGroup SelectGroup = null;
        private bool isInit = false;
        public GroupDialog()
        {
            this.InitializeComponent();
            Title = AppTools.GetReswLanguage("GroupDialogTitle");
            PrimaryButtonText = AppTools.GetReswLanguage("OK");
            SecondaryButtonText = AppTools.GetReswLanguage("Cancel");
            var group = App.Db.Groups;
            foreach (var item in group)
            {
                GroupCollection.Add(item);
            }
            string selectId = AppTools.GetLocalSetting(AppSettings.SelectGroupIndex, "");
            foreach (var item in GroupCollection)
            {
                if (item.GroupId == selectId)
                {
                    GroupCombo.SelectedItem = item;
                    SelectGroup = item;
                }
            }
            if (SelectGroup == null)
            {
                GroupCombo.SelectedItem = GroupCollection.First();
                SelectGroup = GroupCollection.First();
            }
            isInit = true;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void GroupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
            {
                var item = (OneDriveImageGroup)GroupCombo.SelectedItem;
                SelectGroup = item;
            }
            
        }
    }
}
