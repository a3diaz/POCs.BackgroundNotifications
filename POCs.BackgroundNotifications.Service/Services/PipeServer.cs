using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace POCs.BackgroundNotifications.WindowsService.Services
{
    public class PipeServer : BackgroundService
    {
        private const string PipeName = "OXG_BGNSS_PIPE_NAME";
        private readonly ILogger<PipeServer> _logger;
        private readonly Messenger _messenger;
        private NamedPipeServerStream? _pipe;

        public PipeServer(ILogger<PipeServer> logger, Messenger messenger)
        {
            _logger = logger;
            _messenger = messenger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));

            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _pipe?.Dispose();
                    _pipe = NamedPipeServerStreamAcl.Create(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message,
                        PipeOptions.WriteThrough, 1024, 1024, pipeSecurity);

                    _logger.LogInformation("NamedPipeServerStream object created.");

                    _logger.LogInformation("Waiting for client connection...");
                    _pipe.WaitForConnection();

                    _logger.LogInformation("Client connected.");

                    try
                    {
                        // Read user input and send that to the client process.
                        using var _bw = new BinaryWriter(_pipe);
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            if (_messenger.HasMessages)
                            {
                                var message = _messenger.DequeueMessage();
                                var buf = Encoding.UTF8.GetBytes(message);
                                _bw.Write((uint)buf.Length);
                                _bw.Write(buf);
                                _logger.LogInformation("Wrote: \"{message}\"", message);
                                _bw.Flush();
                            }
                            else
                            {
                                await Task.Delay(500, stoppingToken);
                            }
                        }
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException e)
                    {
                        _logger.LogError(e, e.Message);
                    }
                }
            }, stoppingToken);

        }
    }
}