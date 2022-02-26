using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using EasyMirai.Generator.CSharp.Extensions;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class HttpAdapterGenerator : GeneratorBase
    {
        public List<(string name, ClassDef apiDef, string cmd, string method, string contentType)> ApiList
               = new List<(string name, ClassDef apiDef, string cmd, string method, string contentType)>();

        public override void PreProcessing(ClassDef classDef)
        {
            base.PreProcessing(classDef);
            var apiFuncs = classDef.Members.Values.Where(
                m => m.Type == MemberType.Object &&
                m.Reference.Category == MiraiModule.CategoryIFunctione);
            foreach (var apiFunc in apiFuncs)
            {
                var apiName = apiFunc.Name;
                var apiType = classDef.Members.Values.Where(m => m.Name == apiName + "Api").FirstOrDefault();

                classDef.ConstString.TryGetValue(apiName + "Cmd", out var apiCmd);
                classDef.ConstString.TryGetValue(apiName + "Method", out var apiMethod);
                classDef.ConstString.TryGetValue(apiName + "ContentType", out var apiContentType);

                ApiList.Add((apiName, apiType?.Reference, apiCmd.value, apiMethod.value, apiContentType.value));
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
        /// 需要手动对 SessionKey 成员赋值
        /// </remarks>
        public async Task<Api.{api.apiDef.Name}.Response> {api.name}Async(Api.{api.apiDef.Name}.Request request)
        {{
            return await SendAsync<Api.{api.apiDef.Name}.Request, Api.{api.apiDef.Name}.Response>(
                request, ""{api.cmd}"", ""{api.method}"", ""{api.contentType}""); 
        }}";

                // 拆开参数逐个输出
                var apiExpandArgs = $@"{commonComment}
        {requestClassDef.ExpandParamComment(new[] { "sessionKey" }, 2)}
        /// <remarks>
        /// 自动处理 SessionKey
        /// </remarks>
        public async Task<Api.{api.apiDef.Name}.Response> {api.name}Async({requestClassDef.ExpandArgs(new[] { "sessionKey" })})
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

using System;
using System.Text.Json.Serialization;
using {namespaceDef}.Api;
using {namespaceDef}.Message;

namespace {namespaceDef}.Adapter
{{
    public partial class {classDef.Name}
    {{
        /// <summary>
        /// Session Key
        /// </summary>
        public string sessionKey = """";
{string.Join(Environment.NewLine, apiFuncDefs)}
    }}
}}
";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Adapter";
        }

        public override void PostProcessing()
        {

        }
    }
}
