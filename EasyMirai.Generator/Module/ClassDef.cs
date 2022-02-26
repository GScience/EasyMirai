using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMirai.Generator.Module
{
    /// <summary>
    /// 类型定义，从协议定义自动生成
    /// </summary>
    public class ClassDef
    {
        /// <summary>
        /// 内部类型
        /// </summary>
        public List<ClassDef> Classes { get; set; } = new List<ClassDef>();

        /// <summary>
        /// 静态字符串
        /// </summary>
        public Dictionary<string, (string value, string description)> ConstString { get; private set; } 
            = new Dictionary<string, (string value, string description)>();

        /// <summary>
        /// 基类
        /// </summary>
        public ClassDef Base { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 类型分类，用于后续代码生成器的选择
        /// </summary>
        public string Category { get; set; } = "";

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 成员定义
        /// </summary>
        public Dictionary<string, MemberDef> Members { get; private set; } 
            = new Dictionary<string, MemberDef>();

        public ClassDef(string name, string description, ClassDef baseClass = null)
        {
            Name = name;
            Description = description;
            Base = baseClass;
        }

        public string ToString(int depth)
        {
            var retraction = new string('\t', depth);
            var memberStr = "";

            foreach (var constStr in ConstString)
                memberStr += $"{retraction}\tconst string {constStr.Key} = \"{constStr.Value.value}\"; //{constStr.Value.description}\n";

            foreach (var innerClass in Classes)
                memberStr += $"{retraction}\t{innerClass.ToString(depth + 1)}\n";

            foreach (var member in Members)
                memberStr += $"{retraction}\t{member.Value}\n";

            return $"class {Name} : {(Base == null ? "Object" : Base.Name)} //({Category}){Description} \n{retraction}{{\n{memberStr}{retraction}}}\n";
        }

        public override string ToString()
        {
            return ToString(0);
        }
    }
}
