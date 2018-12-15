using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveShareImage.Model
{
    /// <summary>
    /// 图片分组类
    /// </summary>
    public class OneDriveImageGroup
    {
        [Required]
        public int Id { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }

        public OneDriveImageGroup() { }

        public OneDriveImageGroup(string name)
        {
            GroupId = Guid.NewGuid().ToString("N");
            GroupName = name;
            Id = 0;
        }

        public override bool Equals(object obj)
        {
            var group = obj as OneDriveImageGroup;
            return group != null &&
                   GroupId == group.GroupId;
        }

        public override int GetHashCode()
        {
            return -1221475543 + EqualityComparer<string>.Default.GetHashCode(GroupId);
        }

        public override string ToString()
        {
            return GroupName;
        }
    }
}
