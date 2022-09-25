using Microsoft.AspNetCore.Mvc;
using SchemaApi.App;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<EventRepository>();
builder.Services.AddHostedService<EventDownloader>();
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "SchemaApi")
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.Extensions.Diagnostics.HealthChecks", LogEventLevel.Warning)
        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
        .WriteTo.Console(theme: AnsiConsoleTheme.Code);
});

var app = builder.Build();
app.UseSerilogRequestLogging();

app.MapGet("api/events/{person}", (
    [FromRoute] string person, 
    [FromQuery] string? date, 
    [FromServices] EventRepository repository) =>
{
    DateTime? dateFilter = null;
    if (DateTime.TryParse(date, out var validDate))
    {
        dateFilter = validDate;
    }

    var events = repository.FindBy(person, dateFilter).ToArray();
 
    return Results.Json(new
    {
        Items = events
            .OrderBy(x => x.Begin)
            .Select(x => new
            {
                Begin = x.Begin,
                End = x.End,
                Location = x.Location,
                Description = x.Description,
                Summary = x.Summary
            })
            .ToArray()
    });
});

app.Run();