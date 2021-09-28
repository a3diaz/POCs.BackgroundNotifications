using POCs.BackgroundNotifications.WindowsService.Services;

IHost host = Host
    .CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "POC - Background Notifications Service";
    })
    .ConfigureLogging(logging =>
    {
        logging.AddEventLog(eventLogSettings =>
        {
            eventLogSettings.SourceName = "Background Notifications Service";
        });
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<Messenger>()
            .AddHostedService<PipeServer>()
            .AddHostedService<Supervisor>();
    })
    .Build();

await host.RunAsync();
