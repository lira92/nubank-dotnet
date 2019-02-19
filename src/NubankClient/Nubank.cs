using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NubankClient.Model;
using RestSharp;

namespace NubankClient
{
    public class Nubank
    {
        private readonly string _login;
        private readonly string _password;
        private readonly IRestClient _client;
        private readonly Endpoints _endpoints;
        private string AuthToken { get; set; }

        public Nubank(string login, string password)
        {
            _login = login;
            _password = password;
            _client = new RestClient();
            _endpoints = new Endpoints();
        }

        public async Task Login()
        {
            _client.BaseUrl = new Uri(_endpoints.Login);
            var loginRequest = new RestRequest();
            loginRequest.AddJsonBody(new {
                client_id = "other.conta",
                client_secret = "yQPeLzoHuJzlMMSAjC-LgNUJdUecx8XO",
                grant_type = "password",
                login = _login,
                password = _password
            });
            var response = await _client.PostAsync<Dictionary<string, object>>(loginRequest);
            AuthToken = response["access_token"].ToString();
            var listLinks = ((Dictionary<string, object>)response["_links"])
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString()));
            _endpoints.AutenticatedUrls = listLinks.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            _client.BaseUrl = new Uri(_endpoints.Events);
            var eventsRequest = new RestRequest();
            return await _client.GetAsync<List<Event>>(eventsRequest);
        }
    }
}