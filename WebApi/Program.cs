using Microsoft.AspNetCore.Mvc;
using System;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IElectricityPriceService, ElectricityPriceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/electricityprice_zone", async ([FromServices] IElectricityPriceService priceService,
    DateTime date, string zone, Response.Currency currency = Response.Currency.EUR) =>
{
    var items = await priceService.Get(date, zone);
    return Response.Create(items, currency);
})
.WithOpenApi();

app.Run();

public partial class Program { } // For exposing to tests

public readonly record struct Response(decimal Min, decimal Max, decimal Avg, Dictionary<int, decimal> Hourly)
{
    public enum Currency { EUR, SEK };

    public static Response Create(IEnumerable<ElectricyPriceRecord> items, Currency currency)
    {
        var getVal = (ElectricyPriceRecord item) => currency == Currency.SEK ? item.SEK_per_kWh : item.EUR_per_kWh;
        return new Response(
            (decimal)items.Min(getVal),
            (decimal)items.Max(getVal),
            (decimal)items.Average(getVal),
            items
                .GroupBy(o => o.time_start.Hour)
                .ToDictionary(o => o.Key, o => (decimal)o.Select(getVal).Average())
                );
    }
}