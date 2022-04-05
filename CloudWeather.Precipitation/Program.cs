using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/precipitation/{zip}", (string zip, [FromQuery] int? days)=>{
    return Results.Ok(days);
});

app.Run();
