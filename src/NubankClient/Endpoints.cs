using System;
using System.Collections.Generic;
using NubankClient.Http;
using RestSharp;

namespace NubankClient
{
    class Endpoints
    {
        private readonly IHttpClient _client;

        private Dictionary<string, string> _topLevelUrls;
        private Dictionary<string, string> _autenticatedUrls;
        private const string DISCOVERY = "https://prod-s0-webapp-proxy.nubank.com.br/api/discovery";

        public Endpoints(IHttpClient httpClient)
        {
            _client = httpClient;
        }

        public string Login { get => GetTopLevelUrl("login"); }
        public string ResetPassword { get => GetTopLevelUrl("reset_password"); }
        public string Events { get => GetAutenticatedUrl("events"); }

        public Dictionary<string, string> AutenticatedUrls { set => _autenticatedUrls = value; }

        public string GetTopLevelUrl(string key)
        {
            if (_topLevelUrls == null)
            {
                Discover();
            }
            return _topLevelUrls[key];
        }

        public string GetAutenticatedUrl(string key)
        {
            return _autenticatedUrls[key];
        }

        private void Discover()
        {
            var response = _client.GetAsync<Dictionary<string, string>>(DISCOVERY)
                .GetAwaiter().GetResult();
            _topLevelUrls = response;
        }
    }
}