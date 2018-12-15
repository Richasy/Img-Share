using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Img_Share.Model
{
    /// <summary>
    /// 进度条类
    /// </summary>
    public class ProgressStatus
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsStop { get; set; }
        public ProgressStatus()
        {

        }

        public ProgressStatus(int index,string name,bool isstop = false)
        {
            Index = index;
            DisplayName = $"[{Index}] {name}";
            Name = name;
            IsStop = isstop;
        }
    }
}
