using OneDriveShareImage.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Tools;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Img_Share.Controls
{
    /// <summary>
    /// 这是主要的图片显示控件，对ImageEx进行了二次封装
    /// </summary>
    public sealed partial class MyImage : UserControl,INotifyPropertyChanged
    {
        private string _imageLink;
        /// <summary>
        /// 图片链接
        /// </summary>
        public string ImageLink
        {
            get => _imageLink;
            set { _imageLink = value; OnPropertyChanged(); }
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
        public MyImage()
        {
            this.InitializeComponent();
            // ImageEx的CornerRadius属性仅在1809及以上可用
            bool is1809 = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
            if (is1809)
            {
                ImageBlock.CornerRadius = new CornerRadius(10);
            }
            bool isDark = App.Current.RequestedTheme == ApplicationTheme.Dark;
            if (isDark)
            {
                HolderImage = new BitmapImage(new Uri("ms-appx:///Assets/imgHolder_dark.png"));
            }
            else
            {
                HolderImage = new BitmapImage(new Uri("ms-appx:///Assets/imgHolder_light.png"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 图片点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageBlockMouseDown.Begin();
            ImageBlockMouseDown.Completed += ((_s, _e) => { ImageBlockMouseUp.Begin(); });
            var imgTemp = App.Db.Images.Where(p => p.URL == ImageLink).ToList();
            var img = new OneDriveImage();
            if (imgTemp.Count > 0)
            {
                img = imgTemp[0];
            }
            else
            {
                // 这里是为了检测图片链接的准确性进行的异常捕获
                img = null;
                new PopupMaskTip(AppTools.GetReswLanguage("NotExist")).Show();
            }
            if (img != null)
            {
                // 这里加入了快捷复制的功能，即按住某个键点击图片即可在不显示图片详情时直接复制链接
                var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                var alt = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && !shift.HasFlag(CoreVirtualKeyStates.Down) && !alt.HasFlag(CoreVirtualKeyStates.Down))
                {
                    // ctrl
                    // 直接复制连接
                    CopyImage(img, "url");
                }
                else if (!ctrl.HasFlag(CoreVirtualKeyStates.Down) && shift.HasFlag(CoreVirtualKeyStates.Down) && !alt.HasFlag(CoreVirtualKeyStates.Down))
                {
                    // shift
                    // 直接复制Markdown连接
                    CopyImage(img, "md");
                }
                else if (!ctrl.HasFlag(CoreVirtualKeyStates.Down) && !shift.HasFlag(CoreVirtualKeyStates.Down) && alt.HasFlag(CoreVirtualKeyStates.Down))
                {
                    // alt
                    // 复制img标签
                    CopyImage(img, "img");
                }
                else
                {
                    var mask = new ImageMaskControl(imgTemp[0]);
                    mask.Show();
                }
            }

            
        }

        /// <summary>
        /// 复制不同类型的链接
        /// </summary>
        /// <param name="type">类型</param>
        private void CopyImage(OneDriveImage img, string type)
        {
            string str = string.Empty;
            string tipKey = string.Empty;
            switch (type.ToLower())
            {
                case "url":
                    str = img.URL;
                    tipKey = "CopiedLink";
                    break;
                case "md":
                    str = $"![{img.ImageName}]({img.URL})";
                    tipKey = "CopiedMd";
                    break;
                case "html":
                    str = $"<img src=\"{img.URL}\" title=\"{img.ImageName}\" alt=\"{img.ImageName}\" />";
                    tipKey = "CopiedImg";
                    break;
            }
            var dp = new DataPackage();
            dp.SetText(str);
            Clipboard.SetContent(dp);
            new PopupMaskTip(AppTools.GetReswLanguage(tipKey)).Show();
        }
    }
}
