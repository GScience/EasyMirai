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
        public static string RootNamespace => $"{MiraiSource.RootNamespace}.Adapter";

        public List<(string name, ClassDef apiDef, string cmd, string method, string contentType)> ApiList
               = new List<(string name, ClassDef apiDef, string cmd, string method, string contentType)>();
        public override void PreProcessing(ClassDef classDef)
        {
            base.PreProcessing(classDef);
            classDef.Namespace = RootNamespace;
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

        /// <summary>
        /// 获取命令，Get方法直接把参数写到cmd里
        /// </summary>
        /// <returns></returns>
        public string GetCommandSource(ClassDef requestClassDef, string cmd, string method)
        {
            if (method == "Post")
                return cmd;

            var argsSource = string.Join("&", requestClassDef.Members.Values.Select(m =>
            {
                if (m.Type == MemberType.Boolean || m.Type == MemberType.Int || m.Type == MemberType.Long)
                    return $@"{{(request.{m.Name.ToUpperCamel()} != null ? $""{m.Name.ToLowerCamel()}={{request.{m.Name.ToUpperCamel()}.Value}}"" : """")}}";
                return $@"{{(request.{m.Name.ToUpperCamel()} != null ? $""{m.Name.ToLowerCamel()}={{request.{m.Name.ToUpperCamel()}}}"" : """")}}";
            }));

            if (string.IsNullOrEmpty(argsSource))
                return cmd;
            return $"{cmd}?{argsSource}";
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
                request, $""{GetCommandSource(requestClassDef, api.cmd, api.method)}"", ""{api.method}"", ""{api.contentType}""); 
        }}";

                // 拆开参数逐个输出
                var apiExpandArgs = $@"{commonComment}
        {requestClassDef.ExpandParamComment(new[] { "sessionKey" }, 2)}
        /// <remarks>
        /// 自动处理 SessionKey<br/>
        /// Version: {api.apiDef.Version}
        /// </remarks>
        public async Task<Api.{api.apiDef.Name}.Response> {api.name}Async({requestClassDef.ExpandArgs(new[] { "sessionKey" }, true)})
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
    /// Http Adapter
    /// </summary>
    public partial class {classDef.Name} : IDisposable
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
