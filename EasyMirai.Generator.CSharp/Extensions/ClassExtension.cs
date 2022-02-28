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
        public static string ExpandArgs(this ClassDef classDef, string[] ignoreArgs)
        {
            var members = classDef.Members.Values;
            return string.Join(", ", 
                members
                    .OrderBy(m => m.Type)
                    .ThenBy(m => m.Name)
                    .Where(memberDef => !ignoreArgs.Contains(memberDef.Name.ToLowerCamel()))
                    .Select(memberDef => memberDef.GetCSharpMemberDefine(true)));
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
