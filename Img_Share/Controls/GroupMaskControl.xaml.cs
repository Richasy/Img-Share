using Img_Share.Dialogs;
using OneDriveShareImage.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Img_Share.Controls
{
    public sealed partial class GroupMaskControl : UserControl,INotifyPropertyChanged
    {

        private ObservableCollection<OneDriveImageGroup> GroupCollection = new ObservableCollection<OneDriveImageGroup>();
        //创建一个popup对象
        private Popup _popup = null;
        public GroupMaskControl()
        {
            this.InitializeComponent();
            //将当前的长和框 赋值给控件
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            var groups = App.Db.Groups;
            foreach (var item in groups)
            {
                GroupCollection.Add(item);
            }
            //将当前的控价赋值给弹窗的Child属性  Child属性是弹窗需要显示的内容 当前的this是一个Grid控件。
            _popup = new Popup();
            _popup.Child = this;
            //给当前的grid添加一个loaded事件，当使用了ShowAPopup()的时候，也就是弹窗显示了，这个弹窗的内容就是我们的grid，所以我们需要将动画打开了。
            this.Loaded += PopupNoticeLoaded;
        }


        /// <summary>
        /// 显示一个popup弹窗 当需要显示一个弹窗时，执行此方法
        /// </summary>
        public void Show()
        {
            _popup.IsOpen = true;
            MainPage.Current.AddMaskInList(this);
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

        private async void ChangeNameButton_Click(object sender, RoutedEventArgs e)
        {
            
            var con = AppTools.GetParentObject<Grid>((FrameworkElement)sender, "GroupItemContainer");
            var idCon = AppTools.GetChildObject<TextBlock>(con, "ItemId");
            var nameCon = AppTools.GetChildObject<TextBox>(con, "GroupItemNameBox");
            string id = idCon.Text;
            string name = nameCon.Text.Trim();
            if (!string.IsNullOrEmpty(id))
            {
                // 按钮点击后，先确认新名称是否为空
                if (string.IsNullOrEmpty(name))
                {
                    new PopupMaskTip(AppTools.GetReswLanguage("GroupNameEmpty")).Show();
                    return;
                }
                var item = App.Db.Groups.Where(p => p.GroupId == id).ToList()[0];
                // 再确认新名称是否和当前名称相同，相同就没有改名必要了
                if (item.GroupName != name)
                {
                    // 再判断新名称是否与其他组名重复，重复的话就弹出提示，不同的话就进行名称修改
                    bool isIn = App.Db.Groups.Any(p => p.GroupName.ToLower() == name.ToLower());
                    if (isIn)
                    {
                        new PopupMaskTip(AppTools.GetReswLanguage("GroupNameRepeat")).Show();
                    }
                    else
                    {
                        item.GroupName = name;
                        await App.Db.SaveChangesAsync();
                        AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
                        new PopupMaskTip(AppTools.GetReswLanguage("GroupRenameSuccess")).Show();
                        MainPage.Current.GroupCollectionReInit();
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private async void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            // 移除当前组要注意组内是否还有其他的图片，有的话要注意转移
            int GroupCount = GroupCollection.Count;
            if (GroupCount <= 1)
            {
                new PopupMaskTip(AppTools.GetReswLanguage("OnlyOneGroup")).Show();
                return;
            }
            var con = AppTools.GetParentObject<Grid>((FrameworkElement)sender, "GroupItemContainer");
            var idCon = AppTools.GetChildObject<TextBlock>(con, "ItemId");
            string id = idCon.Text;
            if (!string.IsNullOrEmpty(id))
            {
                var item = App.Db.Groups.Where(p => p.GroupId == id).ToList()[0];
                var deleteDialog = new TipDialog(AppTools.GetReswLanguage("DeleteGroupTitle"), AppTools.GetReswLanguage("DeleteGroupTip"), AppTools.GetReswLanguage("MoveImage"), AppTools.GetReswLanguage("Delete"));
                var chooseResult = await deleteDialog.ShowAsync();
                if (chooseResult == ContentDialogResult.Primary)
                {
                    // 转移图片
                    int count = App.Db.Images.Where(p => p.GroupId == item.GroupId).Count();
                    if (count > 0)
                    {
                        var groupDialog = new GroupDialog();
                        var groupResult = await groupDialog.ShowAsync();
                        if (groupResult == ContentDialogResult.Primary)
                        {

                            var selectGroup = groupDialog.SelectGroup;
                            if (selectGroup.Equals(item))
                            {
                                new PopupMaskTip(AppTools.GetReswLanguage("Sao")).Show();
                                return;
                            }
                            else
                            {
                                foreach (var img in App.Db.Images)
                                {
                                    if (img.GroupId == item.GroupId)
                                    {
                                        img.GroupId = selectGroup.GroupId;
                                        img.GroupName = selectGroup.GroupName;
                                    }
                                }
                                App.Db.Groups.Remove(item);
                                await App.Db.SaveChangesAsync();
                                GroupCollection.Remove(item);
                                MainPage.Current.GroupCollectionRemoved(item);
                                MainPage.Current.HistoryInit();
                                AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
                                new PopupMaskTip(AppTools.GetReswLanguage("MoveImageSuccess")).Show();
                                await Task.Delay(800);
                                new PopupMaskTip(AppTools.GetReswLanguage("DeleteGroupSuccess")).Show();
                            }
                        }
                    }
                    else
                    {
                        App.Db.Groups.Remove(item);
                        await App.Db.SaveChangesAsync();
                        new PopupMaskTip(AppTools.GetReswLanguage("DeleteGroupSuccess")).Show();
                    }
                }
                else if (chooseResult == ContentDialogResult.Secondary)
                {
                    // 直接删除分组
                    var images = App.Db.Images.Where(p => p.GroupId == item.GroupId);
                    var waittingTip = new HoldMaskTip(AppTools.GetReswLanguage("Deleting"));
                    waittingTip.Show();
                    int count = 0;
                    if (images.Count() > 0)
                    {
                        count = await App.OneDriveTools.DeleteImage(images.ToList());
                    }
                    if (count > 0)
                    {
                        App.Db.Images.RemoveRange(images);
                        string msg = AppTools.GetReswLanguage("DeleteImageSuccess").Replace("{count}", count.ToString());
                        new PopupMaskTip(msg).Show();
                    }
                    App.Db.Groups.Remove(item);
                    await App.Db.SaveChangesAsync();
                    waittingTip.Close();
                    GroupCollection.Remove(item);
                    MainPage.Current.GroupCollectionRemoved(item);
                    MainPage.Current.LastestInit();
                    MainPage.Current.HistoryInit();
                    AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
                    await Task.Delay(800);
                    new PopupMaskTip(AppTools.GetReswLanguage("DeleteGroupSuccess")).Show();
                }
            }
        }
    }
}

