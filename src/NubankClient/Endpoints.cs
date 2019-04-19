using NubankClient.Http;
using System.Collections.Generic;

namespace NubankClient
{
    class Endpoints
    {
        private readonly IHttpClient _client;

        private Dictionary<string, string> _topLevelUrls;
        private Dictionary<string, string> _autenticatedUrls;
        private Dictionary<string, string> _appUrls;
        private const string DiscoveryUrl = "https://prod-s0-webapp-proxy.nubank.com.br/api/discovery";
        private const string DiscoveryAppUrl = "https://prod-s0-webapp-proxy.nubank.com.br/api/app/discovery";

        public Endpoints(IHttpClient httpClient)
        {
            _client = httpClient;
        }

        public string Login => GetTopLevelUrl("login");
        public string ResetPassword => GetTopLevelUrl("reset_password");
        public string Events => GetAutenticatedUrl("events");
        public string Lift => GetAppUrl("lift");

        public Dictionary<string, string> AutenticatedUrls { set => _autenticatedUrls = value; }

        public string GetTopLevelUrl(string key)
        {
            if (_topLevelUrls == null)
            {
                Discover();
            }
            return GetKey(key, _topLevelUrls);
        }

        public string GetAppUrl(string key)
        {
            if (_appUrls == null)
            {
                DiscoverApp();
            }
            return GetKey(key, _appUrls);
        }

        public string GetAutenticatedUrl(string key)
        {
            return GetKey(key, _autenticatedUrls);
        }

        private void Discover()
        {
            var response = _client.GetAsync<Dictionary<string, string>>(DiscoveryUrl)
                .GetAwaiter().GetResult();
            _topLevelUrls = response;
        }

        private void DiscoverApp()
        {
            var response = _client.GetAsync<Dictionary<string, string>>(DiscoveryAppUrl)
                .GetAwaiter().GetResult();
            _appUrls = response;
        }

        private string GetKey(string key, Dictionary<string, string> source)
        {
            if (!source.ContainsKey(key))
            {
                return null;
            }
            return source[key];
        }
    }
}