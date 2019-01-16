using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frh.K3.FastExtension
{
    public class ExtensionItem
    {
        public string Key { get; set; }//字段名
        public string Name { get; set; }//显示的名称
        public string Type { get; set; }//类型，可以是文本、数字、枚举、F8
        public Boolean InitValue { get; set; }//值类型，可以是上游值过来的，也可以手工填写
        public string Regex { get; set; }//正则表达式，用于约束
        public string F8FormId { get; set; } //F8来源，如果type为F8，则这个必须有
        public string F8TableName { get; set; } //F8来源，如果type为F8，则这个必须有
        public string LookupObjectId { get; set; }//辅助资料来源，必须设置
        public Boolean Hide { get; set; }//是否隐藏
        public Boolean Required { get; set; }//是否必录
    }
}
