using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var databaseUrl = ResolveDatabaseConnectionString(builder.Configuration);
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(databaseUrl).Build());
}

var app = builder.Build();

var dataSource = app.Services.GetService<NpgsqlDataSource>();
if (dataSource is not null)
{
    await InitializeDatabaseAsync(dataSource);
}

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
    if (dataSource is null)
    {
        return Results.Ok(new { status = "ok", database = "not_configured" });
    }

    await using var command = dataSource.CreateCommand("select 1");
    await command.ExecuteScalarAsync();

    return Results.Ok(new { status = "ok", database = "ok" });
});
app.MapGet("/db/notes", async () =>
{
    if (dataSource is null)
    {
        return Results.Problem("Database is not configured.");
    }

    await using var command = dataSource.CreateCommand("""
        select id, message, created_at
        from app_notes
        order by id desc
        limit 20
        """);

    await using var reader = await command.ExecuteReaderAsync();
    var notes = new List<NoteResponse>();

    while (await reader.ReadAsync())
    {
        notes.Add(new NoteResponse(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetDateTime(2)));
    }

    return Results.Ok(notes);
});
app.MapPost("/db/notes", async (CreateNoteRequest request) =>
{
    if (dataSource is null)
    {
        return Results.Problem("Database is not configured.");
    }

    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    await using var command = dataSource.CreateCommand("""
        insert into app_notes (message)
        values (@message)
        returning id, message, created_at
        """);
    command.Parameters.AddWithValue("message", request.Message.Trim());

    await using var reader = await command.ExecuteReaderAsync();
    await reader.ReadAsync();

    return Results.Created($"/db/notes/{reader.GetInt32(0)}", new NoteResponse(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetDateTime(2)));
});
app.MapGet("/version", () => "v1");
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

static async Task InitializeDatabaseAsync(NpgsqlDataSource db)
{
    await using var command = db.CreateCommand("""
        create table if not exists app_notes (
            id integer generated always as identity primary key,
            message text not null,
            created_at timestamptz not null default now()
        );

        insert into app_notes (message)
        select 'PostgreSQL is connected'
        where not exists (select 1 from app_notes);
        """);

    await command.ExecuteNonQueryAsync();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record CreateNoteRequest(string Message);

record NoteResponse(int Id, string Message, DateTime CreatedAt);
