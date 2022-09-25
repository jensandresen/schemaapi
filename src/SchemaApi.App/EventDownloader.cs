namespace SchemaApi.App;

public class EventDownloader : BackgroundService
{
    private readonly ILogger<EventDownloader> _logger;
    private readonly IServiceProvider _serviceProvider;

    public EventDownloader(ILogger<EventDownloader> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<HttpClient>();
            var repository = scope.ServiceProvider.GetRequiredService<EventRepository>();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var entries = new[]
            {
                new
                {
                    Person = "charlie",
                    Url = configuration["SCHEMA_URL_CHARLIE"]
                },
                new
                {
                    Person = "clara",
                    Url = configuration["SCHEMA_URL_CLARA"]
                },
            };
            
            foreach (var entry in entries)
            {
                try
                {
                    await DownloadEvents(
                        client: client,
                        repository: repository,
                        entry.Person, entry.Url,
                        stoppingToken);
                }
                catch (Exception err)
                {
                    _logger.LogError(err, "Fatal error during download of events for {Person} from {Url}", entry.Person, entry.Url);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }, stoppingToken);
    }

    private async Task DownloadEvents(HttpClient client, EventRepository repository, string person, string url, CancellationToken stoppingToken)
    {
        _logger.LogDebug("Downloading events for {Person} from {Url} ...", person, url);
        
        var content = await client.GetStringAsync(url, stoppingToken);

        var lines = content
            .Replace("\r", "")
            .Split("\n");
        
        var events = EventHelper
            .Parse(lines)
            .OrderBy(x => x.Begin)
            .ToArray();

        repository.Upsert(person, events);
        
        _logger.LogDebug("Downloaded {EventCount} for {Person}", events.Length, person);
    }
}