using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using Microsoft.Extensions.Hosting;
using OgnGateway.Ogn.Providers;
using OgnGateway.Ogn.Stream;
using WebsocketGateway.Config;
using WebsocketGateway.Services.MessageProcessing;
using WebsocketGateway.Services.Ogn.Actors;

namespace WebsocketGateway.Services.Ogn
{
    /// <summary>
    /// Controller that handles the working Actors which will parse and pass on incoming messages from the raw OGN-stream
    /// </summary>
    public class OgnActorControllerService : IHostedService
    {
        /// <summary>
        /// Name of the Actor-System
        /// </summary>
        private const string SystemName = "OgnSystem";

        /// <summary>
        /// We need this provider in the Actors during parsing
        /// </summary>
        private readonly AircraftProvider _aircraftProvider;

        /// <summary>
        /// We need this in the Actors when passing the converted information
        /// </summary>
        private readonly IMessageProcessService _messageProcessService;

        /// <summary>
        /// We need the data of this Listener in the Actors as the input source
        /// </summary>
        private readonly StreamListener _streamListener;

        /// <summary>
        /// Current configuration that is needed to determine the number of workers to create
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        /// <summary>
        /// Disposable Subscription that must exist during the lifetime of this service.
        /// </summary>
        private IDisposable? _stream;

        /// <summary>
        /// The Akka.NET ActorSystem
        /// </summary>
        private readonly ActorSystem _system;

        public OgnActorControllerService(
            AircraftProvider aircraftProvider,
            IMessageProcessService messageProcessService,
            StreamListener streamListener,
            GatewayConfiguration gatewayConfiguration
        )
        {
            _aircraftProvider = aircraftProvider ?? throw new ArgumentNullException(nameof(aircraftProvider));
            _messageProcessService = messageProcessService
                                     ?? throw new ArgumentNullException(nameof(messageProcessService));
            _streamListener = streamListener ?? throw new ArgumentNullException(nameof(streamListener));
            _gatewayConfiguration = gatewayConfiguration
                                    ?? throw new ArgumentNullException(nameof(gatewayConfiguration));
            _system = ActorSystem.Create(SystemName);

            if (_gatewayConfiguration.Workers <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_gatewayConfiguration.Workers));
            }
        }

        /// <summary>
        /// Magic method that is called by dependency injection on startup.
        /// Initializes the Actors so they'll start working
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var ognActorsProps = Props
                .Create(() => new OgnActor(_system, _aircraftProvider, _messageProcessService))
                .WithRouter(new SmallestMailboxPool(_gatewayConfiguration.Workers));
            var ognActor = _system.ActorOf(ognActorsProps, OgnActor.Name);

            _stream = _streamListener.Stream.Subscribe(message => ognActor.Tell(message));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Magic method that is called by dependency injection on shutdown.
        /// Stops listening streams and the ActorSystem
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stream?.Dispose();
            return _system.Terminate();
        }
    }
}