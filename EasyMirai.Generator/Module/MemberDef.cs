using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMirai.Generator.Module
{
    /// <summary>
    /// 成员类型
    /// </summary>
    public enum MemberType
    {
        Boolean, Int, Long, String, Object,
        BooleanList, IntList, LongList, StringList, ObjectList
    };

    /// <summary>
    /// 成员定义
    /// </summary>
    public class MemberDef
    {
        /// <summary>
        /// 成员名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public MemberType Type { get; private set; }

        /// <summary>
        /// 成员类型引用，只有当 Type 为 Object 时才不为空
        /// </summary>
        public ClassDef Reference { get; private set; }

        /// <summary>
        /// 成员描述
        /// </summary>
        public string Description { get; private set; }

        public MemberDef(string name, string description, MemberType type, ClassDef classDef = null)
        {
            Name = name;
            Description = description;
            Type = type;
            Reference = classDef;
        }

        public override string ToString()
        {
            if (Type == MemberType.Object)
                return $"{Reference.Name} {Name}; // {Description}";
            if (Type == MemberType.ObjectList)
                return $"List<{Reference.Name}> {Name}; // {Description}";

            switch (Type)
            {
                case MemberType.Boolean:
                case MemberType.Int:
                case MemberType.Long:
                case MemberType.String:
                    return $"{Type} {Name}; // {Description}";
                case MemberType.BooleanList:
                case MemberType.IntList:
                case MemberType.LongList:
                case MemberType.StringList:
                    var typeStr = Type.ToString();
                    return $"List<{typeStr.Substring(0, typeStr.Length - 4)}> {Name}; // {Description}";
                default:
                    throw new NotImplementedException($"Unknown type {Type}");
            }
        }
    }
}
