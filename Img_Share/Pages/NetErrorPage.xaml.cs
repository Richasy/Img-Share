using Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            AppTools.SetTitleBarColorInit(App.Current.RequestedTheme == ApplicationTheme.Dark);
        }
    }
}
