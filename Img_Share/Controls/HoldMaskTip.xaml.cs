using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// <summary>
    /// 做一个等待提醒
    /// </summary>
    public sealed partial class HoldMaskTip : UserControl
    {
        //存放弹出框中的信息
        private string _popupContent;

        //创建一个popup对象
        private Popup _popup = null;
        public HoldMaskTip()
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
        }
        /// <summary>
        /// 重载
        /// </summary>
        /// <param name="popupContentString">弹出框中的内容</param>
        public HoldMaskTip(string popupContentString) : this()
        {
            _popupContent = popupContentString;
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
            PopupContent.Text = _popupContent;

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
    }
}
