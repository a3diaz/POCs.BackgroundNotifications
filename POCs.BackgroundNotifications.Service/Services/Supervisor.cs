using System.ServiceProcess;

namespace POCs.BackgroundNotifications.WindowsService.Services;
public class Supervisor : BackgroundService
{
    private readonly Messenger _messenger;
    private readonly string[] supervisedServices = new[] { "MSSQLSERVER" };
    private readonly TimeSpan timeBetweenReviews = TimeSpan.FromMinutes(1);

    private readonly ILogger<Supervisor> _logger;

    public Supervisor(ILogger<Supervisor> logger, Messenger messenger)
    {
        _logger = logger;
        _messenger = messenger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting supervisor...");
        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Checking for running services.");
                var services = ServiceController.GetServices()
                    .Where(s => supervisedServices.Contains(s.ServiceName))
                    .Where(s => s.Status != (ServiceControllerStatus.Running | ServiceControllerStatus.StartPending));

                foreach (var service in services)
                {
                    var message = $"The service \"{service.ServiceName}\" is not running";

                    _logger.LogDebug(message);
                    _messenger.EnqueueMessage(message);
                }

                await Task.Delay(timeBetweenReviews, stoppingToken);
            }
        }, stoppingToken);
    }
}