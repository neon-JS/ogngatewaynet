using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using WebsocketGateway.Hubs;

namespace WebsocketGateway.Services
{
    /// <summary>
    /// Background service that converts and publishes data from OGN listener to SignalR clients
    /// </summary>
    public class PublishService : IHostedService
    {
        /// <summary>
        /// This is the default method for receiving data
        /// </summary>
        private const string DataMethod = "Data";

        /// <summary>
        /// HubContext that provides us the data needed for publishing messages
        /// </summary>
        private readonly IHubContext<DefaultWebsocketHub> _hubContext;

        private readonly DataService _dataService;

        /// <summary>
        /// Disposable representation of the observable that is needed when stopping the Service.
        /// </summary>
        private IDisposable? _stream;

        public PublishService(
            IHubContext<DefaultWebsocketHub> hubContext, 
            DataService dataService
            )
        {
            _hubContext = hubContext;
            _dataService = dataService;
        }
        
        /// <summary>
        /// Magic method that is called by dependency injection on startup
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Save stream to cancel it properly on StopAsync.
            _stream = _dataService.Stream
                .Subscribe(async entry => await _hubContext.Clients.All.SendAsync(DataMethod, entry, cancellationToken));
            
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