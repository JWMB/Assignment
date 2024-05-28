namespace WebApi
{
    public interface IElectricyPriceService
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

    public class ElectricyPriceService : IElectricyPriceService
    {
        public Task<List<ElectricyPriceRecord>> Get(DateTime date, string zone)
        {
            return Task.FromResult(new[] { new ElectricyPriceRecord(1, 1, 1, DateTime.Today, DateTime.Today.AddHours(1)) }.ToList());
        }
    }
}
