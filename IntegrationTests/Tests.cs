using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using ReceivingApi;
using Xunit;

namespace IntegrationTests
{
    public class Tests :
        IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public Tests(
            WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        
        [Fact]
        public async Task Client_posting_to_Api_returns_Ok()
        {
            /* Arrange */
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteLineAsync("FILE CONTENTS");
            await writer.FlushAsync();
            stream.Position = 0;

            using var client = _factory.CreateDefaultClient();

            /* Act */
            using var response =
                await client.PostAsync(
                    "Receive",
                    new MultipartFormDataContent
                    {
                        {
                            new StreamContent(stream),
                            "file",
                            "fileName"
                        }
                    });
            
            /* Assert */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}