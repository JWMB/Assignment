using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests
{
    public class Endpoints_Tests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public Endpoints_Tests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Zone_Get_Integration()
        {
            // Arrange
            var client = factory.CreateClient();

            var date = DateTime.Today;
            var zone = "SE1";

            // Act
            var response = await client.GetAsync($"/electricityprice_zone?date={date:yyyy-MM-dd}&zone={zone}");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}