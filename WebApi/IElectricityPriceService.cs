namespace WebApi
{
    public interface IElectricityPriceService
    {
        Task<List<ElectricyPriceRecord>> Get(DateTime date, string zone);
    }

    // generated using https://json2csharp.com/
    public readonly record struct ElectricyPriceRecord(
        double SEK_per_kWh,
        double EUR_per_kWh,
        double EXR,
        DateTime time_start,
        DateTime time_end
    );

    public class ElectricityPriceService : IElectricityPriceService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ElectricityPriceService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<List<ElectricyPriceRecord>> Get(DateTime date, string zone)
        {
            using var client = httpClientFactory.CreateClient();
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"https://www.elprisetjustnu.se/api/v1/prices/{date.Year}/{date:MM-dd}_{zone}.json "));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<List<ElectricyPriceRecord>>();
            if (content == null)
                throw new Exception("No data");

            return content;
        }
    }
}
