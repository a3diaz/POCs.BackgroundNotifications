using POCs.BackGroundNotifications.Windows10.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<PipeClient>();
    })
    .Build();

await host.RunAsync();
