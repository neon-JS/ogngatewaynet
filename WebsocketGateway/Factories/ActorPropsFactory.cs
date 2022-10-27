using System;
using Akka.Actor;
using Akka.Routing;
using OgnGateway.Providers;
using WebsocketGateway.Actors;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.Dtos;
using WebsocketGateway.Providers;
using WebsocketGateway.Services;

namespace WebsocketGateway.Factories
{
    /// <summary>
    /// Factory which returns the configuration (as <see cref="Akka.Actor.Props"/>) for out Actors
    /// </summary>
    public class ActorPropsFactory
    {
        private readonly GatewayConfiguration _gatewayConfiguration;
        private readonly ActorSystem _actorSystem;
        private readonly AircraftProvider _aircraftProvider;
        private readonly LatestDataProvider _latestDataProvider;
        private readonly WebsocketService _websocketService;

        public ActorPropsFactory(
            GatewayConfiguration gatewayConfiguration,
            ActorSystem actorSystem,
            AircraftProvider aircraftProvider,
            LatestDataProvider latestDataProvider,
            WebsocketService websocketService
        )
        {
            _gatewayConfiguration = gatewayConfiguration;
            _actorSystem = actorSystem;
            _aircraftProvider = aircraftProvider;
            _latestDataProvider = latestDataProvider;
            _websocketService = websocketService;
        }

        public Props CreateMessageProcessActorProps()
        {
            // DO NOT use a Pool of Actors as we have a certain state in this actor which should be kept between the calls.
            return _gatewayConfiguration.HasInterval()
                ? Props.Create(() => new DelayMessageProcessActor(_actorSystem, _gatewayConfiguration, _latestDataProvider))
                : Props.Create(() => new InstantMessageProcessActor(_actorSystem, _gatewayConfiguration));
        }

       public Props CreateOgnConvertActorProps()
        {
            if (_gatewayConfiguration.Workers <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_gatewayConfiguration.Workers));
            }

            return Props
                .Create(() => new OgnConvertActor(_actorSystem, _aircraftProvider, _gatewayConfiguration))
                .WithRouter(new SmallestMailboxPool(_gatewayConfiguration.Workers));
        }

        public Props CreatePublishActorProps()
        {
            if (_gatewayConfiguration.Workers <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_gatewayConfiguration.Workers));
            }

            return Props
                .Create(() => new PublishActor(_websocketService, _latestDataProvider))
                .WithRouter(new SmallestMailboxPool(_gatewayConfiguration.Workers));
        }
    }
}
