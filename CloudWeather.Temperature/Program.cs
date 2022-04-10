using CloudWeather.Temperature.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TemperatureDbContext>(
    options =>
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.UseNpgsql(builder.Configuration.GetConnectionString("TemperatureDb"));
    }, ServiceLifetime.Transient
);


var app = builder.Build();

app.MapGet("/observation/{zip}", async (string zip, [FromQuery] int? days, TemperatureDbContext db) => {
    if (days == null || days < 0 || days > 30)
    {
        return Results.BadRequest("Please enter a 'days' query paramter or days must be greater than zero and less than 30");
    }
    var startdate = DateTime.UtcNow - TimeSpan.FromDays(days.Value);
    var results = await db.TemperatureSet
        .Where<Temperature>(prep => prep.ZipCode == zip && prep.CreatedOn > startdate)
        .ToListAsync<Temperature>();
    return Results.Ok(results);
});


app.MapPost("/observation", async (Temperature temperature, TemperatureDbContext dbContext) =>
{
    temperature.CreatedOn = temperature.CreatedOn.ToUniversalTime();
    await dbContext.AddAsync<Temperature>(temperature);
    await dbContext.SaveChangesAsync();
});

app.Run();
