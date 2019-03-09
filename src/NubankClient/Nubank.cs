using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using NubankClient.Http;
using NubankClient.Model;
using RestSharp;

namespace NubankClient
{
    public class Nubank
    {
        private readonly string _login;
        private readonly string _password;
        private readonly IHttpClient _client;
        private readonly Endpoints _endpoints;
        private string AuthToken { get; set; }

        public Nubank(string login, string password)
        {
            _login = login;
            _password = password;
            _client = new HttpClient();
            _endpoints = new Endpoints(_client);
        }

        public Nubank(IHttpClient httpClient, string login, string password)
        {
            _login = login;
            _password = password;
            _client = httpClient;
            _endpoints = new Endpoints(_client);
        }

        public async Task Login()
        {
            var body = new {
                client_id = "other.conta",
                client_secret = "yQPeLzoHuJzlMMSAjC-LgNUJdUecx8XO",
                grant_type = "password",
                login = _login,
                password = _password
            };
            var response = await _client.PostAsync<Dictionary<string, object>>(_endpoints.Login, body);
            if (!response.Keys.Any(x => x == "access_token"))
            {
                if(response.Keys.Any(x => x == "error"))
                {
                    throw new AuthenticationException(response["error"].ToString());
                }
                throw new AuthenticationException("Unknow error occurred on trying to do login on Nubank using the entered credentials");
            }
            AuthToken = response["access_token"].ToString();
            var listLinks = ((Dictionary<string, object>)response["_links"]);
            var listLinksConverted = listLinks
                .Select(x => new KeyValuePair<string, string>(x.Key, (((Dictionary<string, object>)x.Value)["href"].ToString())));
            _endpoints.AutenticatedUrls = listLinksConverted.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                throw new InvalidOperationException("GetEvents requires the user to be logged in. Make sure that the Login method has been called.");
            }

            var response = await _client.GetAsync<GetEventsResponse>(_endpoints.Events, GetHeaders());
            return response.Events;
        }

        private Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string> {
                { "Authorization", $"Bearer {AuthToken}" }
            };
        }
    }
}