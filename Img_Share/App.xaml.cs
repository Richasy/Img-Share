
using Img_Share.Controls;
using Microsoft.Toolkit.Uwp.Connectivity;
using OneDriveShareImage;
using OneDriveShareImage.Model;
using System;
using Tools;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Img_Share
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        
        // 建立一个OneDrive工具的全局对象
        public static ShareImageTools OneDriveTools = new ShareImageTools();
        // 建立一个图片数据库的全局对象
        public static ImageDbContext Db = null;
        public App()
        {
            SwitchLanguage();
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += OnUnhandleException;
        }

        private void OnUnhandleException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            string msg = e.Message;
            new PopupMaskTip(msg).Show();
        }

        /// <summary>
        /// 检测系统语言，非中文语言一律切换为英文
        /// </summary>
        private void SwitchLanguage()
        {
            var Languages = Windows.System.UserProfile.GlobalizationPreferences.Languages;
            if (Languages.Count > 0)
            {
                var language = Languages[0];
                if (language.ToLower().IndexOf("zh") != -1)
                {
                    ApplicationLanguages.PrimaryLanguageOverride = "zh-CN";
                }
                else
                {
                    ApplicationLanguages.PrimaryLanguageOverride = "en-US";
                }
            }
            else
            {
                ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            }
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        rootFrame.Navigate(typeof(Pages.NetErrorPage));
                    }
                    else
                    {
                        // 如果用户是第一次登录，那么切换到登录页
                        bool isFirst = Convert.ToBoolean(AppTools.GetLocalSetting(AppSettings.IsFirstRun, "True"));

                        if (isFirst)
                        {
                            rootFrame.Navigate(typeof(Pages.LoginPage));
                        }
                        else
                        {
                            rootFrame.Navigate(typeof(MainPage), e.Arguments);
                        }
                    }
                    
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
