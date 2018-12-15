using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace OneDriveShareImage.Model
{
    /// <summary>
    /// OneDrive图片实体类
    /// </summary>
    public class OneDriveImage
    {
        [Required]
        public int Id { get; set; }
        public string ImageId { get; set; }
        public string ImageName { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string URL { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public int UploadTime { get; set; }

        public OneDriveImage() { }

        public OneDriveImage(string name,BitmapImage image,OneDriveImageGroup group,string url,string imageId)
        {
            GroupId = group.GroupId;
            GroupName = group.GroupName;
            ImageName = name;
            Width = image.PixelWidth;
            Height = image.PixelHeight;
            ImageId = imageId;
            URL = url;
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            UploadTime = Convert.ToInt32(ts.TotalSeconds);
        }

        public override bool Equals(object obj)
        {
            var image = obj as OneDriveImage;
            return image != null &&
                   ImageId == image.ImageId;
        }

        public override int GetHashCode()
        {
            return 1454589903 + EqualityComparer<string>.Default.GetHashCode(ImageId);
        }
    }
}
