using Img_Share.Controls;
using Img_Share.Dialogs;
using Img_Share.Model;
using Microsoft.Toolkit.Uwp.Helpers;
using OneDriveShareImage.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tools;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Img_Share
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page,INotifyPropertyChanged
    {
        private double _searchBackgroundWidth;
        private double _searchBoxWidth;
        private double _uploadAreaHeight;
        private double _uploadInfoWidth;
        private double _showAreaWidth;
        private double _uploadProgressWidth;
        /// <summary>
        /// 在页面宽度变化时，控制最近上传列表的显示数量
        /// </summary>
        private int LastestImgCount;
        /// <summary>
        /// 搜索框背景宽度
        /// </summary>
        private double SearchBackgroundWidth
        {
            get { return _searchBackgroundWidth; }
            set { _searchBackgroundWidth = value;OnPropertyChanged(); }
        }
        /// <summary>
        /// 搜索框宽度
        /// </summary>
        private double SearchBoxWidth
        {
            get { return _searchBoxWidth; }
            set { _searchBoxWidth = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 上传区域高度
        /// </summary>
        private double UploadAreaHeight
        {
            get { return _uploadAreaHeight; }
            set { _uploadAreaHeight = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 单个上传信息的宽度
        /// </summary>
        public double UploadInfoWidth
        {
            get { return _uploadInfoWidth; }
            set { _uploadInfoWidth = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 上传进度条的宽度
        /// </summary>
        public double UploadProgressWidth
        {
            get { return _uploadProgressWidth; }
            set { _uploadProgressWidth = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 显示区域的宽度（为了给滚动条挪位置）
        /// </summary>
        public double ShowAreaWidth
        {
            get { return _showAreaWidth; }
            set { _showAreaWidth = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 控制加载完成的变量
        /// </summary>
        private bool isInit = false;

        /// <summary>
        /// 上传图片信息集合
        /// </summary>
        private ObservableCollection<KeyValue> UploadInfoCollection = new ObservableCollection<KeyValue>();
        /// <summary>
        /// 上传进度条集合
        /// </summary>
        private ObservableCollection<ProgressStatus> UploadProgressCollection = new ObservableCollection<ProgressStatus>();

        /// <summary>
        /// 最近上传图片集合
        /// </summary>
        private ObservableCollection<OneDriveImage> LastestImageCollection = new ObservableCollection<OneDriveImage>();
        /// <summary>
        /// 组内历史图片集合
        /// </summary>
        private ObservableCollection<OneDriveImage> HistoryImageCollection = new ObservableCollection<OneDriveImage>();
        /// <summary>
        /// 分组集合
        /// </summary>
        private ObservableCollection<OneDriveImageGroup> GroupCollection = new ObservableCollection<OneDriveImageGroup>();

        /// <summary>
        /// 搜索结果集合
        /// </summary>
        private ObservableCollection<OneDriveImage> SearchResultCollection = new ObservableCollection<OneDriveImage>();

        /// <summary>
        /// 数据库同步计时器
        /// </summary>
        private DispatcherTimer AutoUpdateTimer = new DispatcherTimer();
        
        /// <summary>
        /// 确认最近上传列表是否加载完成
        /// </summary>
        private bool IsLastImagesInit = false;
        /// <summary>
        /// 确认分组历史列表是否加载完成
        /// </summary>
        private bool IsHistoryImageInit = false;

        /// <summary>
        /// 等待提醒
        /// </summary>
        private HoldMaskTip WaittingTip = new HoldMaskTip(AppTools.GetReswLanguage("WaittingTip"));

        /// <summary>
        /// 弹出层集合
        /// </summary>
        public List<UserControl> PopupUserControlList = new List<UserControl>();

        /// <summary>
        /// <see cref="MainPage"/> 的实例
        /// </summary>
        public static MainPage Current;
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            MainPageInit();
        }

        /// <summary>
        /// MainPage的初始化工作
        /// </summary>
        private async void MainPageInit()
        {
            // 设置标题栏样式，为上传信息加载默认数据，显示等待提示
            AppTools.SetTitleBarColorInit(App.Current.RequestedTheme == ApplicationTheme.Dark);
            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("DefaultUploadInfoTitle"), AppTools.GetReswLanguage("None")));
            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("ImageSize"), AppTools.GetReswLanguage("None")));
            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("ImageType"), AppTools.GetReswLanguage("None")));
            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("CreateDate"), AppTools.GetReswLanguage("None")));
            string theme = App.Current.RequestedTheme.ToString();
            var image = new BitmapImage();
            image.UriSource = new Uri($"ms-appx:///Assets/{theme}.png");
            AppIcon.Source = image;
            UploadIcon.Source = image;
            //WaittingTip.Show();
            OneDriveInit();
            // 建立数据库连接
            try
            {
                App.Db = new ImageDbContext();
                var test = App.Db.Images.Where(p => p.Id == 1);
            }
            catch (Exception)
            {
                App.Db = await App.OneDriveTools.GetImgMetaList();
            }
            finally
            {
                // 加载分组
                GroupInit();
                // 加载最近上传列表
                LastestInit();
            }
            // 设立图片操作的快捷键
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            // 每10秒检测一次数据库状态，一旦数据库需要同步，则执行同步操作
            AutoUpdateTimer.Interval = new TimeSpan(0, 0, 10);
            AutoUpdateTimer.Tick += AutoUpdateDatabase;
            AutoUpdateTimer.Start();
        }
        private async void OneDriveInit()
        {
            try
            {
                UpdateLoadingRing.IsActive = true;
                // 检查OneDrive授权并获取应用文件夹
                await App.OneDriveTools.OneDriveAuthorize();
                // 检查云端数据库是否需要同步最新更改
                bool isNoNeedSync = await App.OneDriveTools.CheckLastAsync();
                if (!isNoNeedSync)
                {
                    new PopupMaskTip(AppTools.GetReswLanguage("HaveUpdate")).Show();
                    App.Db.Dispose();
                    // 从别的客户端处修改了数据，那么本机进行数据同步
                    await App.OneDriveTools.SyncCloud();
                    App.Db = new ImageDbContext();
                }
                UpdateLoadingRing.IsActive = false;
            }
            catch (Exception ex)
            {
                UpdateLoadingRing.IsActive = false;
                // 这里可能会出现授权失败的情况
                if (ex.GetType() == typeof(UnauthorizedAccessException))
                {

                }
                else
                {
                    throw;
                }
            }
        }
        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                // 按下Esc键则退出图片详情
                if (args.VirtualKey == Windows.System.VirtualKey.Escape)
                {
                    foreach (var item in PopupUserControlList)
                    {
                        if (item.GetType() == typeof(ImageMaskControl))
                        {
                            var t = (ImageMaskControl)item;
                            t.Close();
                            break;
                        }
                    }
                }
                // 向左或向上切换上一张图片，没有则弹出提醒
                else if (args.VirtualKey == VirtualKey.Left || args.VirtualKey==VirtualKey.Up)
                {
                    foreach (var item in PopupUserControlList)
                    {
                        if (item.GetType() == typeof(ImageMaskControl))
                        {
                            var t = (ImageMaskControl)item;
                            var oldImg = t.imageFile;
                            var groupimages = App.Db.Images.Where(p => p.GroupId == oldImg.GroupId).ToList();
                            int index = groupimages.IndexOf(oldImg);
                            if(index!=-1 && index > 0)
                            {
                                t.Init(groupimages[index - 1]);
                                break;
                            }
                            else
                            {
                                new PopupMaskTip(AppTools.GetReswLanguage("IsFirstImage")).Show();
                                break;
                            }
                        }
                    }
                }
                // 向右或向下切换下一张图片，没有则弹出提醒
                else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey==VirtualKey.Down)
                {
                    foreach (var item in PopupUserControlList)
                    {
                        var t = (ImageMaskControl)item;
                        var oldImg = t.imageFile;
                        var groupimages = App.Db.Images.Where(p => p.GroupId == oldImg.GroupId).ToList();
                        int index = groupimages.IndexOf(oldImg);
                        if (index != -1 && index <groupimages.Count-1)
                        {
                            t.Init(groupimages[index + 1]);
                            break;
                        }
                        else
                        {
                            new PopupMaskTip(AppTools.GetReswLanguage("IsLastImage")).Show();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 同步数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AutoUpdateDatabase(object sender, object e)
        {
            await App.OneDriveTools.BackupDatabase();
        }

        /// <summary>
        /// 加载最近上传列表
        /// </summary>
        public void LastestInit()
        {
            // 根据上传时间进行数据排序，再根据限定的LastestImgCount来限制图片数量
            var images = App.Db.Images.ToList();
            LastestImageCollection.Clear();
            if (images.Count > 0)
            {
                images.Sort((x, y) => { return y.UploadTime - x.UploadTime; });
                int count = 0;
                for (int i = 0; i < images.Count; i++)
                {
                    if (count == LastestImgCount)
                    {
                        break;
                    }
                    LastestImageCollection.Add(images[i]);
                    count++;
                }
                LastestListView.Visibility = Visibility.Visible;
                LastestNoDataTipBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                LastestListView.Visibility = Visibility.Collapsed;
                LastestNoDataTipBlock.Visibility = Visibility.Visible;
            }
            IsLastImagesInit = true;
            AllInitCheck();
        }

        /// <summary>
        /// 分组加载
        /// </summary>
        private async void GroupInit()
        {
            var group = App.Db.Groups;
            // 如果是第一次运行软件，则为其创建一个默认分组
            if (group.Count() == 0)
            {
                int num=await App.OneDriveTools.AddGroupToDatabase(App.Db, new OneDriveImageGroup("Default"));
                if (num > 0)
                {
                    GroupInit();
                }
            }
            else
            {
                // 检查上一次选中的分组，如果没有，则默认选中第一个
                GroupCollection.Clear();
                string lastGroupId = AppTools.GetLocalSetting(AppSettings.SelectGroupIndex, "");
                if (string.IsNullOrEmpty(lastGroupId))
                {
                    lastGroupId = group.First().GroupId;
                    AppTools.WriteLocalSetting(AppSettings.SelectGroupIndex, lastGroupId);
                }
                foreach (var item in group)
                {
                    
                    GroupCollection.Add(item);
                    if (item.GroupId == lastGroupId)
                    {
                        GroupCombo.SelectedItem = item;
                    }
                }
                HistoryInit();
            }
        }

        /// <summary>
        /// 组内历史图片记录
        /// </summary>
        public void HistoryInit()
        {
            // 这里的显示就按时间正序
            string groupId = AppTools.GetLocalSetting(AppSettings.SelectGroupIndex, "");
            var imgs = App.Db.Images.Where(p => p.GroupId == groupId);
            if (imgs.Count()>0)
            {
                HistoryImageCollection.Clear();

                foreach (var item in imgs)
                {
                    HistoryImageCollection.Add(item);
                }
                HistoryNoDataTipBlock.Visibility = Visibility.Collapsed;
                HistoryGridView.Visibility = Visibility.Visible;
            }
            else
            {
                HistoryNoDataTipBlock.Visibility = Visibility.Visible;
                HistoryGridView.Visibility = Visibility.Collapsed;
            }
            IsHistoryImageInit = true;
            AllInitCheck();
        }

        /// <summary>
        /// 确认各个组件加载完成
        /// </summary>
        private void AllInitCheck()
        {
            if(IsHistoryImageInit && IsLastImagesInit)
            {
                isInit = true;
                //WaittingTip.Close();
            }
        }

        /// <summary>
        /// 软件视窗大小变化时的自适应改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = e.NewSize.Width;
            double height = e.NewSize.Height;

            // 确定上传区域的宽度
            double uploadColumnWidth=UploadColumn.ActualWidth-30;
            UploadAreaHeight = height * 0.7;
            int count = UploadInfoCollection.Count == 0 ? 1 : UploadInfoCollection.Count;
            UploadInfoWidth = uploadColumnWidth / count;
            UploadProgressWidth = uploadColumnWidth;

            // 确定搜索区域的大致宽度
            SearchBackgroundWidth = width * 0.4;
            SearchBoxWidth = SearchBackgroundWidth - 60;
            
            // 确定显示区域内各组件的占位
            ShowAreaWidth = ShowColumn.ActualWidth-50;
            int last = Convert.ToInt32(Math.Floor(ShowAreaWidth / 170.0));
            if (last != LastestImgCount)
            {
                LastestImgCount = last;
                if(isInit)
                    LastestInit();
            }
            if (PopupUserControlList.Count > 0)
            {
                foreach (var item in PopupUserControlList)
                {
                    item.Width = Window.Current.Bounds.Width;
                    item.Height = Window.Current.Bounds.Height;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 图片搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchImgBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 根据输入内容搜索数据库内的图片名称
            string text = SearchImgBox.Text.Trim();
            SearchResultCollection.Clear();
            if (string.IsNullOrEmpty(text))
            {
                SearchPop.IsOpen = false;
                return;
            }
            var list = App.Db.Images.Where(p => AppTools.NormalString(p.ImageName).IndexOf(AppTools.NormalString(text)) != -1);
            if (list.Count() > 0)
            {
                foreach (var item in list)
                {
                    SearchResultCollection.Add(item);
                }
                SearchPop.IsOpen = true;
            }
            else
            {
                SearchPop.IsOpen = false;
            }
        }

        /// <summary>
        /// 搜索结果备选被点击时，打开被选图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchResultListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SearchPop.IsOpen = false;
            var img = (OneDriveImage)e.ClickedItem;
            var mask = new ImageMaskControl(img);
            mask.Show();
        }

        private void UploadArea_DragEnter(object sender, DragEventArgs e)
        {
            // 拖拽时显示的手势
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        /// <summary>
        /// 当文件被拖拽进上传框中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UploadArea_Drop(object sender, DragEventArgs e)
        {
           // 如果当前有文件正在上传，则阻止当前行为
            if (UploadProgressCollection.Count > 0)
            {
                string msg = AppTools.GetReswLanguage("WaitToUpload");
                var tipDialog = new TipDialog(AppTools.GetReswLanguage("DefaultTipTitle"), msg);
                await tipDialog.ShowAsync();
                return;
            }
            // 检测授权码是否存在，如不存在，则要求用户重新输入授权码
            string authKey = AppTools.GetLocalSetting(AppSettings.AuthKey, "");
            if (string.IsNullOrEmpty(authKey))
            {
                var keyDialog = new AuthKeyDialog();
                await keyDialog.ShowAsync();
                return;
            }
            // 检测拖拽的文件
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {

                var items = await e.DataView.GetStorageItemsAsync();
                items = items.OfType<StorageFile>().ToList() as IReadOnlyList<IStorageItem>;
                // 对文件进行筛选，只要指定格式的图片
                if (items != null && items.Any())
                {
                    var imgList = new List<StorageFile>();
                    foreach (var item in items)
                    {
                        string path = item.Path;
                        string extends = Path.GetExtension(item.Path).Substring(1);
                        var file = item as StorageFile;
                        if(extends.ToLower()=="jpg" || extends.ToLower() == "png"|| extends.ToLower() == "bmp"||extends.ToLower() == "gif"|| extends.ToLower() == "jpeg" || extends.ToLower() == "webp")
                        {
                            imgList.Add(file);
                        }
                    }
                    await FileUpload(imgList);
                }
                
            }
        }

        /// <summary>
        /// 点击上传区域，弹出文件选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UploadArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await AddImageFromFile();
        }

        public async Task AddImageFromFile()
        {
            if (UploadProgressCollection.Count > 0)
            {
                string msg = AppTools.GetReswLanguage("WaitToUpload");
                var tipDialog = new TipDialog(AppTools.GetReswLanguage("DefaultTipTitle"), msg);
                await tipDialog.ShowAsync();
                return;
            }
            string authKey = AppTools.GetLocalSetting(AppSettings.AuthKey, "");
            if (string.IsNullOrEmpty(authKey))
            {
                var keyDialog = new AuthKeyDialog();
                await keyDialog.ShowAsync();
                return;
            }
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".webp");
            var files = await picker.PickMultipleFilesAsync();
            if (files != null && files.Any())
            {
                var imgList = new List<StorageFile>();
                foreach (var item in files)
                {
                    var file = item as StorageFile;
                    imgList.Add(file);
                }
                await FileUpload(imgList);
            }
        }

        public async Task FileUpload(List<StorageFile> imgList, bool isDeleteFiles = false)
        {
            if (imgList.Count > 0)
            {
                // 弹出组选择框
                var groupDialog = new GroupDialog();
                groupDialog.PrimaryButtonClick += async (_s, _e) =>
                {
                    // 选定分组后，上传图片
                    if (groupDialog.SelectGroup != null)
                    {
                        var errorFile = new List<StorageFile>();
                        UploadInfoCollection.Clear();
                        UploadProgressCollection.Clear();
                        var imgItemList = new List<OneDriveImage>();
                        var imgTemp = await AppTools.ConvertFileToImage(imgList[0]);
                        // 如果仅一张图片上传，则显示该图片的分辨率、大小等信息
                        if (imgList.Count == 1)
                        {
                            var fileInfo = await imgList[0].GetBasicPropertiesAsync();

                            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("RP"), imgTemp.PixelWidth + "x" + imgTemp.PixelHeight));
                            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("ImageSize"), Math.Round((fileInfo.Size / (1024.0 * 1024.0)), 1) + "M"));
                            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("ImageType"), Path.GetExtension(imgList[0].Path).Substring(1).ToUpper()));
                            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("CreateDate"), fileInfo.DateModified.ToString("yyyy/MM/dd")));
                        }
                        // 如果有多张图片上传，则显示总览信息
                        else
                        {
                            double size = 0;
                            foreach (var item in imgList)
                            {
                                var fileInfo = await item.GetBasicPropertiesAsync();
                                size += Math.Round((fileInfo.Size / (1024.0 * 1024.0)), 1);
                            }
                            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("ImageCount"), imgList.Count.ToString()));
                            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("GroupName"), groupDialog.SelectGroup.GroupName));
                            UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("AllSize"), size + "M"));
                        }
                        // 将上传列表的第一张图片作为上传区域背景
                        var backBrush = new ImageBrush();
                        backBrush.Stretch = Stretch.UniformToFill;
                        backBrush.ImageSource = imgTemp;
                        UploadArea.Background = backBrush;
                        // 隐藏上传提示字符
                        UploadAreaHold.Visibility = Visibility.Collapsed;
                        double uploadColumnWidth = UploadColumn.ActualWidth - 30;
                        // 根据上传信息显示的数目来确定单一信息所占区域大小
                        UploadInfoWidth = uploadColumnWidth / UploadInfoCollection.Count;
                        // 装载进度条
                        for (int i = 0; i < imgList.Count; i++)
                        {
                            var item = imgList[i];
                            UploadProgressCollection.Add(new ProgressStatus(i + 1, item.DisplayName));
                        }
                        var tasks = new List<Task>();
                        // 开始逐一上传图片
                        foreach (var item in imgList)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                                {
                                    var img = await App.OneDriveTools.UploadImage(item, groupDialog.SelectGroup);
                                    // 图片若上传错误，则加入错误文件列表中
                                    if (img == null)
                                    {
                                        errorFile.Add(item);
                                    }
                                    // 否则，写入成功列表
                                    else
                                    {
                                        imgItemList.Add(img);
                                        LastestImageCollection.Insert(0, img);
                                        if (LastestNoDataTipBlock.Visibility == Visibility.Visible)
                                        {
                                            LastestNoDataTipBlock.Visibility = Visibility.Collapsed;
                                            LastestListView.Visibility = Visibility.Visible;
                                        }
                                        for (int j = UploadProgressCollection.Count - 1; j >= 0; j--)
                                        {
                                            if (UploadProgressCollection[j].Name.Replace($"[{j + 1}] ", "") == item.DisplayName)
                                            {
                                                UploadProgressCollection.RemoveAt(j);
                                            }
                                        }
                                        if (isDeleteFiles)
                                        {
                                            await item.DeleteAsync();
                                        }
                                    }
                                });
                            }));
                        }
                        await Task.WhenAll(tasks.ToArray());
                        // 所有任务上传完成，清空背景，加入占位符
                        var res = App.Current.RequestedTheme == ApplicationTheme.Dark ? (ResourceDictionary)App.Current.Resources.ThemeDictionaries["Dark"] : (ResourceDictionary)App.Current.Resources.ThemeDictionaries["Light"];
                        var color = (SolidColorBrush)res["MainBackground"];
                        UploadArea.Background = color;
                        UploadAreaHold.Visibility = Visibility.Visible;
                        // 恢复默认上传信息
                        UploadInfoCollection.Clear();
                        UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("DefaultUploadInfoTitle"), AppTools.GetReswLanguage("None")));
                        UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("ImageSize"), AppTools.GetReswLanguage("None")));
                        UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("ImageType"), AppTools.GetReswLanguage("None")));
                        UploadInfoCollection.Add(new KeyValue(AppTools.GetReswLanguage("CreateDate"), AppTools.GetReswLanguage("None")));
                        UploadInfoWidth = uploadColumnWidth / UploadInfoCollection.Count;
                        // 清除上传进度条
                        UploadProgressCollection.Clear();

                        // 当成功列表中有数据时，显示成功数目，并将成功上传的图片反映在UI上
                        if (imgItemList.Any())
                        {
                            int num = await App.OneDriveTools.AddImageToDatabase(App.Db, imgItemList);
                            string msg = AppTools.GetReswLanguage("AddImageSuccess").Replace("{count}", num.ToString());
                            new PopupMaskTip(msg).Show();
                            AppTools.WriteLocalSetting(AppSettings.SelectGroupIndex, groupDialog.SelectGroup.GroupId);
                            GroupInit(groupDialog.SelectGroup);
                        }
                        // 如果错误列表中有数据，则提醒用户
                        if (errorFile.Any())
                        {
                            int num = errorFile.Count;
                            string msg = AppTools.GetReswLanguage("AddImageFailed").Replace("{count}", num.ToString());
                            var tipDialog = new TipDialog(AppTools.GetReswLanguage("AddFailTitle"), msg);
                            await tipDialog.ShowAsync();
                        }
                    }
                };
                await groupDialog.ShowAsync();
            }
        }

        /// <summary>
        /// 传入指定分组数据时进行跳转
        /// </summary>
        /// <param name="group"></param>
        private void GroupInit(OneDriveImageGroup group)
        {
            var groupImages = App.Db.Images.Where(p => p.GroupId == group.GroupId);
            HistoryImageCollection.Clear();
            foreach (var item in groupImages)
            {
                HistoryImageCollection.Add(item);
            }
            if (HistoryImageCollection.Count > 0)
            {
                HistoryGridView.Visibility = Visibility.Visible;
                HistoryNoDataTipBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                HistoryGridView.Visibility = Visibility.Collapsed;
                HistoryNoDataTipBlock.Visibility = Visibility.Visible;
            }
            if (!((OneDriveImageGroup)GroupCombo.SelectedItem).Equals(group))
            {
                GroupCombo.SelectedItem = group;
            }
        }

        /// <summary>
        /// 分组列表改变时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupCombo.SelectedItem != null)
            {
                var item = (OneDriveImageGroup)GroupCombo.SelectedItem;
                string selectId = AppTools.GetLocalSetting(AppSettings.SelectGroupIndex, "");
                if (item.GroupId != selectId)
                {
                    AppTools.WriteLocalSetting(AppSettings.SelectGroupIndex, item.GroupId);
                    GroupInit(item);
                    HistoryInit();
                }
            }
            
        }

        /// <summary>
        /// 添加分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddGroupDialog();
            await dialog.ShowAsync();
        }

        /// <summary>
        /// 显示分组管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManagerGroupButton_Click(object sender, RoutedEventArgs e)
        {
            new GroupMaskControl().Show();
        }

        /// <summary>
        /// 显示设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingControl().Show();
        }

        /// <summary>
        /// （供外部调用）将弹出层加入已弹出列表
        /// </summary>
        /// <param name="control"></param>
        public void AddMaskInList(UserControl control)
        {
            if (PopupUserControlList.Any(p => p.GetType()==control.GetType()))
            {
                return;
            }
            else
            {
                PopupUserControlList.Add(control);
            }
        }

        /// <summary>
        /// （供外部调用）将弹出层从已弹出列表中移除
        /// </summary>
        /// <param name="control"></param>
        public void RemoveMaskFromList(UserControl control)
        {
            if (PopupUserControlList.Count > 0)
            {
                for (int i = PopupUserControlList.Count - 1; i >=0 ; i--)
                {
                    if (PopupUserControlList[i].GetType() == control.GetType())
                    {
                        PopupUserControlList.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// （供外部调用）添加新的分组，并将该分组作为选中项
        /// </summary>
        /// <param name="group"></param>
        public void GroupCollectionAdd(OneDriveImageGroup group)
        {
            GroupCollection.Add(group);
            GroupCombo.SelectedItem = group;
        }

        /// <summary>
        /// （供外部调用）移除分组
        /// </summary>
        /// <param name="group"></param>
        public void GroupCollectionRemoved(OneDriveImageGroup group)
        {
            // 待定
            string selectId = AppTools.GetLocalSetting(AppSettings.SelectGroupIndex, "");
            if (selectId == group.GroupId)
            {
                AppTools.WriteLocalSetting(AppSettings.SelectGroupIndex, "");
                GroupCollectionReInit();
            }
            else
            {
                GroupCollection.Remove(group);
            }
        }

        /// <summary>
        /// （供外部调用）重新加载分组集合
        /// </summary>
        public void GroupCollectionReInit()
        {
            isInit = false;
            var groups = App.Db.Groups;
            GroupCollection.Clear();
            string selectId = AppTools.GetLocalSetting(AppSettings.SelectGroupIndex, "");
            if (string.IsNullOrEmpty(selectId))
            {
                selectId = groups.ToList()[0].GroupId;
                AppTools.WriteLocalSetting(AppSettings.SelectGroupIndex, selectId);
            }
            foreach (var item in groups)
            {
                GroupCollection.Add(item);
            }
            foreach (var item in GroupCollection)
            {
                if (item.GroupId == selectId)
                    GroupCombo.SelectedItem = item;
            }
            isInit = true;
        }

        /// <summary>
        /// （供外部调用）批量为图片添加前缀
        /// </summary>
        /// <param name="newKey"></param>
        public void ChangeImagesUrl(string newKey)
        {
            foreach (var item in HistoryImageCollection)
            {
                int index = item.URL.LastIndexOf("key=") + 4;
                item.URL = item.URL.Replace(item.URL.Substring(index), newKey);
            }
        }

        private async void FromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await AddImageFromFile();
        }

        private async void FromClipboardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (UploadProgressCollection.Count > 0)
            {
                string msg = AppTools.GetReswLanguage("WaitToUpload");
                var tipDialog = new TipDialog(AppTools.GetReswLanguage("DefaultTipTitle"), msg);
                await tipDialog.ShowAsync();
                return;
            }
            string authKey = AppTools.GetLocalSetting(AppSettings.AuthKey, "");
            if (string.IsNullOrEmpty(authKey))
            {
                var keyDialog = new AuthKeyDialog();
                await keyDialog.ShowAsync();
                return;
            }
            try
            {
                var data = Clipboard.GetContent();
                var bitmap = await data.GetBitmapAsync();
                var tempFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(Guid.NewGuid().ToString("N")+".png",CreationCollisionOption.OpenIfExists);
                using (var stream = await bitmap.OpenReadAsync() as IRandomAccessStream)
                {
                    using (var reader = new DataReader(stream))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        var buffer = new byte[(int)stream.Size];
                        reader.ReadBytes(buffer);
                        await FileIO.WriteBytesAsync(tempFile, buffer);
                    }
                }
                await FileUpload(new List<StorageFile>() { tempFile },true);
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UploadArea_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            UploadAreaMenuFlyout.ShowAt(sender as FrameworkElement, e.GetPosition(sender as FrameworkElement));
        }

        private void UploadArea_Holding(object sender, HoldingRoutedEventArgs e)
        {
            UploadAreaMenuFlyout.ShowAt(sender as FrameworkElement, e.GetPosition(sender as FrameworkElement));
        }
    }
}
