using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Extensions
{
    /// <summary>
    /// 成员扩展
    /// </summary>
    internal static class MemberExtensions
    {
        /// <summary>
        /// C# 中列表对应类型
        /// </summary>
        public static string List = "global::System.Collections.Generic.IEnumerable";

        /// <summary>
        /// 获取成员对应 C# 类型
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetCSharpMemberType(this MemberDef memberDef)
        {
            string memberTypeName;

            switch (memberDef.Type)
            {
                case MemberType.String:
                    memberTypeName = "string";
                    break;
                case MemberType.StringList:
                    memberTypeName = $"{List}<string>";
                    break;
                case MemberType.Boolean:
                    memberTypeName = "bool";
                    break;
                case MemberType.BooleanList:
                    memberTypeName = $"{List}<bool>";
                    break;
                case MemberType.Int:
                    memberTypeName = $"int";
                    break;
                case MemberType.IntList:
                    memberTypeName = $"{List}<int>";
                    break;
                case MemberType.Long:
                    memberTypeName = "long";
                    break;
                case MemberType.LongList:
                    memberTypeName = $"{List}<long>";
                    break;
                case MemberType.Object:
                    memberTypeName = memberDef.Reference.FullName;
                    break;
                case MemberType.ObjectList:
                    memberTypeName = $"{List}<{memberDef.Reference.FullName}>";
                    break;
                case MemberType.Stream:
                    memberTypeName = $"System.IO.Stream";
                    break;
                default:
                    throw new NotImplementedException($"Unknown type {memberDef.Type}");
            }

            return memberTypeName;
        }

        /// <summary>
        /// 获取列表成员对应 C# 类型
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetCSharpMemberListComponentType(this MemberDef memberDef)
        {
            string memberTypeName;

            switch (memberDef.Type)
            {
                case MemberType.StringList:
                    memberTypeName = $"string";
                    break;
                case MemberType.BooleanList:
                    memberTypeName = $"bool";
                    break;
                case MemberType.IntList:
                    memberTypeName = $"int";
                    break;
                case MemberType.LongList:
                    memberTypeName = $"long";
                    break;
                case MemberType.ObjectList:
                    memberTypeName = $"{memberDef.Reference.FullName}";
                    break;
                default:
                    throw new NotImplementedException($"Unknown type {memberDef.Type}");
            }

            return memberTypeName;
        }

        /// <summary>
        /// 获取成员声明
        /// </summary>
        /// <param name="memberDef"></param>
        /// <param name="useLowerCamel"></param>
        /// <returns></returns>
        public static string GetCSharpMemberDefine(this MemberDef memberDef, bool useLowerCamel = false, bool allowNull = false)
        {
            var memberTypeName = GetCSharpMemberType(memberDef);
            if (useLowerCamel)
                return $"{memberTypeName}{(allowNull ? "?" : "")} {memberDef.Name.ToLowerCamel()}";
            return $"{memberTypeName}{(allowNull ? "?" : "")} {memberDef.Name.ToUpperCamel()}";
        }

        /// <summary>
        /// 获取列表成员对应的类型
        /// </summary>
        /// <param name="memberDef"></param>
        /// <returns></returns>
        public static MemberType GetListComponentType(this MemberDef memberDef)
        {
            MemberType type;

            switch (memberDef.Type)
            {
                case MemberType.BooleanList:
                    type = MemberType.Boolean;
                    break;
                case MemberType.StringList:
                    type = MemberType.String;
                    break;
                case MemberType.IntList:
                    type = MemberType.Int;
                    break;
                case MemberType.LongList:
                    type = MemberType.Long;
                    break;
                case MemberType.ObjectList:
                    type = MemberType.Object;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return type;
        }
    }
}
