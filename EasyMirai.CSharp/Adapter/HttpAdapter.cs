using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyMirai.CSharp.Adapter
{
    public partial class HttpAdapter : IDisposable
    {
        MemoryStream testStream = new();

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="cmd"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        private async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, string cmd, string method, string contentType)
            where TRequest : Util.MiraiJsonSerializers.ISerializable<TRequest>, new()
            where TResponse : Util.MiraiJsonSerializers.ISerializable<TResponse>, new()
        {
            var writer = new Utf8JsonWriter(testStream);
            request.DefaultConverter.Write(writer, request);
            writer.Flush();
            var requestJson = Encoding.UTF8.GetString(testStream.ToArray());
            testStream.Seek(0, SeekOrigin.Begin);

            var response = new TResponse();
            response.DefaultConverter.Read("{}", response);
            if (response == null)
                throw new Exception("Failed to deserialize object");
            return response;
        }

        public HttpAdapter(MiraiConfig config)
        {

        }

        /// <summary>
        /// 由已建立的连接创建
        /// </summary>
        /// <param name="key"></param>
        public HttpAdapter(string key)
        {
            sessionKey = key;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
