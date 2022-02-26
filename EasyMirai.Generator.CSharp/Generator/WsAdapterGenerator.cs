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
        public List<(string name, ClassDef apiDef, string cmd)> ApiList 
            = new List<(string name, ClassDef apiDef, string cmd)>();

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
                var comment = $@"
        /// <summary>
        /// {api.apiDef.Description}
        /// </summary>
        /// <returns>Response</returns>";

                // 直接传递完整Request
                var apiWithRequestObject = $@"{comment}
        public Api.{api.apiDef.Name}.Response {api.name}(Api.{api.apiDef.Name}.Request request)
        {{
            Api.{api.apiDef.Name}.Response response = new();
            Send(request, {"\""}{api.cmd}{"\""}, response); 
            return response;
        }}";
                var requestClassDef = api.apiDef.Classes
                        .Where(c => c.Name == "Request").FirstOrDefault();

                // 构造请求
                var requestConstruct = $@"Api.{api.apiDef.Name}.Request request = new()
            {{
                {requestClassDef.ExpandCtor(4)}
            }};";

                var apiExpandArgs = $@"{comment}
        {requestClassDef.ExpandParamComment(2)}
        public Api.{api.apiDef.Name}.Response {api.name}({requestClassDef.ExpandArgs()})
        {{
            {requestConstruct}
            return {api.name}(request); 
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
        partial void Send<TRequest, TResponse>(TRequest request, string cmd, TResponse response);
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
