using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ProtocolGenerator.Protocol;

namespace ProtocolGenerator
{
    using IEnumerableVersioningXmlPath = IEnumerable<(Version version, string path)>;
    using VersioningPathDictionary = Dictionary<string, (Version version,string path)>;

    /// <summary>
    /// Mirai协议
    /// </summary>
    public class MiraiProtocol
    {
        public const string ProtocolNamespace = "ProtocolGenerator.Protocol.";

        public List<ObjectDef> Objects { get; set; } = new List<ObjectDef>();
        public List<ApiDef> Apis { get; set; } = new List<ApiDef>();
        public List<MessageDef> Messages { get; set; } = new List<MessageDef>();

        public MiraiProtocol()
        {
            LoadFromAssembly(GetType().Assembly, ProtocolNamespace);
        }

        /// <summary>
        /// 加入内置对象描述
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="description">描述</param>
        public ObjectDef AddObjectDef(string name, string description)
        {
            var objDef = new ObjectDef(name, description, true);
            Objects.Add(objDef);
            return objDef;
        }

        /// <summary>
        /// 比较版本并自动覆盖老版本协议
        /// </summary>
        /// <param name="dict">带有版本号的协议路径</param>
        /// <param name="protocolPath">协议路径，经过处理后的资源路径，用来辨识文件是否指向同一文件</param>
        /// <param name="version">当前协议版本</param>
        /// <param name="resourcePath">资源路径，用于访问对应的协议定义xml文件</param>
        private void CompairVersionAndAdd(VersioningPathDictionary dict, string protocolPath, Version version, string resourcePath)
        {
            if (dict.TryGetValue(protocolPath, out var value) && value.version > version)
                return;
            dict[protocolPath] = (version, resourcePath);
        }

        /// <summary>
        /// 加载协议
        /// </summary>
        private void LoadFromAssembly(Assembly assembly, string protocolNamespace)
        {
            var resourcePaths = assembly.GetManifestResourceNames().Where(s => s.StartsWith(protocolNamespace));

            var ObjectPaths     = new VersioningPathDictionary();
            var ApiPaths        = new VersioningPathDictionary();
            var MessagePaths    = new VersioningPathDictionary();

            foreach (var resourcePath in resourcePaths)
            {
                var resourcePathWithoutNamespace = resourcePath.Substring(protocolNamespace.Length);

                var args = resourcePathWithoutNamespace.Split('.');

                // 长度不对劲
                if (args.Length < 4)
                    Console.WriteLine($"Can not resolve resource name {resourcePath}");

                // 不是xml
                if (args[args.Length - 1].ToLower() != "xml")
                    continue;

                Version version;

                // 获取协议版本号相关内容
                if (args[0].StartsWith("v"))
                {
                    // 获取版本号字符串
                    var versionName = args[0].Substring(1).Replace('_', '.');
                    version = Version.Parse(versionName);
                }
                else if (args[0] == "Common")
                {
                    // 通用协议
                    version = new Version(1, 0, 0);
                }
                else
                {
                    // 未知
                    Console.WriteLine($"Unknown version {args[0]}");
                    continue;
                }

                var protocolPath = args[1] + "." + args[2];
                // 根据类型选择加载方式
                switch (args[1])
                {
                    case "Object":
                        CompairVersionAndAdd(ObjectPaths, protocolPath, version, resourcePath);
                        break;
                    case "Api":
                        CompairVersionAndAdd(ApiPaths, protocolPath, version, resourcePath);
                        break;
                    case "Message":
                        CompairVersionAndAdd(MessagePaths, protocolPath, version, resourcePath);
                        break;
                }
            }
            LoadObject(ObjectPaths.Values);
            LoadApi(ApiPaths.Values);
            LoadMessage(MessagePaths.Values);
        }

        private static XmlDocument LoadXmlDocument(string path)
        {
            var document = new XmlDocument();
            using (var stream = typeof(MiraiProtocol).Assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    throw new Exception($"{path} not found");
                document.Load(stream);
            }
            return document;
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        /// <param name="version"></param>
        /// <param name="resourcePath"></param>
        private void LoadObject(IEnumerableVersioningXmlPath resourcePaths)
        {
            foreach (var path in resourcePaths)
            {
                var document = LoadXmlDocument(path.path);
                var obj = new ObjectDef();
                obj.FromXml(path.version, document.DocumentElement);
                Objects.Add(obj);
            }
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        /// <param name="version"></param>
        /// <param name="resourcePath"></param>
        private void LoadApi(IEnumerableVersioningXmlPath resourcePaths)
        {
            foreach (var path in resourcePaths)
            {
                var document = LoadXmlDocument(path.path);
                var api = new ApiDef();
                api.FromXml(path.version, document.DocumentElement);
                Apis.Add(api);
            }
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        /// <param name="version"></param>
        /// <param name="resourcePath"></param>
        private void LoadMessage(IEnumerableVersioningXmlPath resourcePaths)
        {
            foreach (var path in resourcePaths)
            {
                var document = LoadXmlDocument(path.path);
                var mgs = new MessageDef();
                mgs.FromXml(path.version, document.DocumentElement);
                Messages.Add(mgs);
            }
        }
    }
}
