using Microsoft.AspNetCore.Mvc;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IElectricyPriceService, ElectricyPriceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/electricityprice_zone", async ([FromServices] IElectricyPriceService priceService,
    DateTime date, string zone) =>
{
    var items = await priceService.Get(date, zone);
    return new Response(
        (decimal)items.Min(o => o.EUR_per_kWh),
        (decimal)items.Max(o => o.EUR_per_kWh),
        (decimal)items.Average(o => o.EUR_per_kWh),
        "EUR");
})
.WithOpenApi();

app.Run();

public partial class Program { } // For exposing to tests

public readonly record struct Response(decimal Min, decimal Max, decimal Avg, string Unit);