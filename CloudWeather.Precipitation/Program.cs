using CloudWeather.Precipitation.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PrecipitationDbContext>(
    options =>
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.UseNpgsql(builder.Configuration.GetConnectionString("PrecipitationDb"));
    }, ServiceLifetime.Transient
);

var app = builder.Build();

app.MapGet("/precipitation/{zip}",async (string zip, [FromQuery] int? days, PrecipitationDbContext db)=>{
    if (days == null || days < 0 || days > 30) {
        return Results.BadRequest("Please enter a 'days' query paramter or days must be greater than zero and less than 30");
    }
    var startdate = DateTime.UtcNow - TimeSpan.FromDays(days.Value);
    var results = await db.PrecipitationSet
        .Where<Precipitation>(prep => prep.ZipCode == zip && prep.CreateOn > startdate)
        .ToListAsync<Precipitation>();
    return Results.Ok(results);
});

app.Run();
