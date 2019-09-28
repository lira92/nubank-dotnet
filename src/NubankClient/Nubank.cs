using Newtonsoft.Json.Linq;
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
        private const string StatementGraphQl = "{\n   \"query\": \"{\\r    viewer {\\r        savingsAccount {\\r            feed {\\r                id\\r                __typename\\r                title\\r                detail\\r                postDate\\r                ... on TransferInEvent {\\r                    amount\\r                    originAccount {\\r                        name\\r                    }\\r                }\\r                ... on TransferOutEvent {\\r                    amount\\r                    destinationAccount {\\r                        name\\r                    }\\r                }\\r                ... on BarcodePaymentEvent {\\r                    amount\\r                }\\r            }\\r        }\\r    }\\r}\"\n}";
        
        private readonly string _login;
        private readonly string _password;
        private readonly IHttpClient _client;
        private readonly Endpoints _endpoints;
        private string _authToken;
                
        public Nubank(string login, string password)
            : this(new HttpClient(), login, password)
        { }

        public Nubank(IHttpClient httpClient, string login, string password)
        {
            _login = login;
            _password = password;
            _client = httpClient;
            _endpoints = new Endpoints(_client);
        }

        public async Task<LoginResponse> LoginAsync()
        {
            await GetTokenAsync();

            if (_endpoints.Events != null)
            {
                return new LoginResponse();
            }

            return new LoginResponse(Guid.NewGuid().ToString());
        }

        private async Task GetTokenAsync()
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

        public async Task AutenticateWithQrCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await GetTokenAsync();
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
            _authToken = response["access_token"].ToString();
        }

        private void FillAutenticatedUrls(Dictionary<string, object> response)
        {
            var listLinks = (JObject)response["_links"];
            var properties = listLinks.Properties();
            var values = listLinks.Values();
            _endpoints.AutenticatedUrls = listLinks
                .Properties()
                .Select(x => new KeyValuePair<string, string>(x.Name, (string)listLinks[x.Name]["href"]))
                .ToDictionary(key => key.Key, key => key.Value);
        }

        public async Task<IEnumerable<Saving>> GetSavingsAsync()
        {
            EnsureAuthenticated();

            var response = await _client.PostAsync<GetSavingsResponse>(_endpoints.GraphQl, JObject.Parse(StatementGraphQl), GetHeaders());

            return response.Savings;
        }       
       
        public async Task<IEnumerable<Event>> GetEventsAsync()
        {
            EnsureAuthenticated();

            var response = await _client.GetAsync<GetEventsResponse>(_endpoints.Events, GetHeaders());
            return response.Events;
        }

        private void EnsureAuthenticated()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                throw new InvalidOperationException("This operation requires the user to be logged in. Make sure that the Login method has been called.");
            }
        }

        private Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string> {
                { "Authorization", $"Bearer {_authToken}" }
            };
        }
    }
}