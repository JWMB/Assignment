using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net.Http.Json;
using WebApi;
using static Response;

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

            var date = new DateTime(2024, 1, 1);
            var zone = "SE1";

            // Act
            var response = await client.GetAsync($"/electricityprice_zone?date={date:yyyy-MM-dd}&zone={zone}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<Response>();
            result.Min.ShouldBe(0.02123M);
            result.Hourly.Count.ShouldBe(24);
        }

        [Theory]
        [InlineData(Response.Currency.SEK)]
        [InlineData(Response.Currency.EUR)]
        public async Task Zone_Get_Mocked(Response.Currency currency)
        {
            // Arrange
            var client = Utils.CreateClient(factory, services => {
                var service = A.Fake<IElectricityPriceService>();
                A.CallTo(() => service.Get(A<DateTime>._, A<string>._))
                    .Returns(Task.FromResult(new List<ElectricyPriceRecord> { new ElectricyPriceRecord(55, 99, 1, DateTime.Today.Date, DateTime.Today.Date.AddHours(1)) }));
                services.AddScoped(sp => service);
            });

            var date = DateTime.Today;
            var zone = "SE1";

            // Act
            var response = await client.GetAsync($"/electricityprice_zone?date={date:yyyy-MM-dd}&zone={zone}&currency={currency}");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Response>();
            content.Max.ShouldBe(currency == Currency.EUR ? 99 : 55);
        }
    }
}