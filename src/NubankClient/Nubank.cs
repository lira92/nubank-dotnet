using NubankClient.Http;
using NubankClient.Model;
using NubankClient.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace NubankClient
{
    public class Nubank
    {
        private readonly string _login;
        private readonly string _password;
        private readonly IHttpClient _client;
        private readonly Endpoints _endpoints;
        private string AuthToken { get; set; }
        private string RefreshToken { get; set; }

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

        public async Task<LoginResponse> Login()
        {
            await GetToken();

            if (_endpoints.Events != null)
            {
                return new LoginResponse();
            }

            await TryToAutenticateWithRefreshToken();

            if (_endpoints.Events != null)
            {
                return new LoginResponse();
            }

            return new LoginResponse(Guid.NewGuid().ToString());
        }

        private async Task TryToAutenticateWithRefreshToken()
        {
            var body = new
            {
                grant_type = "refresh_token",
                refresh_token = RefreshToken,
                client_id = "other.conta",
                client_secret=  "yQPeLzoHuJzlMMSAjC-LgNUJdUecx8XO"
            };
            var response = await _client.PostAsync<Dictionary<string, object>>(_endpoints.Login, body);

            FillTokens(response);

            FillAutenticatedUrls(response);
        }

        private async Task GetToken()
        {
            var body = new
            {
                client_id = "other.conta",
                client_secret = "yQPeLzoHuJzlMMSAjC-LgNUJdUecx8XO",
                grant_type = "password",
                login = _login,
                password = _password
            };
            var response = await _client.PostAsync<Dictionary<string, object>>(_endpoints.Login, body);

            FillTokens(response);

            FillAutenticatedUrls(response);
        }

        public async Task AutenticateWithQrCode(string code)
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                await GetToken();
            }

            var payload = new
            {
                qr_code_id = code,
                type = "login-webapp"
            };

            var response = await _client.PostAsync<Dictionary<string, object>>(_endpoints.Lift, payload, GetHeaders());

            FillTokens(response);

            FillAutenticatedUrls(response);
        }

        private void FillTokens(Dictionary<string, object> response)
        {
            if (!response.Keys.Any(x => x == "access_token"))
            {
                if (response.Keys.Any(x => x == "error"))
                {
                    throw new AuthenticationException(response["error"].ToString());
                }
                throw new AuthenticationException("Unknow error occurred on trying to do login on Nubank using the entered credentials");
            }
            AuthToken = response["access_token"].ToString();
            RefreshToken = response["refresh_token"].ToString();
        }

        private void FillAutenticatedUrls(Dictionary<string, object> response)
        {
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