using Img_Share.Dialogs;
using OneDriveShareImage.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Tools;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Img_Share.Conrols
{
    /// <summary>
    /// 图片详情控件
    /// </summary>
    public sealed partial class ImageMaskControl : UserControl,INotifyPropertyChanged
    {
        public OneDriveImage imageFile;
        private string _fileName;
        private string _groupName;
        private string _link;
        public string GroupId;
        private string _rp;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName
        {
            get => _groupName;
            set { _groupName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 图片链接
        /// </summary>
        public string Link
        {
            get => _link;
            set { _link = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 分辨率
        /// </summary>
        public string RP
        {
            get => _rp;
            set { _rp = value;OnPropertyChanged(); }
        }

        private BitmapImage _holderImage;
        /// <summary>
        /// 占位图片
        /// </summary>
        public BitmapImage HolderImage
        {
            get => _holderImage;
            set { _holderImage = value; OnPropertyChanged(); }
        }

        //创建一个popup对象
        private Popup _popup = null;
        public ImageMaskControl()
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
            bool isDark = App.Current.RequestedTheme == ApplicationTheme.Dark;
            // 由于ImageEx的 CornerRadius 属性在1809以上才可用，故此需要进行一个判断
            bool is1809 = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
            if (is1809)
            {
                MyImageEx.CornerRadius = new CornerRadius(5);
            }
            if (isDark)
            {
                HolderImage = new BitmapImage(new Uri("ms-appx:///Assets/imgHolder_dark.png"));
            }
            else
            {
                HolderImage = new BitmapImage(new Uri("ms-appx:///Assets/imgHolder_light.png"));
            }
            ToolTipService.SetToolTip(CopytoMdButton, AppTools.GetReswLanguage("CopyToMarkdown"));
            ToolTipService.SetToolTip(CopytoHtmlButton, AppTools.GetReswLanguage("CopyToHTML"));
            //给当前的grid添加一个loaded事件，当使用了ShowAPopup()的时候，也就是弹窗显示了，这个弹窗的内容就是我们的grid，所以我们需要将动画打开了。
            this.Loaded += PopupNoticeLoaded;
        }
        /// <summary>
        /// 重载
        /// </summary>
        /// <param name="popupContentString">弹出框中的内容</param>
        public ImageMaskControl(OneDriveImage image) : this()
        {
            Init(image);
        }

        /// <summary>
        /// 对传入的图片对象进行解析
        /// </summary>
        /// <param name="image"></param>
        public void Init(OneDriveImage image)
        {
            imageFile = image;
            FileName = image.ImageName;
            GroupId = image.GroupId;
            Link = image.URL;
            GroupName = image.GroupName;
            RP = image.Width + " * " + image.Height;
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
            if (MainPage.Current != null)
            {
                this.PopupIn.Begin();
            }
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

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyImage("url");
        }

        private void CopytoMdButton_Click(object sender, RoutedEventArgs e)
        {
            CopyImage("md");
        }

        private void CopytoHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            CopyImage("img");
        }
        /// <summary>
        /// 复制不同类型的链接
        /// </summary>
        /// <param name="type">类型</param>
        private void CopyImage(string type)
        {
            string str = string.Empty;
            string tipKey = string.Empty;
            switch (type.ToLower())
            {
                case "url":
                    str= Link;
                    tipKey = "CopiedLink";
                    break;
                case "md":
                    str = $"![{imageFile.ImageName}]({Link})";
                    tipKey = "CopiedMd";
                    break;
                case "html":
                    str = $"<img src=\"{Link}\" title=\"{FileName}\" alt=\"{FileName}\" />";
                    tipKey = "CopiedImg";
                    break;
            }
            var dp = new DataPackage();
            dp.SetText(str);
            Clipboard.SetContent(dp);
            new PopupMaskTip(AppTools.GetReswLanguage(tipKey)).Show();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // 删除图片时，执行两个步骤
            // 1. 先删除OneDrive中的源文件
            // 2. 成功后删除数据库中记录的数据，并将数据库的状态设置为Change
            var tipDialog = new TipDialog(AppTools.GetReswLanguage("DeleteImageTipTitle"), AppTools.GetReswLanguage("DeleteImageTipContent"), true);
            var result=await tipDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var images = App.Db.Images.Where(p => p.ImageId == imageFile.ImageId);
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
                await App.Db.SaveChangesAsync();
                Close();
                waittingTip.Close();
                MainPage.Current.LastestInit();
                MainPage.Current.HistoryInit();
                AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
            }
        }

        /// <summary>
        /// 转移图片只需要修改数据库中的分组信息即可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            var groupDialog = new GroupDialog();
            var groupResult = await groupDialog.ShowAsync();
            if (groupResult == ContentDialogResult.Primary)
            {

                var selectGroup = groupDialog.SelectGroup;
                if (selectGroup.GroupId==GroupId)
                {
                    new PopupMaskTip(AppTools.GetReswLanguage("Sao")).Show();
                    return;
                }
                else
                {
                    foreach (var img in App.Db.Images)
                    {
                        if (img.ImageId == imageFile.ImageId)
                        {
                            img.GroupId = selectGroup.GroupId;
                            img.GroupName = selectGroup.GroupName;
                        }
                    }
                    await App.Db.SaveChangesAsync();
                    MainPage.Current.HistoryInit();
                    AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
                    GroupName = selectGroup.GroupName;
                    new PopupMaskTip(AppTools.GetReswLanguage("MoveImageSuccess")).Show();
                }
            }
        }

        /// <summary>
        /// 修改图片名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileName == imageFile.ImageName)
            {
                return;
            }
            // 图片名称必须包含扩展名
            string[] tempSp = FileName.Split(".");
            // 图片名称中不能有Windows不能识别的非法字符
            char[] notAllowChar = new char[] { '<', '>', '/', '\\', ':', '"', '*', '?' };
            string[] goodExtends = new string[] { "jpg", "jpeg", "png", "bmp", "gif", "webp" };
            if (tempSp.Length<2 || !goodExtends.Any(p => p == tempSp[tempSp.Length - 1].ToLower()))
            {
                new PopupMaskTip(AppTools.GetReswLanguage("FileNameErrorExtends")).Show();
                return;
            }
            else
            {
                bool isNotAllow = FileName.Any(p => notAllowChar.Any(s => s == p));
                if (isNotAllow)
                {
                    new PopupMaskTip(AppTools.GetReswLanguage("PrefixNotAllow")).Show();
                }
                else
                {
                    // 完成之后，修改OneDrive中保存的图片名以及数据库内的记录，并将数据库状态设置为Change
                    var tip = new HoldMaskTip(AppTools.GetReswLanguage("WaitToChange"));
                    tip.Show();
                    string name = await App.OneDriveTools.RenameImage(imageFile, FileName);
                    foreach (var item in App.Db.Images)
                    {
                        if (item.ImageName == imageFile.ImageName)
                        {
                            item.ImageName = name;
                            break;
                        }
                    }
                    await App.Db.SaveChangesAsync();
                    AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
                    FileName = name;
                    imageFile.ImageName = name;
                    tip.Close();
                    new PopupMaskTip(AppTools.GetReswLanguage("FileNameChangeSuccess")).Show();
                }
            }
        }
    }
}
