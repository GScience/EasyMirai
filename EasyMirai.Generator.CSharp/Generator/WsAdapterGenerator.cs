using EasyMirai.Generator;
using EasyMirai.Generator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EasyMirai.Generator.CSharp.Extensions;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class WsAdapterGenerator : GeneratorBase
    {
        public static string RootNamespace => $"{MiraiSource.RootNamespace}.Adapter";

        public List<(string name, ClassDef apiDef, string cmd)> ApiList 
            = new List<(string name, ClassDef apiDef, string cmd)>();

        public override void PreProcessing(ClassDef classDef)
        {
            base.PreProcessing(classDef);
            classDef.Namespace = MiraiSource.RootNamespace;
            var apiFuncs = classDef.Members.Values.Where(
                m => m.Type == MemberType.Object &&
                m.Reference.Category == MiraiModule.CategoryIFunctione);
            foreach (var apiFunc in apiFuncs)
            {
                var apiName = apiFunc.Name;
                var apiType = classDef.Members.Values.Where(m => m.Name == apiName + "Api").FirstOrDefault();
                if (classDef.ConstString.TryGetValue(apiName + "Cmd", out var apiCmd))
                    ApiList.Add((apiName, apiType?.Reference, apiCmd.value));
                else
                    ApiList.Add((apiName, apiType?.Reference, ""));
            }
        }

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            // 列表查找IFunction命令
            var apiFuncDefs = ApiList.Select(api =>
            {
                var requestClassDef = api.apiDef.Classes
                    .Where(c => c.Name == "Request").FirstOrDefault();

                var commonComment = $@"
        /// <summary>
        /// {api.apiDef.Description}
        /// </summary>
        /// <returns>Response</returns>";

                // 直接传递完整Request
                var apiWithRequestObject = $@"{commonComment}
        /// <remarks>
        /// 需要手动对 SessionKey 成员赋值<br/>
        /// Version: {api.apiDef.Version}
        /// </remarks>
        public async Task<Api.{api.apiDef.Name}.Response> {api.name}Async(Api.{api.apiDef.Name}.Request request)
        {{
            return await SendAsync<Api.{api.apiDef.Name}.Request, Api.{api.apiDef.Name}.Response>(
                request, ""{api.cmd}"");
        }}";

                // 拆开参数逐个输出
                var apiExpandArgs = $@"{commonComment}
        {requestClassDef.ExpandParamComment(new[] { "sessionKey" }, 2)}
        /// <remarks>
        /// 自动处理 SessionKey<br/>
        /// Version: {api.apiDef.Version}
        /// </remarks>
        public async Task<Api.{api.apiDef.Name}.Response> {api.name}Async({requestClassDef.ExpandArgs(new[]{ "sessionKey" }, true)})
        {{
            Api.{api.apiDef.Name}.Request request = new()
            {{
                {requestClassDef.ExpandCtor(4)}
            }};
            return await {api.name}Async(request); 
        }}";
                return apiWithRequestObject + Environment.NewLine + apiExpandArgs;
            });

            string source = $@"{base.GenerateFrom(classDef, namespaceDef + ".Adapter")}
#nullable enable
using System;
using System.Text.Json.Serialization;
using {EventGenerator.RootNamespace};
using {MessageGenerator.RootNamespace};
using {ApiGenerator.RootNamespace};
using {SerializeGenerator.RootNamespace};

namespace {RootNamespace}
{{
    /// <summary>
    /// Ws Adapter
    /// </summary>
    public sealed partial class {classDef.Name} : IDisposable
    {{
        /// <summary>
        /// Session Key
        /// </summary>
        public string sessionKey = """";
{string.Join(Environment.NewLine, apiFuncDefs)}
    }}
}}
#nullable restore";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Adapter";
        }
    }
}
