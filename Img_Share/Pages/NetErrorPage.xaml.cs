using System;
using Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Img_Share.Pages
{
    /// <summary>
    /// 网络异常时显示
    /// </summary>
    public sealed partial class NetErrorPage : Page
    {
        public NetErrorPage()
        {
            this.InitializeComponent();
            string theme = App.Current.RequestedTheme.ToString();
            var image = new BitmapImage();
            image.UriSource = new Uri($"ms-appx:///Assets/{theme}.png");
            AppIcon.Source = image;
            AppTools.SetTitleBarColorInit(App.Current.RequestedTheme == ApplicationTheme.Dark);
        }
    }
}
