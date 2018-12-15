using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Img_Share.Model
{
    /// <summary>
    /// 一个简单的键值对类，修改Value会对UI产生影响
    /// </summary>
    public class KeyValue:INotifyPropertyChanged
    {
        public string Key { get; set; }

        private string _value { get; set; }
        public string Value
        {
            get => _value;
            set { _value = value;OnPropertyChanged(); }
        }

        public KeyValue() { }

        public KeyValue(string key,string value)
        {
            Key = key;
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
