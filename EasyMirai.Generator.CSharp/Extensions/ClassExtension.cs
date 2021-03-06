using EasyMirai.Generator.CSharp.Generator;
using EasyMirai.Generator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Extensions
{
    internal static class ClassExtension
    {
        /// <summary>
        /// 展开参数
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static string ExpandArgs(this ClassDef classDef, string[] ignoreArgs, bool allowNull)
        {
            var members = classDef.Members.Values;
            return string.Join(", ", 
                members
                    .OrderBy(m => m.Type)
                    .ThenBy(m => m.Name)
                    .Where(memberDef => !ignoreArgs.Contains(memberDef.Name.ToLowerCamel()))
                    .Select(memberDef => memberDef.GetCSharpMemberDefine(true, allowNull) + (allowNull ? "=null" :"")));
        }

        /// <summary>
        /// 生成参数注释
        /// </summary>
        /// <param name="classDef"></param>
        /// <param name="ignoreArgs">忽略的参数</param>
        /// <param name="depth">深度/param>
        /// <returns></returns>
        public static string ExpandParamComment(this ClassDef classDef, string[] ignoreArgs, int depth = 0)
        {
            var members = classDef.Members.Values;
            var newLine = Environment.NewLine + new string('\t', depth);
            return string.Join(
                newLine, 
                members
                    .Where(memberDef => !ignoreArgs.Contains(memberDef.Name.ToLowerCamel()))
                    .Select(
                        memberDef => $"/// <param name=\"{memberDef.Name.ToLowerCamel()}\">{memberDef.Description}</param>"
                        ));
        }

        /// <summary>
        /// 获取所有成员包括基类中的成员
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static IEnumerable<MemberDef> GetMembersIncludingBase(this ClassDef classDef)
        {
            if (classDef.Base == null)
                return classDef.Members.Values;
            return classDef.Members.Values.Concat(classDef.Base.GetMembersIncludingBase());
        }

        /// <summary>
        /// 展开构造
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static string ExpandCtor(this ClassDef classDef, int depth = 0)
        {
            var members = classDef.Members.Values;
            var newLine = Environment.NewLine + new string('\t', depth);
            return 
                string.Join(
                    $",{newLine}", 
                    members.Select(memberDef => 
                        $"{memberDef.Name.ToUpperCamel()} = {memberDef.Name.ToLowerCamel()}"
                        ));
        }
    }
}
