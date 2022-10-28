using System;
using Akka.Actor;
using Akka.Routing;
using OgnGateway.Providers;
using OgnGateway.Services;
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
    public class ActorPropsFactory : IActorPropsFactory
    {
        private readonly GatewayConfiguration _gatewayConfiguration;
        private readonly IActorRefFactory _actorRefFactory;
        private readonly IAircraftProvider _aircraftProvider;
        private readonly ILatestDataProvider _latestDataProvider;
        private readonly IWebsocketService _websocketService;
        private readonly IStreamConverter _streamConverter;

        public ActorPropsFactory(
            GatewayConfiguration gatewayConfiguration,
            IActorRefFactory actorRefFactory,
            IAircraftProvider aircraftProvider,
            ILatestDataProvider latestDataProvider,
            IWebsocketService websocketService,
            IStreamConverter streamConverter
        )
        {
            _gatewayConfiguration = gatewayConfiguration;
            _actorRefFactory = actorRefFactory;
            _aircraftProvider = aircraftProvider;
            _latestDataProvider = latestDataProvider;
            _websocketService = websocketService;
            _streamConverter = streamConverter;
        }

        public Props CreateMessageProcessActorProps()
        {
            // DO NOT use a Pool of Actors as we have a certain state in this actor which should be kept between the calls.
            return _gatewayConfiguration.HasInterval()
                ? Props.Create(() => new DelayMessageProcessActor(_actorRefFactory, _gatewayConfiguration, _latestDataProvider))
                : Props.Create(() => new InstantMessageProcessActor(_actorRefFactory, _gatewayConfiguration, _latestDataProvider));
        }

        public Props CreateOgnConvertActorProps()
        {
            if (_gatewayConfiguration.Workers <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_gatewayConfiguration.Workers));
            }

            return Props
                .Create(() => new OgnConvertActor(_actorRefFactory, _aircraftProvider, _streamConverter, _gatewayConfiguration))
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
