using Moq;
using Newtonsoft.Json.Linq;
using NubankClient.Http;
using NubankClient.Model.Events;
using NubankClient.Model.Savings;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Xunit;

namespace NubankClient.Tests
{
    public class NubankClientTests
    {
        private readonly Mock<IHttpClient> _mockRestClient = new Mock<IHttpClient>();
        private readonly List<Event> _events;
        private readonly List<Saving> _savings;

        public NubankClientTests()
        {
            _events = new List<Event>
            {
                new Event
                {
                    Title = "Test transaction",
                    Amount = 500,
                    Category = EventCategory.Transaction,
                    Description = "Test transaction description",
                    Time = DateTime.Now.Subtract(TimeSpan.FromDays(1))
                },
                new Event
                {
                    Title = "Test transaction 2",
                    Amount = 50,
                    Category = EventCategory.Transaction,
                    Description = "Test transaction 2 description",
                    Time = DateTime.Now.Subtract(TimeSpan.FromDays(2))
                }
            };

            _savings = new List<Saving>()
            {
                new Saving
                {
                    Title = "Transferência enviada",
                    Amount = 500,
                    TypeName = SavingType.TransferOutEvent,
                    Detail = "Renato - R$ 500,00",
                    PostDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                    DestinationAccount = new Account() {Name = "Renato"}
                },
                new Saving
                {
                    Title = "Transferência recebida",
                    Amount = 50,
                    TypeName = SavingType.TransferInEvent,
                    Detail = "R$ 50,00",
                    PostDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                    OriginAccount = new Account() {Name = "Renato"}
                }
            };
        }

        [Fact]
        public async Task ShouldThrowExceptionWithErrorProvidedWhenLoginFailed()
        {
            MockDiscoveryRequest();

            const string errorMessage = "error message";

            var loginData = new Dictionary<string, object> {
                { "error", errorMessage }
            };

            _mockRestClient
                .Setup(x => x.PostAsync<Dictionary<string, object>>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(loginData);

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            var exception = await Assert.ThrowsAsync<AuthenticationException>(
                async () => await nubankClient.LoginAsync()
            );

            Assert.Equal(errorMessage, exception.Message);
        }

        [Fact]
        public async Task ShouldThrowExceptionWithGenericMessageWhenLoginFailedAndErrorIsNotPresent()
        {
            MockDiscoveryRequest();

            const string errorMessage = "Unknow error occurred on trying to do login on Nubank using the entered credentials";

            var loginData = new Dictionary<string, object>();

            _mockRestClient
                .Setup(x => x.PostAsync<Dictionary<string, object>>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(loginData);

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            var exception = await Assert.ThrowsAsync<AuthenticationException>(
                async () => await nubankClient.LoginAsync()
            );

            Assert.Equal(errorMessage, exception.Message);
        }

        [Fact]
        public async Task ShouldReturnLoginResponseWithNeedsDeviceAuthorizationWhenEventsNotPresentInLoginResponse()
        {
            MockDiscoveryRequest();

            MockLoginRequestWithouEventsUrl();

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            var loginResponse = await nubankClient.LoginAsync();

            Assert.True(loginResponse.NeedsDeviceAuthorization);
            Assert.NotNull(loginResponse.Code);
        }

        [Fact]
        public async Task ShouldReturnLoginResponseWithNotNeedsDeviceAuthorizationWhenEventsPresentInLoginResponse()
        {
            MockDiscoveryRequest();

            MockLoginRequest();

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            var loginResponse = await nubankClient.LoginAsync();

            Assert.False(loginResponse.NeedsDeviceAuthorization);
            Assert.Null(loginResponse.Code);
        }

        [Fact]
        public async Task ShouldAutenticateWithQrCode()
        {
            MockDiscoveryRequest();
            MockDiscoveryAppRequest();

            MockLoginRequestWithouEventsUrl();
            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            var loginResponse = await nubankClient.LoginAsync();

            MockLiftRequest(loginResponse.Code);

            await nubankClient.AutenticateWithQrCodeAsync(loginResponse.Code);

            var expectedPayload = new
            {
                qr_code_id = loginResponse.Code,
                type = "login-webapp"
            };

            _mockRestClient.Verify(x => x.PostAsync<Dictionary<string, object>>(
                "lift_url",
                It.Is<object>(o => o.GetHashCode() == expectedPayload.GetHashCode()),
                It.Is<Dictionary<string, string>>(dictionary => IsValidAuthorizationHeader(dictionary))
                ), Times.Once());
        }

        private static bool IsValidAuthorizationHeader(Dictionary<string, string> dictionary)
        {
            return dictionary.ContainsKey("Authorization")
                && !string.IsNullOrEmpty(dictionary["Authorization"])
                && !string.IsNullOrEmpty(dictionary["Authorization"].Replace("Bearer ", ""));
        }

        [Fact]
        public async Task ShouldGetTokenIfAutenticateWithQrCodeIsCalledWithoutLoginMade()
        {
            MockDiscoveryRequest();
            MockDiscoveryAppRequest();

            MockLoginRequestWithouEventsUrl();
            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");

            var code = Guid.NewGuid().ToString();

            MockLiftRequest(code);

            await nubankClient.AutenticateWithQrCodeAsync(code);

            var expectedPayload = new
            {
                qr_code_id = code,
                type = "login-webapp"
            };

            _mockRestClient.Verify(x => x.PostAsync<Dictionary<string, object>>(
                    "lift_url",
                    It.Is<object>(o => o.GetHashCode() == expectedPayload.GetHashCode()),
                    It.Is<Dictionary<string, string>>(dictionary => IsValidAuthorizationHeader(dictionary))
                ), Times.Once());
        }

        [Fact]
        public async Task ShouldThrowExceptionWhenLoginWasNotCalled()
        {
            MockDiscoveryRequest();

            MockLoginRequest();

            var getEventsResponse = new GetEventsResponse
            {
                Events = _events
            };

            _mockRestClient
                .Setup(x => x.GetAsync<GetEventsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()
                ))
                .ReturnsAsync(getEventsResponse);

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => nubankClient.GetEventsAsync());
        }

        [Fact]
        public async Task ShouldGetEvents()
        {
            MockDiscoveryRequest();

            MockLoginRequest();

            var getEventsResponse = new GetEventsResponse
            {
                Events = _events
            };

            _mockRestClient
                .Setup(x => x.GetAsync<GetEventsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()
                ))
                .ReturnsAsync(getEventsResponse);

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            await nubankClient.LoginAsync();
            var actualEvents = await nubankClient.GetEventsAsync();

            Assert.Equal(_events, actualEvents);
            _mockRestClient.Verify(x => x.GetAsync<GetEventsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()
                ), Times.Once());
        }

        [Fact]
        public async Task ShouldGetSavings()
        {
            MockDiscoveryRequest();

            MockLoginRequest();

            var getSavingsResponse = new GetSavingsResponse()
            {
                Data = new DataResponse()
                {
                    Viewer = new ViewerResponse() { SavingsAccount = new SavingsAccount() { Feed = _savings } }
                }
            };

            _mockRestClient
                .Setup(x => x.PostAsync<GetSavingsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<Dictionary<string, string>>()
                ))
                .ReturnsAsync(getSavingsResponse);

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            await nubankClient.LoginAsync();
            var actualSavings = await nubankClient.GetSavingsAsync();

            Assert.Equal(_savings, actualSavings);

            _mockRestClient.Verify(x => x.PostAsync<GetSavingsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<Dictionary<string, string>>()
                ), Times.Once());
        }

        private void MockDiscoveryRequest()
        {
            var discoveryData = new Dictionary<string, string> {
                { "login", "login_url" }
            };

            _mockRestClient
                .Setup(x => x.GetAsync<Dictionary<string, string>>("https://prod-s0-webapp-proxy.nubank.com.br/api/discovery"))
                .ReturnsAsync(discoveryData);
        }

        private void MockLoginRequest()
        {
            var eventsLink = new Dictionary<string, object> { { "href", "events_url" } };
            var resetPasswordUrl = new Dictionary<string, object> { { "href", "reset_password_url" } };

            var links = JObject.Parse("{\"events\": { \"href\": \"events_url\"}, \"reset_password\": { \"href\": \"reset_password_url\"}}");

            var responseLogin = new Dictionary<string, object> {
                { "access_token", "eyJhbGciOiJSUzI1Ni" },
                { "token_type", "bearer" },
                { "_links", links },
            };

            _mockRestClient
                .Setup(x => x.PostAsync<Dictionary<string, object>>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(responseLogin);
        }

        private void MockLoginRequestWithouEventsUrl()
        {
            var links = JObject.Parse("{\"account_emergency\": { \"href\": \"account_emergency_url\"}}");

            var responseLogin = new Dictionary<string, object> {
                { "access_token", "eyJhbGciOiJSUzI1Ni" },
                { "token_type", "bearer" },
                { "_links", links },
            };

            _mockRestClient
                .Setup(x => x.PostAsync<Dictionary<string, object>>("login_url", It.IsAny<object>()))
                .ReturnsAsync(responseLogin);
        }

        private void MockLiftRequest(string code)
        {
            var links = JObject.Parse("{\"events\": { \"href\": \"events_url\"}, \"reset_password\": { \"href\": \"reset_password_url\"}}");

            var responseLift = new Dictionary<string, object> {
                { "access_token", "eyJhbGciOiJSUzI1Ni" },
                { "token_type", "bearer" },
                { "_links", links },
            };

            _mockRestClient
                .Setup(x => x.PostAsync<Dictionary<string, object>>(
                    "lift_url",
                    It.IsAny<object>(),
                    It.IsAny<Dictionary<string, string>>())
                 )
                .ReturnsAsync(responseLift);
        }

        private void MockDiscoveryAppRequest()
        {
            var discoveryData = new Dictionary<string, object> {
                { "lift", "lift_url" },
                { "faq", JObject.Parse("{ \"android\": \"faq_android_url\"}") }
            };

            _mockRestClient
                .Setup(x => x.GetAsync<Dictionary<string, object>>("https://prod-s0-webapp-proxy.nubank.com.br/api/app/discovery"))
                .ReturnsAsync(discoveryData);
        }
    }
}