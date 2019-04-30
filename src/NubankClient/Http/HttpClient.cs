using RestSharp;
using RestSharp.Serializers.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestRequest = RestSharp.Serializers.Newtonsoft.Json.RestRequest;

namespace NubankClient.Http
{
    class HttpClient : IHttpClient
    {
        private readonly IRestClient _client = new RestClient();
        public HttpClient()
        {
            _client.AddHandler("application/json", new NewtonsoftJsonSerializer());
        }
        public async Task<T> GetAsync<T>(string url) where T : new()
        {
            _client.BaseUrl = new Uri(url);
            return await _client.GetAsync<T>(new RestRequest());
        }

        public async Task<T> GetAsync<T>(string url, Dictionary<string, string> headers) where T : new()
        {
            _client.BaseUrl = new Uri(url);
            var request = new RestRequest();
            headers.ToList().ForEach((KeyValuePair<string, string> header) =>
            {
                request.AddHeader(header.Key, header.Value);
            });
            var response = await Task.FromResult(_client.Get<T>(request));
            return response.Data;
        }

        public async Task<T> PostAsync<T>(string url, object body) where T : new()
        {
            _client.BaseUrl = new Uri(url);
            var request = new RestRequest();
            request.AddJsonBody(body);
            return await _client.PostAsync<T>(request);
        }

        public async Task<T> PostAsync<T>(string url, object body, Dictionary<string, string> headers) where T : new()
        {
            _client.BaseUrl = new Uri(url);
            var request = new RestRequest();
            headers.ToList().ForEach((KeyValuePair<string, string> header) => {
                request.AddHeader(header.Key, header.Value);
            });
            request.AddJsonBody(body);
            return await _client.PostAsync<T>(request);
        }
    }
}
