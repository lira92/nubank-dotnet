using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NubankClient.Http;
using NubankClient.Model;
using RestSharp;
using Xunit;

namespace NubankClient.Tests
{
    public class NubankClientTests
    {
        private readonly Mock<IHttpClient> _mockRestClient = new Mock<IHttpClient>();
        private readonly List<Event> _events;

        public NubankClientTests()
        {
            _events = new List<Event>
            {
                new Event
                {
                    Title = "Test transaction",
                    Amount = 500,
                    Category = "transaction",
                    Description = "Test transaction description",
                    Time = DateTime.Now.Subtract(TimeSpan.FromDays(1))
                },
                new Event
                {
                    Title = "Test transaction 2",
                    Amount = 50,
                    Category = "transaction",
                    Description = "Test transaction 2 description",
                    Time = DateTime.Now.Subtract(TimeSpan.FromDays(2))
                }
            };
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
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => nubankClient.GetEvents());
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
            await nubankClient.Login();
            var actualEvents = await nubankClient.GetEvents();

            Assert.Equal(_events, actualEvents);
            _mockRestClient.Verify(x => x.GetAsync<GetEventsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()
                ), Times.Once());
        }

        private void MockDiscoveryRequest()
        {
            var discoveryData = new Dictionary<string, string>() {
                { "login", "login_url" }
            };

            _mockRestClient
                .Setup(x => x.GetAsync<Dictionary<string, string>>(It.IsAny<string>()))
                .ReturnsAsync(discoveryData);
        }

        private void MockLoginRequest()
        {
            var eventsLink = new Dictionary<string, object>() { { "href", "events_url" } };
            var resetPasswordUrl = new Dictionary<string, object>() { { "href", "reset_password_url" } };
            var links = new Dictionary<string, object>() {
                { "events", eventsLink },
                { "reset_password", resetPasswordUrl }
            };

            var responseLogin = new Dictionary<string, object>() {
                { "access_token", "eyJhbGciOiJSUzI1Ni" },
                { "token_type", "bearer" },
                { "_links", links },
            };

            _mockRestClient
                .Setup(x => x.PostAsync<Dictionary<string, object>>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(responseLogin);
        }
    }
}