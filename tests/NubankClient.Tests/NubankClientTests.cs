using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Moq;
using RestSharp;
using Xunit;

namespace NubankClient.Tests
{
    public class NubankClientTests
    {
        private readonly Mock<IRestClient> _mockRestClient;

        [Fact]
        public async Task ShouldLogin()
        {
            var discoveryData = new Dictionary<string, object>() {
                { "login", "teste2" }
            };

            var discoveryResponse =  new Mock<IRestResponse<Dictionary<string, object>>>();
            discoveryResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);
            discoveryResponse.Setup(_ => _.Data).Returns(discoveryData);

            _mockRestClient
                .Setup(x => x.Get<Dictionary<string, object>>(It.IsAny<IRestRequest>()))
                .Returns(discoveryResponse.Object);

            var eventsLink = new Dictionary<string, object>() { {"href", "teste"} };
            var links = new Dictionary<string, object>() {
                { "events", eventsLink }
            };

            var responseLogin = new Dictionary<string, object>() {
                { "access_token", "eyJhbGciOiJSUzI1Ni" },
                { "token_type", "bearer" },
                { "_links", links },
            };

            _mockRestClient
                .Setup(x => x.PostAsync<Dictionary<string, object>>(It.IsAny<IRestRequest>()))
                .ReturnsAsync(responseLogin);

            var nubankClient = new Nubank(_mockRestClient.Object, "login", "password");
            await nubankClient.Login();
        }

        [Fact]
        public void ShouldGetEvents()
        {
            Assert.True(false);
        }
    }
}