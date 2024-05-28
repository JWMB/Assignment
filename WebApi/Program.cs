var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/electricityprice_zone", (
    DateTime date, string zone) =>
{
    return new Response(1, 1, 1, "SEK");
})
.WithOpenApi();

app.Run();

public partial class Program { } // For exposing to tests

public readonly record struct Response(decimal Min, decimal Max, decimal Avg, string Unit);