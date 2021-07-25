using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Hosting;
using OgnGateway.Providers;
using WebsocketGateway.Actors;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.Dtos;
using WebsocketGateway.Factories;

namespace WebsocketGateway.Services
{
    /// <summary>
    /// Controller that handles the working Actors which will parse and pass on incoming messages from the raw OGN-stream
    /// </summary>
    public class ActorControlService : IHostedService
    {
        /// <summary>
        /// Name of the MessageProcessActor
        /// </summary>
        public const string MessageProcessActorName = "MessageProcess";

        /// <summary>
        /// Name of the PublishActor
        /// </summary>
        public const string PublishActorName = "Publish";

        /// <summary>
        /// Name of the OgnConvertActor
        /// </summary>
        private const string OgnConvertActorName = "OgnConvert";

        /// <summary>
        /// We need the data of this Listener in the Actors as the input source
        /// </summary>
        private readonly StreamProvider _streamProvider;

        /// <summary>
        /// Is needed to create the actors based on the Props
        /// </summary>
        private readonly ActorPropsFactory _actorPropsFactory;

        /// <summary>
        /// Disposable Subscription that must exist during the lifetime of this service.
        /// </summary>
        private IDisposable? _stream;

        /// <summary>
        /// The Akka.NET ActorSystem
        /// </summary>
        private readonly ActorSystem _actorSystem;

        /// <summary>
        /// Current configuration
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        public ActorControlService(
            StreamProvider streamProvider,
            ActorPropsFactory actorPropsFactory,
            ActorSystem actorSystem,
            GatewayConfiguration gatewayConfiguration
        )
        {
            _streamProvider = streamProvider;
            _actorPropsFactory = actorPropsFactory;
            _actorSystem = actorSystem;
            _gatewayConfiguration = gatewayConfiguration;
        }

        /// <summary>
        /// Magic method that is called by dependency injection on startup.
        /// Initializes the Actors so they'll start working
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _actorSystem
                .ActorOf(_actorPropsFactory.CreatePublishActorProps(), PublishActorName);
            var messageProcessActor = _actorSystem
                .ActorOf(_actorPropsFactory.CreateMessageProcessActorProps(), MessageProcessActorName);
            var ognActor = _actorSystem
                .ActorOf(_actorPropsFactory.CreateOgnConvertActorProps(), OgnConvertActorName);

            _stream = _streamProvider.Stream.Subscribe(message => ognActor.Tell(message));

            if (_gatewayConfiguration.HasInterval())
            {
                _actorSystem.Scheduler.ScheduleTellRepeatedly(
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(_gatewayConfiguration.GetIntervalSeconds()),
                    messageProcessActor,
                    new DelayMessageProcessActor.PushMessages(),
                    null
                );
            }

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
            return _actorSystem.Terminate();
        }
    }
}
