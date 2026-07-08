using DemoDockerAPI2.Services;
using DemoDockerAPI2.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDockerOptions(builder.Configuration);

var databaseUrl = ResolveDatabaseConnectionString(builder.Configuration);
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(databaseUrl).Build());
}

builder.Services.AddScoped<IProductService, ProductService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
app.MapGet("/hello", () =>
{
    return "Hello Docker";
});
app.MapGet("/health", async () =>
{
    return Results.Ok(new { status = "ok", database = "not_configured" });  
}); 
app.MapGet("/version", () => "v1.02");
app.MapGet("/debug", (IConfiguration config) =>
{
    return new
    {
        Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        Connection =
            config.GetConnectionString("DefaultConnection") != null
    };
});

app.MapControllers();
app.Run();

static string? ResolveDatabaseConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? configuration["DATABASE_URL"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return null;
    }

    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
        || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        return connectionString;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty),
        Password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty),
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record CreateNoteRequest(string Message);

record NoteResponse(int Id, string Message, DateTime CreatedAt);
