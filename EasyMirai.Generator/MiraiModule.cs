using EasyMirai.Generator.Extensions;
using EasyMirai.Generator.Module;
using EasyMirai.Generator.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMirai.Generator
{
    /// <summary>
    /// Mirai 模块，包含所有具体的类型定义
    /// </summary>
    public class MiraiModule
    {
        /// <summary>
        /// 仅生成期间使用 Tag
        /// </summary>
        public const string TagOnlyForGenerate = "[OnlyForGenerating] ";

        public const string CategoryAdapterWs = "Adapter.Ws"; 
        public const string CategoryAdapterHttp = "Adapter.Http";
        public const string CategoryMessage = "Message";
        public const string CategoryIMessage = "Interface.Message";
        public const string CategoryIFunctione = "Interface.Function";
        public const string CategoryApi = "Api";
        public const string CategoryApiRequest = "Api.Request";
        public const string CategoryApiResponse = "Api.Response";
        public const string CategoryObject = "Object";
        public const string CategoryEvent = "Event";

        /// <summary>
        /// 已经加载的所有对象
        /// </summary>
        public Dictionary<ProtocolComponent, ClassDef> ClassTable = new Dictionary<ProtocolComponent, ClassDef>();

        public List<ClassDef> Classes { get; private set; } = new List<ClassDef>();
        private ClassDef _wsAdapter { get; set; }
        private ClassDef _httpAdapter { get; set; }

        private ClassDef _messageBaseDef;
        private ClassDef _functionDef;

        public MiraiModule(MiraiProtocol protocol)
        {
            _wsAdapter = AddInternalClass(protocol, "WsAdapter", "Websocket Adapter");
            _wsAdapter.Category = CategoryAdapterWs;
            _httpAdapter = AddInternalClass(protocol, "HttpAdapter", "Http Adapter");
            _httpAdapter.Category = CategoryAdapterHttp;

            _messageBaseDef = AddInternalClass(protocol, "Message", "通用消息接口");
            _functionDef = AddInternalClass(protocol, "Function", "函数");

            _messageBaseDef.Name = "IMessage";
            _messageBaseDef.Category = CategoryIMessage;
            _functionDef.Name = "IFunction";
            _functionDef.Category = CategoryIFunctione;

            foreach (var obj in protocol.Objects)
                Classes.Add(FromObjectDef(obj));
            foreach (var msg in protocol.Messages)
                Classes.Add(FromMessageDef(msg));
            foreach (var api in protocol.Apis)
                Classes.Add(FromApiDef(api));
            foreach (var eventDef in protocol.Events)
                Classes.Add(FromEventDef(eventDef));
        }

        private ClassDef AddInternalClass(MiraiProtocol protocol, string name, string description)
        {
            var objDef = protocol.AddObjectDef(name, description);
            return FromObjectDef(objDef);
        }

        /// <summary>
        /// 从Api定义中创建类型，若已经加载则直接返回
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private ClassDef FromApiDef(ApiDef apiDef)
        {
            if (ClassTable.ContainsKey(apiDef))
                return ClassTable[apiDef];

            var classDef = new ClassDef(apiDef.Name, apiDef.Description, null);

            classDef.Version = apiDef.ProtocolVersion;
            classDef.Category = CategoryApi;

            var requestClass = FromObjectDef(apiDef.Request, null, CategoryApiRequest);
            requestClass.Name = "Request";
            requestClass.Description = apiDef.Name + ".Request";
            var responseClass = FromObjectDef(apiDef.Response, null, CategoryApiResponse);
            responseClass.Name = "Response";
            responseClass.Description = apiDef.Name + ".Response";

            if (apiDef.HttpAdapter != null)
            {
                var cmd = apiDef.HttpAdapter.Command;
                var contentType = apiDef.HttpAdapter.ContentType;
                var method = apiDef.HttpAdapter.Method;

                var http = new MemberDef(apiDef.Name, apiDef.Description, MemberType.Object, _functionDef);
                _httpAdapter.Members[http.Name] = http;
                _httpAdapter.ConstString[apiDef.Name + "Cmd"] = (cmd, TagOnlyForGenerate + "命令字");
                _httpAdapter.ConstString[apiDef.Name + "ContentType"] = (contentType, TagOnlyForGenerate + "Post 请求时内容类型");
                _httpAdapter.ConstString[apiDef.Name + "Method"] = (method.ToString(), TagOnlyForGenerate + "请求方法");

                var api = new MemberDef(apiDef.Name + "Api", TagOnlyForGenerate + apiDef.Description, MemberType.Object, classDef);
                _httpAdapter.Members[api.Name] = api;
            }
            if (apiDef.WsAdapter != null)
            {
                var cmd = apiDef.WsAdapter.Command;

                var ws = new MemberDef(apiDef.Name, apiDef.Description, MemberType.Object, _functionDef);
                _wsAdapter.Members[apiDef.Name] = ws;
                _wsAdapter.ConstString[apiDef.Name + "Cmd"] = (cmd, TagOnlyForGenerate + "命令字");

                var api = new MemberDef(apiDef.Name + "Api", TagOnlyForGenerate + apiDef.Description, MemberType.Object, classDef);
                _wsAdapter.Members[api.Name] = api;
            }

            classDef.Classes.Add(requestClass); 
            classDef.Classes.Add(responseClass);

            ClassTable[apiDef] = classDef;

            return classDef;
        }


        /// <summary>
        /// 从消息定义中创建类型，若已经加载则直接返回
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private ClassDef FromMessageDef(MessageDef msg)
        {
            var classDef = FromObjectDef(msg, _messageBaseDef, CategoryMessage);
            classDef.ConstString["Type"] = (msg.Name, TagOnlyForGenerate + "消息类型");
            return classDef;
        }

        /// <summary>
        /// 从事件定义中创建类型，若已经加载则直接返回
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private ClassDef FromEventDef(EventDef msg)
        {
            var classDef = FromObjectDef(msg, _messageBaseDef, CategoryEvent);
            classDef.ConstString["Type"] = (msg.Name, TagOnlyForGenerate + "事件类型");
            return classDef;
        }

        /// <summary>
        /// 从对象定义中创建类型，若已经加载则直接返回
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private ClassDef FromObjectDef(ObjectDef obj)
        {
            return FromObjectDef(obj, null, CategoryObject);
        }

        /// <summary>
        /// 从对象定义中创建类型，若已经加载则直接返回
        /// </summary>
        /// <param name="obj">对象描述</param>
        /// <param name="objBase">基类</param>
        /// <param name="category">分类，用于代码生成器的选择</param>
        /// <returns></returns>
        private ClassDef FromObjectDef(ObjectDef obj, ClassDef objBase, string category)
        {
            if (ClassTable.ContainsKey(obj))
                return ClassTable[obj];

            var classDef = new ClassDef(obj.Name, obj.Description, objBase);

            classDef.Version = obj.ProtocolVersion;
            classDef.Category = category;

            // 加载值成员
            foreach (var valueDef in obj.Values)
            {
                MemberType memberType;
                switch (valueDef.Value.valDef)
                {
                    case ValDef.Boolean:
                        memberType = MemberType.Boolean;
                        break;
                    case ValDef.Int:
                        memberType = MemberType.Int;
                        break;
                    case ValDef.Long:
                        memberType = MemberType.Long;
                        break;
                    case ValDef.String:
                        memberType = MemberType.String;
                        break;
                    default:
                        throw new NotImplementedException();
                };

                var member = new MemberDef(valueDef.Key, valueDef.Value.description, memberType);
                classDef.Members[valueDef.Key] = member;
            }

            // 加载对象成员
            foreach (var objDef in obj.Objects)
            {
                var memberType = MemberType.Object;
                var typeDef = FromObjectDef(objDef.Value.objectDef);
               
                // 给内部匿名对象一个名称
                typeDef.Name = obj.Name + objDef.Key.FirstToUpper();
                typeDef.Description = objDef.Value.description;
                classDef.Classes.Add(typeDef);

                var member = new MemberDef(objDef.Key, objDef.Value.description, memberType, typeDef);
                classDef.Members[objDef.Key] = member;
            }

            // 加载引用成员
            foreach (var refDef in obj.References)
            {
                var memberType = MemberType.Object;
                var refTypeDef = refDef.Value.objectRef.TypeDef;
                if (refTypeDef == null)
                    throw new Exception("Failed to find reference " + refDef.Value.objectRef);

                var refObject = FromObjectDef(refTypeDef);

                var member = new MemberDef(refDef.Key, refDef.Value.description, memberType, refObject);
                classDef.Members[refDef.Key] = member;
            }

            // 加载值列表
            foreach (var valueDef in obj.ValueList)
            {
                MemberType memberType;
                switch (valueDef.Value.valDef)
                {
                    case ValDef.Boolean:
                        memberType = MemberType.BooleanList;
                        break;
                    case ValDef.Int:
                        memberType = MemberType.IntList;
                        break;
                    case ValDef.Long:
                        memberType = MemberType.LongList;
                        break;
                    case ValDef.String:
                        memberType = MemberType.StringList;
                        break;
                    default:
                        throw new NotImplementedException();
                };

                var member = new MemberDef(valueDef.Key, valueDef.Value.description, memberType);
                classDef.Members[valueDef.Key] = member;
            }

            // 加载对象成员列表
            foreach (var objDef in obj.ObjectList)
            {
                var memberType = MemberType.ObjectList;
                var typeDef = FromObjectDef(objDef.Value.objectDef);

                // 给内部匿名对象一个名称
                typeDef.Name = obj.Name + objDef.Key.FirstToUpper();
                typeDef.Description = objDef.Value.description;
                classDef.Classes.Add(typeDef);

                var member = new MemberDef(objDef.Key, objDef.Value.description, memberType, typeDef);
                classDef.Members[objDef.Key] = member;
            }

            // 加载引用列表
            foreach (var refDef in obj.ReferenceList)
            {
                var memberType = MemberType.ObjectList;
                var refTypeDef = refDef.Value.objectRef.TypeDef;
                if (refTypeDef == null)
                    throw new Exception("Failed to find reference " + refDef.Value.objectRef);

                var refObject = FromObjectDef(refTypeDef);

                var member = new MemberDef(refDef.Key, refDef.Value.description, memberType, refObject);
                classDef.Members[refDef.Key] = member;
            }

            ClassTable[obj] = classDef;

            return classDef;
        }
    }
}
