using Microsoft.Toolkit.Uwp.Notifications;
using System.IO.Pipes;
using System.Security.Principal;

namespace POCs.BackGroundNotifications.Windows10.Services
{
    public class PipeClient : BackgroundService
    {
        private const string PipeName = "OXG_BGNSS_PIPE_NAME";
        private readonly TimeSpan timeBetweenReadAttemps = TimeSpan.FromSeconds(5);
        private readonly ILogger<PipeClient> _logger;
        private NamedPipeClientStream? _pipe;

        public PipeClient(ILogger<PipeClient> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Connecting to server...");
                        _pipe = new NamedPipeClientStream(".", PipeName,
                                PipeDirection.InOut, PipeOptions.None,
                                TokenImpersonationLevel.Anonymous);

                        _pipe.Connect();
                        using var _br = new BinaryReader(_pipe);

                        while (!stoppingToken.IsCancellationRequested)
                        {
                            var messageLength = _br.ReadUInt32();
                            var message = new string(_br.ReadChars((int)messageLength));

                            new ToastContentBuilder()
                                .AddText("Service stopped")
                                .AddText(message)
                                .SetToastScenario(ToastScenario.Alarm)
                                .SetToastDuration(ToastDuration.Long)
                                .Show(toast =>
                                {
                                    toast.Tag = "1";
                                    toast.Group = "serviceStopped";
                                });

                            await Task.Delay(timeBetweenReadAttemps);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        await Task.Delay(timeBetweenReadAttemps);
                    }
                }
            }, stoppingToken);
        }
    }
}