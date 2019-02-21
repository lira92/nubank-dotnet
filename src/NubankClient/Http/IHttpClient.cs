using System.Collections.Generic;
using System.Threading.Tasks;

namespace NubankClient.Http
{
    public interface IHttpClient
    {
        Task<T> GetAsync<T>(string url) where T : new();
        Task<T> GetAsync<T>(string url, Dictionary<string, string> headers) where T : new();
        Task<T> PostAsync<T>(string url, object body) where T : new();
        Task<T> PostAsync<T>(string url, object body, Dictionary<string, string> headers) where T : new();
    }
}
