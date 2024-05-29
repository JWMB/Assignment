using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IElectricityPriceService, ElectricityPriceService>();
builder.Services.AddSingleton<ICoordinateToZoneService, CoordinateToZoneService>();
builder.Services.AddSingleton<IZoneDefinitionProvider, GeoJsonZoneDefinitionProvider>();

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
    return Response.Create(await priceService.Get(date, zone), currency);
})
.WithOpenApi();

app.MapGet("/electricityprice_coords", async ([FromServices] IElectricityPriceService priceService, [FromServices] ICoordinateToZoneService coordinateService,
    DateTime date, decimal longitude, decimal latitude, Response.Currency currency = Response.Currency.EUR) =>
{
    var zone = await coordinateService.Get(new(longitude, latitude));
    if (zone == null)
        throw new Exception(""); // System.Net.HttpStatusCode.BadRequest);
    return Response.Create(await priceService.Get(date, zone), currency);
})
.WithOpenApi();

app.MapGet("/zones_image", async ([FromServices] IZoneDefinitionProvider zoneDefinitionProvider,
    [Range(0.1, 100)]
    float sizeXFactor = 1,
    [Range(0.1, 100)]
    float sizeYFactor = 1) =>
{
    var zones = await zoneDefinitionProvider.Get();
    if (zones == null)
        throw new Exception("");
    var allPolygons = zones.SelectMany(o => o.Polygons);
    // TODO: check final size before rendering (attack vector / memory use)
    using var img = PolygonRenderer.Render(allPolygons.Select(o => o.Points), (sizeXFactor, sizeYFactor));
    using var stream = new MemoryStream();
    await img.SaveAsWebpAsync(stream);
    return Results.File(stream.ToArray(), "image/webp");
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
                .GroupBy(o => o.time_start.Hour) // this method accepts any date range, could be multiple dates - needs grouping
                .ToDictionary(o => o.Key, o => (decimal)o.Select(getVal).Average())
                );
    }
}