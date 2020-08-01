using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using WebsocketGateway.Config;
using WebsocketGateway.Hubs;
using WebsocketGateway.Services.MessageProcessing;

namespace WebsocketGateway.Services.Publishing
{
    /// <summary>
    /// Service that takes the flight-data of the <see cref="IMessageProcessService"/> and publishes it to the
    /// SignalR-clients.
    /// </summary>
    public class SignalRPublishService : IHostedService
    {
        /// <summary>
        /// HubContext that allows us to broadcast our messages to the SignalR-clients
        /// </summary>
        private readonly IHubContext<DefaultHub> _hubContext;

        /// <summary>
        /// IMessageProcessService from which we'll obtain our data to publish
        /// </summary>
        private readonly IMessageProcessService _messageProcessService;

        /// <summary>
        /// Intermediate provider that we use to inform new SignalR-clients about our currently active flight-data
        /// </summary>
        private readonly SignalRInitialDataProvider _signalRInitialDataProvider;

        /// <summary>
        /// Disposable representation of the observable that is needed when stopping the Service.
        /// </summary>
        private IDisposable? _stream;

        public SignalRPublishService(
            IHubContext<DefaultHub> hubContext,
            IMessageProcessService messageProcessService,
            SignalRInitialDataProvider signalRInitialDataProvider
        )
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _messageProcessService = messageProcessService
                                     ?? throw new ArgumentNullException(nameof(messageProcessService));
            _signalRInitialDataProvider = signalRInitialDataProvider
                                          ?? throw new ArgumentNullException(nameof(signalRInitialDataProvider));
        }

        /// <summary>
        /// Magic method that is called by dependency injection on startup
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Save stream to cancel it properly on StopAsync.
            _stream = _messageProcessService.Stream.Subscribe(async entry =>
            {
                _signalRInitialDataProvider.Push(entry);
                await _hubContext.Clients.All.SendAsync(IConstants.NewDataMethod, entry, cancellationToken);
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Magic method that is called by dependency injection on shutdown
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // If stream exists (should?!), dispose its subscription.
            _stream?.Dispose();

            return Task.CompletedTask;
        }
    }
}