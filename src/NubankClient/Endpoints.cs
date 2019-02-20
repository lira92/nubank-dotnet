using System.Collections.Generic;
using RestSharp;

namespace NubankClient
{
    class Endpoints
    {
        private Dictionary<string, string> _topLevelUrls;
        private Dictionary<string, string> _autenticatedUrls;
        private const string DISCOVERY = "https://bd224b7a-1d36-4b61-984f-780297250e74.mock.pstmn.io/api/discovery";

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
            var client = new RestClient(DISCOVERY);
            var request = new RestRequest();
            var response = client.Get<Dictionary<string, string>>(request);
            _topLevelUrls = response.Data;
        }
    }
}