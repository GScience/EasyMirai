using EasyMirai.CSharp.Util;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyMirai.CSharp.Adapter
{
    public partial class HttpAdapter : IDisposable
    {
        private HttpClient _httpClient = new();

        private MiraiConfig _config;

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
            where TRequest : MiraiJsonSerializers.ISerializable<TRequest>, new()
            where TResponse : MiraiJsonSerializers.ISerializable<TResponse>, new()
        {
            HttpResponseMessage response;

            if (method == "Post")
            {
                var arrayBuffer = new ArrayBufferWriter<byte>();
                var writer = new Utf8JsonWriter(arrayBuffer);
                request.DefaultConverter.Write(writer, request);
                writer.Flush();
                var httpContent = new ReadOnlyMemoryContent(arrayBuffer.WrittenMemory);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                response = await _httpClient.PostAsync($"http://{_config.Host}:{_config.Port}{cmd}", httpContent);
            }
            else if (method == "Get")
            {
                response = await _httpClient.GetAsync($"http://{_config.Host}:{_config.Port}{cmd}");
            }
            else
                throw new NotImplementedException();

            var responseObj = new TResponse();
            using var responseStream = response.Content.ReadAsStream();
            using var responseReader = new StreamReader(responseStream);

            responseObj.DefaultConverter.Read(await responseReader.ReadToEndAsync(), responseObj);
            return responseObj;
        }

        public async Task StartAsync(CancellationToken token)
        {
            var verifyResponse = await VerifyAsync(_config.VerifyKey);
            sessionKey = verifyResponse.Session!;
            var bindResponse = await BindAsync(_config.Id);
        }

        internal HttpAdapter(MiraiConfig config)
        {
            _config = config;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
