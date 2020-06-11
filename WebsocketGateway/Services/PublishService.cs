using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using OgnGateway.ogn.aircraft;
using OgnGateway.ogn.models;
using OgnGateway.ogn.stream;
using WebsocketGateway.Hubs;
using WebsocketGateway.Models;

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
        
        /// <summary>
        /// Listener that returns raw messages to convert
        /// </summary>
        private readonly StreamListener _ognStreamListener;
        
        /// <summary>
        /// Aircraft provider that we can use to fetch data from.
        /// Note that this provider has been initialized by the dependency injection.
        /// </summary>
        private readonly AircraftProvider _aircraftProvider;
        
        /// <summary>
        /// Disposable representation of the observable that is needed when stopping the Service.
        /// </summary>
        private IDisposable? _stream;

        public PublishService(
            IHubContext<DefaultWebsocketHub> hubContext, 
            StreamListener streamListener, 
            AircraftProvider aircraftProvider
            )
        {
            _hubContext = hubContext;
            _ognStreamListener = streamListener;
            _aircraftProvider = aircraftProvider;
        }
        
        /// <summary>
        /// Magic method that is called by dependency injection on startup
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Save stream to cancel it properly on StopAsync.
            _stream = GetEventStream(_ognStreamListener.Stream)
                .Select(data => new WebsocketEntry(data, _aircraftProvider.Load(data.AircraftId)))
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
        
        /// <summary>
        /// Creates and returns an Observable that publishes all FlightData-entries that should be published.
        /// May be overridden to allow filtering, buffering etc.
        /// </summary>
        /// <param name="ognListenerStream">The raw OGN-Observable returning the received data-lines</param>
        /// <returns>Observable publishing all data that should be published</returns>
        protected virtual IObservable<FlightData> GetEventStream(IObservable<string> ognListenerStream)
        {
            // Default behaviour: Convert and publish all messages directly
            return ognListenerStream
                .Select(StreamConverter.ConvertData)
                .OfType<FlightData>(); // Remove nulls
        }
    }
}