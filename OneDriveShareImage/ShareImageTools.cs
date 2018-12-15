using Microsoft.Toolkit.Services.OneDrive;
using Microsoft.Toolkit.Services.Services.MicrosoftGraph;
using OneDriveShareImage.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Tools;
using Windows.Storage;
using Windows.Storage.Streams;

namespace OneDriveShareImage
{
    /// <summary>
    /// OneDrive图片分享工具
    /// </summary>
    public class ShareImageTools
    {
        /// <summary>
        /// 应用根目录
        /// </summary>
        private OneDriveStorageFolder _appFolder = null;
        /// <summary>
        /// 客户端ID 需前往 https://apps.dev.microsoft.com/ 注册一个应用账号
        /// </summary>
        private string _clientId = "your_client_id";
        /// <summary>
        /// 授权范围
        /// </summary>
        private string[] _scopes = new string[] { MicrosoftGraphScope.FilesReadWriteAll, MicrosoftGraphScope.FilesReadWriteAppFolder,MicrosoftGraphScope.UserReadAll };

        /// <summary>
        /// 启动OneDrive登录授权
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OneDriveAuthorize()
        {
            if (_appFolder != null)
            {
                return true;
            }

            try
            {
                // 初始化OneDrive服务实例
                bool isInit = OneDriveService.Instance.Initialize(_clientId, _scopes);
                // 确认用户完成了微软账户登录流程
                bool isLogin = await OneDriveService.Instance.LoginAsync();
                if (isInit && isLogin)
                {
                    // 获取应用文件夹
                    _appFolder = await OneDriveService.Instance.AppRootFolderAsync();
                    await _appFolder.StorageFolderPlatformService.CreateFolderAsync("Image", CreationCollisionOption.OpenIfExists);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// 获取OneDrive应用文件夹中的图片文件夹
        /// </summary>
        /// <returns></returns>
        private async Task<OneDriveStorageFolder> GetImgSaveFolder()
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can get a folder");
            }
            var folder = await _appFolder.StorageFolderPlatformService.CreateFolderAsync("Image", CreationCollisionOption.OpenIfExists);
            return folder;
        }

        /// <summary>
        /// 确认OneDrive中是否有备份数据存在
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EnsureCloudMetaExist()
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can get this file");
            }
            try
            {
                var file = await _appFolder.GetFileAsync("ImgMeta.db");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取OneDrive中存储的图片元数据，这里以SQLite数据存储
        /// </summary>
        /// <returns></returns>
        public async Task<ImageDbContext> GetImgMetaList()
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can get this file");
            }
            bool hasCloudFile = await EnsureCloudMetaExist();
            if (hasCloudFile)
            {
                return new ImageDbContext();
            }
            else
            {
                await DatabaseInit();
                return null;
            }
        }

        /// <summary>
        /// 上传图片至OneDrive
        /// </summary>
        /// <param name="image">图片文件</param>
        /// <returns></returns>
        public async Task<OneDriveImage> UploadImage(StorageFile image, OneDriveImageGroup group)
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can upload the image");
            }
            var imageFolder = await GetImgSaveFolder();
            var per = await image.GetBasicPropertiesAsync();
            string fileId = "";
            string name = "";
            OneDriveStorageFile cre = null;
            try
            {
                // 这里要对文件大小进行判断，以4MB为分水岭，需要用不同的办法上传
                if (per.Size < 4 * 1024 * 1024)
                {
                    using (var stream = await image.OpenReadAsync())
                    {
                        cre = await imageFolder.StorageFolderPlatformService.CreateFileAsync(image.Name.Trim(), CreationCollisionOption.ReplaceExisting, stream);
                        fileId = cre.OneDriveItem.Id;
                        name = cre.OneDriveItem.Name;
                    }
                }
                else
                {
                    using (var stream = await image.OpenReadAsync())
                    {
                        cre = await imageFolder.StorageFolderPlatformService.UploadFileAsync(image.Name.Trim(), stream, CreationCollisionOption.ReplaceExisting, 320 * 1024);
                        fileId = cre.OneDriveItem.Id;
                        name = cre.OneDriveItem.Name;
                    }
                }
                string link = LinkConvert(fileId);
                var item = new OneDriveImage(name, await AppTools.ConvertFileToImage(image), group, link, fileId);
                return item;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 检测上一次同步的时间，并于本机同步时间比对
        /// </summary>
        /// <returns>相同返回<c>True</c>，不同返回<c>False</c></returns>
        public async Task<bool> CheckLastAsync()
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can upload the image");
            }
            try
            {
                var historyFile = await _appFolder.GetFileAsync("LastAsyncTime");
                using (var stream = (await historyFile.StorageFilePlatformService.OpenAsync()) as IRandomAccessStream)
                {
                    Stream st = WindowsRuntimeStreamExtensions.AsStreamForRead(stream);
                    st.Position = 0;
                    StreamReader sr = new StreamReader(st, Encoding.UTF8);
                    string result = sr.ReadToEnd();
                    result = result.Replace("\0", "");
                    if (string.IsNullOrEmpty(result))
                    {
                        return true;
                    }
                    else
                    {
                        try
                        {
                            int cloudTime = Convert.ToInt32(result);
                            int localTime = Convert.ToInt32(AppTools.GetLocalSetting(AppSettings.SyncTime, "0"));
                            if (cloudTime != localTime)
                            {
                                return false;
                            }
                            return true;
                        }
                        catch (Exception)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                await _appFolder.StorageFolderPlatformService.CreateFileAsync("LastAsyncTime", CreationCollisionOption.OpenIfExists);
                return true;
            }

        }

        /// <summary>
        /// 强制执行云端数据同步，并将同步时间写入记录
        /// </summary>
        /// <returns></returns>
        public async Task SyncCloud()
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can upload the image");
            }
            await ReplaceDatabase();
            var historyFile = await _appFolder.GetFileAsync("LastAsyncTime");
            using (var stream = (await historyFile.StorageFilePlatformService.OpenAsync()) as IRandomAccessStream)
            {
                Stream st = WindowsRuntimeStreamExtensions.AsStreamForRead(stream);
                st.Position = 0;
                StreamReader sr = new StreamReader(st, Encoding.UTF8);
                string result = sr.ReadToEnd();
                result = string.IsNullOrEmpty(result) ? "0" : result;
                AppTools.WriteLocalSetting(AppSettings.SyncTime, result);
            }
        }
        /// <summary>
        /// 批量删除图片
        /// </summary>
        /// <param name="images">图片列表</param>
        /// <returns></returns>
        public async Task<int> DeleteImage(ICollection<OneDriveImage> images)
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can upload the image");
            }
            int count = 0;
            var imageFolder = await GetImgSaveFolder();
            foreach (var item in images)
            {
                try
                {
                    var file = await imageFolder.GetFileAsync(item.ImageName);
                    if (file != null)
                    {
                        try
                        {
                            await file.DeleteAsync();
                            count += 1;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception)
                {

                    continue;
                }

            }
            return count;
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="image">图片信息</param>
        /// <returns></returns>
        public async Task<bool> DeleteImage(OneDriveImage image)
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can upload the image");
            }
            var imageFolder = await GetImgSaveFolder();
            var file = await imageFolder.StorageFolderPlatformService.CreateFileAsync(image.ImageName, CreationCollisionOption.OpenIfExists);
            if (file != null)
            {
                try
                {
                    await file.DeleteAsync();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 将图片数据添加到数据库
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="item">图片条目</param>
        /// <returns></returns>
        public async Task<int> AddImageToDatabase(ImageDbContext context, ICollection<OneDriveImage> item)
        {
            await context.Images.AddRangeAsync(item);
            int num = await context.SaveChangesAsync();
            AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
            return num;
        }

        /// <summary>
        /// 将组添加到数据库
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="item">组</param>
        /// <returns></returns>
        public async Task<int> AddGroupToDatabase(ImageDbContext context, OneDriveImageGroup item)
        {
            await context.Groups.AddAsync(item);
            int num = await context.SaveChangesAsync();
            AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "True");
            return num;
        }

        /// <summary>
        /// 完成对云端元数据的替换
        /// </summary>
        /// <returns></returns>
        public async Task BackupDatabase()
        {
            bool isChanged = Convert.ToBoolean(AppTools.GetLocalSetting(AppSettings.IsDatabaseChanged, "False"));
            if (isChanged)
            {
                var localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("ImgMeta.db", CreationCollisionOption.OpenIfExists);
                using (var localStream = await localFile.OpenReadAsync())
                {
                    var cloudDb = await _appFolder.StorageFolderPlatformService.CreateFileAsync("ImgMeta.db", CreationCollisionOption.ReplaceExisting, localStream);

                    AppTools.WriteLocalSetting(AppSettings.IsDatabaseChanged, "False");
                }
                TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                string ChangeTime = Convert.ToInt32(ts.TotalSeconds).ToString();
                var temp = await WriteTempFile("LastAsyncTime", ChangeTime);
                using (var stream = await temp.OpenReadAsync())
                {
                    await _appFolder.StorageFolderPlatformService.CreateFileAsync("LastAsyncTime", CreationCollisionOption.ReplaceExisting, stream);
                }
                AppTools.WriteLocalSetting(AppSettings.SyncTime, ChangeTime);
            }
        }

        public async Task<StorageFile> WriteTempFile(string fileName, string content)
        {
            var localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(localFile, content);
            return localFile;
        }

        /// <summary>
        /// 完成对本地元数据的替换
        /// </summary>
        /// <returns></returns>
        public async Task ReplaceDatabase()
        {

            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can get this file");
            }
            var cloudFile = await _appFolder.GetFileAsync("ImgMeta.db");
            using (IRandomAccessStream defaultStream = (await cloudFile.StorageFilePlatformService.OpenAsync()) as IRandomAccessStream)
            {
                await SaveToLocalFolder(defaultStream, "ImgMeta.db");
            }
        }

        /// <summary>
        /// 初始化云端和本地元数据库
        /// </summary>
        /// <returns></returns>
        public async Task DatabaseInit()
        {
            var DefaultDb = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ImgShare.db"));
            var localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("ImgMeta.db", CreationCollisionOption.OpenIfExists);
            using (var defaultStream = await DefaultDb.OpenReadAsync())
            {
                var cloudDb = await _appFolder.StorageFolderPlatformService.CreateFileAsync("ImgMeta.db", CreationCollisionOption.ReplaceExisting, defaultStream);

            }
            await DefaultDb.CopyAndReplaceAsync(localFile);
        }

        /// <summary>
        /// 链接转换
        /// </summary>
        /// <param name="id">回传的文件ID</param>
        /// <returns></returns>
        public string LinkConvert(string id)
        {
            string ImageAuthkey = AppTools.GetLocalSetting(AppSettings.AuthKey, "");
            if (!String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(ImageAuthkey))
            {
                return $"http://storage.live.com/items/{id}?authkey={ImageAuthkey}";
            }
            return null;
        }

        /// <summary>
        /// Save the stream to a file in the local folder
        /// </summary>
        /// <param name="remoteStream">Stream to save</param>
        /// <param name="fileName">File's name</param>
        /// <returns>Task to support await of async call.</returns>
        public async Task SaveToLocalFolder(IRandomAccessStream remoteStream, string fileName)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            byte[] buffer = new byte[remoteStream.Size];
            var localBuffer = await remoteStream.ReadAsync(buffer.AsBuffer(), (uint)remoteStream.Size, InputStreamOptions.ReadAhead);
            var myLocalFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var localStream = await myLocalFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await localStream.WriteAsync(localBuffer);
                await localStream.FlushAsync();
            }
        }

        /// <summary>
        /// 批量为图片增加前缀
        /// </summary>
        /// <param name="images">改名图片列表</param>
        /// <param name="prefix">前缀</param>
        /// <returns></returns>
        public async Task<int> RenameImage(ICollection<OneDriveImage> images, string prefix)
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can get this file");
            }
            int count = 0;
            var imageFolder = await GetImgSaveFolder();
            foreach (var item in images)
            {
                try
                {
                    var file = await imageFolder.GetFileAsync(item.ImageName);
                    await file.RenameAsync(prefix + item.ImageName);
                    count += 1;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return count;
        }
        /// <summary>
        /// 修改某图片名
        /// </summary>
        /// <param name="image">图片</param>
        /// <param name="newName">新名字</param>
        /// <returns></returns>
        public async Task<string> RenameImage(OneDriveImage image, string newName)
        {
            if (_appFolder == null)
            {
                throw new UnauthorizedAccessException("You need to complete OneDrive authorization before you can get this file");
            }
            var imageFolder = await GetImgSaveFolder();
            try
            {
                var file = await imageFolder.GetFileAsync(image.ImageName);
                await file.RenameAsync(newName);
                return newName;
            }
            catch (Exception)
            {
                return image.ImageName;
            }
        }
    }
}
