using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
                // 直接传递完整Request
                var apiWithRequestObject = $@"
        public Api.{api.apiDef.Name}.Response {api.name}(Api.{api.apiDef.Name}.Request request)
        {{
            Api.{api.apiDef.Name}.Response response = new();
            Send(request, {"\""}{api.cmd}{"\""}, {"\""}{api.method}{"\""}, {"\""}{api.contentType}{"\""}, response); 
            return response;
        }}";

                var members = api.apiDef.Classes
                        .Where(c => c.Name == "Request")
                        .First().Members.Values;

                // 展开参数的调用
                var expandArgs
                    = members.Select(memberDef =>
                        {
                            return ObjectGenerator.GetMemberSource(memberDef, true);
                        });

                // 构造请求
                var requestConstruct = $@"Api.{api.apiDef.Name}.Request request = new
            {{
                {string.Join($",{Environment.NewLine}\t\t\t\t", members.Select(memberDef => $"{FormatNameToUpperCamel(memberDef.Name)} = {FormatNameToLowerCamel(memberDef.Name)}"))}
            }};";

                var apiExpandArgs = $@"
        public Api.{api.apiDef.Name}.Response {api.name}({string.Join(", ", expandArgs)})
        {{
            Api.{api.apiDef.Name}.Response response = new();
            {requestConstruct}
            Send(request, {"\""}{api.cmd}{"\""}, {"\""}{api.method}{"\""}, {"\""}{api.contentType}{"\""}, response); 
            return response;
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
        partial void Send<TRequest, TResponse>(TRequest request, string cmd, string method, string contentType, TResponse response);
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
