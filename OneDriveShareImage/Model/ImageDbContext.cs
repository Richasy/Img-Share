using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using Windows.Storage;

namespace OneDriveShareImage.Model
{
    /// <summary>
    /// 图片数据上下文
    /// </summary>
    public class ImageDbContext:DbContext
    {
        public DbSet<OneDriveImage> Images { get; set; }
        public DbSet<OneDriveImageGroup> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ImgMeta.db");
            //string path = AppTools.GetLocalSetting(AppSettings.DatabasePath, "ImgShare.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }

    /// <summary>
    /// 图片元数据实体类
    /// </summary>
    public class ImageMetaList
    {
        public List<OneDriveImage> Images { get; set; }
        public List<OneDriveImageGroup> Groups { get; set; }
    }
}
