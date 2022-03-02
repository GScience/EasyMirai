using EasyMirai.CSharp.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EasyMirai.CSharp
{
    /// <summary>
    /// Mirai配置
    /// </summary>
    public class MiraiConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 12345;
        public string VerifyKey { get; set; } = "[YourVerifyKeyHere]";
        public long Id { get; set; } = 123456789;
        public AdapterTypes[] Adapters { get; set; } = new[] { AdapterTypes.Http, AdapterTypes.Ws };

        /// <summary>
        /// 从文件中加载配置
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static MiraiConfig FromFile(string filePath)
        {
            using var configStream = File.OpenRead("config.json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            return JsonSerializer.Deserialize<MiraiConfig>(configStream, options)!;
        }
    }
}
