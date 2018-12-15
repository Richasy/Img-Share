using Img_Share.Dialogs;
using OneDriveShareImage.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Img_Share.Controls
{
    public sealed partial class SettingControl : UserControl,INotifyPropertyChanged
    {
        private string _authKey;

        public string AuthKey
        {
            get => _authKey;
            set
            {
                _authKey = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<OneDriveImageGroup> GroupCollection = new ObservableCollection<OneDriveImageGroup>();
        
        private Popup _popup = null;
        public SettingControl()
        {
            this.InitializeComponent();
            //将当前的长和框 赋值给控件
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            //将当前的控价赋值给弹窗的Child属性  Child属性是弹窗需要显示的内容 当前的this是一个Grid控件。
            _popup = new Popup();
            _popup.Child = this;
            //给当前的grid添加一个loaded事件，当使用了ShowAPopup()的时候，也就是弹窗显示了，这个弹窗的内容就是我们的grid，所以我们需要将动画打开了。
            this.Loaded += PopupNoticeLoaded;
            AuthKey = AppTools.GetLocalSetting(AppSettings.AuthKey, "");
            var group = App.Db.Groups;
            foreach (var item in group)
            {
                GroupCollection.Add(item);
            }
            GroupNameCombo.SelectedItem = GroupCollection.First();
            ToolTipService.SetToolTip(OpenSourceButton, AppTools.GetReswLanguage("OpenSourceButton"));
            ToolTipService.SetToolTip(UseInfoButton, AppTools.GetReswLanguage("UseInfoButton"));
        }


        /// <summary>
        /// 显示一个popup弹窗 当需要显示一个弹窗时，执行此方法
        /// </summary>
        public void Show()
        {
            _popup.IsOpen = true;
            if (MainPage.Current != null)
            {
                MainPage.Current.AddMaskInList(this);
            }
        }


        /// <summary>
        /// 弹窗加载好了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PopupNoticeLoaded(object sender, RoutedEventArgs e)
        {

            //打开动画
            this.PopupIn.Begin();
        }


        /// <summary>
        /// 当进入动画完成后 到此
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Close()
        {
            //将消失动画打开
            this.PopupOut.Begin();
            if (MainPage.Current != null)
            {
                MainPage.Current.RemoveMaskFromList(this);
            }
            //popout 动画完成后 触发
            this.PopupOut.Completed += PopupOutCompleted;
        }


        //弹窗退出动画结束 代表整个过程结束 将弹窗关闭
        public void PopupOutCompleted(object sender, object e)
        {
            _popup.IsOpen = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AuthKeyTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ModifyAuthKey();
            }
        }

        private void EnSureAuthKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ModifyAuthKey();
        }

        /// <summary>
        /// 更改授权码
        /// </summary>
        private async void ModifyAuthKey()
        {
            string oldKey = AppTools.GetLocalSetting(AppSettings.AuthKey, "");
            string newKey = AuthKey.Trim();
            if (string.IsNullOrEmpty(newKey))
            {
                new PopupMaskTip(AppTools.GetReswLanguage("AuthKeyEmpty")).Show();
                return;
            }
            if (oldKey == newKey)
            {
                return;
            }
            else
            {
                if (App.Db.Images.Count() > 0)
                {
                    // 弹出警告，询问是否对数据库内所有条目进行修改
                    var warning = new TipDialog(AppTools.GetReswLanguage("AuthorizeChangeTitle"), AppTools.GetReswLanguage("AuthorizeChangeTip"),AppTools.GetReswLanguage("Yes"),AppTools.GetReswLanguage("No"));
                    var result = await warning.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        foreach (var item in App.Db.Images)
                        {
                            // 这里是修改链接内的AuthKey
                            int index = item.URL.LastIndexOf("key=")+4;
                            item.URL = item.URL.Replace(item.URL.Substring(index), newKey);
                        }
                        await App.Db.SaveChangesAsync();
                        AppTools.WriteLocalSetting(AppSettings.AuthKey, newKey);
                        MainPage.Current.HistoryInit();
                        MainPage.Current.LastestInit();
                        new PopupMaskTip(AppTools.GetReswLanguage("AuthorizeChangeSuccess")).Show();
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        AppTools.WriteLocalSetting(AppSettings.AuthKey, newKey);
                        new PopupMaskTip(AppTools.GetReswLanguage("AuthorizeChangeSuccess")).Show();
                    }
                }
                else
                {
                    AppTools.WriteLocalSetting(AppSettings.AuthKey, newKey);
                    new PopupMaskTip(AppTools.GetReswLanguage("AuthorizeChangeSuccess")).Show();
                }
            }
        }

        private void GroupNameCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 自动为组填充前缀
            var item = (OneDriveImageGroup)GroupNameCombo.SelectedItem;
            string name = item.GroupName;
            PrefixTextBox.Text = name + "-";
        }

        /// <summary>
        /// 为组内所有图片批量添加前缀
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddPrefixButton_Click(object sender, RoutedEventArgs e)
        {
            // 这个方法会将组内的所有图片全部加上统一的前缀，即便这个图片有前缀了，照加不误，因为没有做判断
            string prefix = PrefixTextBox.Text;
            char[] notAllowChar = new char[] { '<', '>', '/', '\\', ':', '"', '*', '?' };
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrWhiteSpace(prefix))
            {
                new PopupMaskTip(AppTools.GetReswLanguage("PrefixEmpty")).Show();
            }
            else
            {
                bool isNotAllow = prefix.Any(p => notAllowChar.Any(s => s == p));
                if (isNotAllow)
                {
                    new PopupMaskTip(AppTools.GetReswLanguage("PrefixNotAllow")).Show();
                }
                else
                {
                    var item = (OneDriveImageGroup)GroupNameCombo.SelectedItem;
                    var images = App.Db.Images.Where(p => p.GroupId == item.GroupId).ToList();
                    // 按顺序为每张图片执行改名过程
                    // 先将OneDrive内的图片源文件进行改名
                    // 然后再修改数据库内的内容，并将数据库的状态设置为Change

                    // Tip. 图片名称的修改不会影响到链接，因为链接中包含的是图片在OneDrive内的资源ID，这个是唯一的，且不可修改
                    if (images.Count > 0)
                    {
                        string tipMsg = AppTools.GetReswLanguage("AddPrefixTipContent").Replace("{group}", item.GroupName).Replace("{count}", images.Count.ToString()).Replace("{prefix}", prefix);
                        var tipDialog = new TipDialog(AppTools.GetReswLanguage("AddPrefixTipTitle"), tipMsg, true);
                        var result = await tipDialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            var tip = new HoldMaskTip(AppTools.GetReswLanguage("WaitToChange"));
                            tip.Show();
                            int count = await App.OneDriveTools.RenameImage(images, prefix);
                            if (count > 0)
                            {
                                foreach (var img in App.Db.Images)
                                {
                                    img.ImageName = prefix + img.ImageName;
                                }
                                await App.Db.SaveChangesAsync();
                                AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
                                new PopupMaskTip(AppTools.GetReswLanguage("FileNameChangeSuccess")).Show();
                            }
                            tip.Close();
                        }
                    }
                    else
                    {
                        string msg = AppTools.GetReswLanguage("NoImageTip");
                        new PopupMaskTip(msg).Show();
                    }
                }
            }
            
        }

        private async void OpenSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // 开源地址
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Richasy/Img-Share"));
        }

        private async void UseInfoButton_Click(object sender, RoutedEventArgs e)
        {
            // 应用说明书
            await Launcher.LaunchUriAsync(new Uri("https://blog.richasy.cn/document/imgshare/"));
        }
    }
}
