using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Tools
{
    public class AppTools
    {

        /// <summary>
        /// 写入本地设置
        /// </summary>
        /// <param name="key">设置名</param>
        /// <param name="value">设置值</param>
        public static void WriteLocalSetting(AppSettings key, string value)
        {
            var localSetting = ApplicationData.Current.LocalSettings;
            var localcontainer = localSetting.CreateContainer("Manager", ApplicationDataCreateDisposition.Always);
            localcontainer.Values[key.ToString()] = value;
        }
        /// <summary>
        /// 读取本地设置
        /// </summary>
        /// <param name="key">设置名</param>
        /// <returns></returns>
        public static string GetLocalSetting(AppSettings key, string defaultValue)
        {
            var localSetting = ApplicationData.Current.LocalSettings;
            var localcontainer = localSetting.CreateContainer("Manager", ApplicationDataCreateDisposition.Always);
            bool isKeyExist = localcontainer.Values.ContainsKey(key.ToString());
            if (isKeyExist)
            {
                return localcontainer.Values[key.ToString()].ToString();
            }
            else
            {
                WriteLocalSetting(key, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// 将 <see cref="StorageFile"/> 转化为 <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="file">获取的图片文件</param>
        /// <returns></returns>
        public async static Task<BitmapImage> ConvertFileToImage(StorageFile file)
        {
            var image = new BitmapImage();
            using (var stream = await file.OpenReadAsync())
            {
                await image.SetSourceAsync(stream);
            }
            return image;
        }

        /// <summary>
        /// 根据语言选项选择对应语言的语句
        /// </summary>
        /// <param name="name">键值</param>
        /// <returns></returns>
        public static string GetReswLanguage(string name)
        {

            var loader = ResourceLoader.GetForCurrentView();
            var language = loader.GetString(name);
            language = language.Replace("\\n", "\n");
            return language;
        }
        /// <summary>
        /// 设置标题栏的样式
        /// </summary>
        /// <param name="isDark">是否为Dark模式</param>
        public static void SetTitleBarColorInit(bool isDark)
        {
            var view = ApplicationView.GetForCurrentView();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            if (isDark)
            {
                // active
                view.TitleBar.BackgroundColor = Colors.Transparent;
                view.TitleBar.ForegroundColor = Colors.White;

                // inactive
                view.TitleBar.InactiveBackgroundColor = Colors.Transparent;
                view.TitleBar.InactiveForegroundColor = Colors.Gray;
                // button
                view.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                view.TitleBar.ButtonForegroundColor = Colors.White;

                view.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 33, 42, 67);
                view.TitleBar.ButtonHoverForegroundColor = Colors.White;

                view.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 255, 86, 86);
                view.TitleBar.ButtonPressedForegroundColor = Colors.White;

                view.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                view.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
            }
            else
            {
                // active
                view.TitleBar.BackgroundColor = Colors.Transparent;
                view.TitleBar.ForegroundColor = Colors.Black;

                // inactive
                view.TitleBar.InactiveBackgroundColor = Colors.Transparent;
                view.TitleBar.InactiveForegroundColor = Colors.Gray;
                // button
                view.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                view.TitleBar.ButtonForegroundColor = Colors.DarkGray;

                view.TitleBar.ButtonHoverBackgroundColor = Colors.LightGray;
                view.TitleBar.ButtonHoverForegroundColor = Colors.DarkGray;

                view.TitleBar.ButtonPressedBackgroundColor = Colors.DarkGray;
                view.TitleBar.ButtonPressedForegroundColor = Colors.White;

                view.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                view.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
            }
        }

        /// <summary>
        /// 获取当前指定的父控件
        /// </summary>
        /// <typeparam name="T">转换类型</typeparam>
        /// <param name="obj">控件</param>
        /// <param name="name">父控件名</param>
        /// <returns></returns>
        public static T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)parent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
        /// <summary>
        /// 获取当前控件的指定子控件
        /// </summary>
        /// <typeparam name="T">控件类型</typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="name">子控件名</param>
        /// <returns></returns>
        public static T GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            T grandChild = null;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChildObject<T>(child, name);
                }
                if (grandChild != null)
                {
                    return grandChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 标准化字符串，去掉空格，全部小写
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string NormalString(string str)
        {
            str = str.ToLower();
            var reg = new Regex(@"\s", RegexOptions.IgnoreCase);
            str = reg.Replace(str, "");
            return str;
        }

        /// <summary>
        /// 后台任务注册
        /// </summary>
        /// <param name="isTaskReg">后台任务是否注册</param>
        /// <param name="isCanReg">后台任务注册许可</param>
        /// <param name="TaskName">后台任务名</param>
        /// <param name="EntryPoint">后台任务入口</param>
        /// <param name="trigger">后台任务触发条件</param>
        public static void BackgroundTaskReg(bool isTaskReg, bool isCanReg, string TaskName, string EntryPoint, TimeTrigger trigger)
        {
            if (!isTaskReg && isCanReg)
            {
                var builder = new BackgroundTaskBuilder
                {
                    Name = TaskName,
                    TaskEntryPoint = EntryPoint
                };
                builder.SetTrigger(trigger);
                BackgroundTaskRegistration task = builder.Register();
            }
            else if (!isCanReg)
            {
                foreach (var item in BackgroundTaskRegistration.AllTasks)
                {
                    if (item.Value.Name == TaskName)
                    {
                        item.Value.Unregister(true);
                    }
                }
            }
        }
    }
}
