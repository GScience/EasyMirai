using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyMirai.CSharp.Adapter
{
    public partial class WsAdapter
    {
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        private async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, string cmd)
        {
            var requestJson = JsonSerializer.Serialize(request);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("{\"a\":1}"));

            var response = await JsonSerializer.DeserializeAsync<TResponse>(ms);
            if (response == null)
                throw new Exception("Failed to deserialize object");
            return response;
        }
    }
}
