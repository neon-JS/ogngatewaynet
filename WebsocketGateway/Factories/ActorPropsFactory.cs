using System;
using Akka.Actor;
using Akka.Routing;
using Microsoft.AspNetCore.SignalR;
using OgnGateway.Ogn.Providers;
using WebsocketGateway.Actors;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.Dtos;
using WebsocketGateway.Hubs;
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
        private readonly IHubContext<DefaultHub> _hubContext;
        private readonly LatestDataProvider _latestDataProvider;

        public ActorPropsFactory(
            GatewayConfiguration gatewayConfiguration,
            ActorSystem actorSystem,
            AircraftProvider aircraftProvider,
            IHubContext<DefaultHub> hubContext,
            LatestDataProvider latestDataProvider
        )
        {
            _gatewayConfiguration = gatewayConfiguration
                                    ?? throw new ArgumentNullException(nameof(gatewayConfiguration));
            _actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
            _aircraftProvider = aircraftProvider ?? throw new ArgumentNullException(nameof(aircraftProvider));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _latestDataProvider = latestDataProvider
                                          ?? throw new ArgumentNullException(nameof(latestDataProvider));
        }

        /// <summary>
        /// Returns Props for the IMessageProcessActor based on the current configuration.
        /// </summary>
        /// <returns>Props for the IMessageProcessActor based on the current configuration</returns>
        public Props CreateMessageProcessActorProps()
        {
            // DO NOT use a Pool of Actors as we have a certain state in this actor which should be kept between the calls.
            return _gatewayConfiguration.HasInterval()
                ? Props.Create(() => new DelayMessageProcessActor(_actorSystem, _gatewayConfiguration))
                : Props.Create(() => new InstantMessageProcessActor(_actorSystem, _gatewayConfiguration));
        }

        /// <summary>
        /// Returns Props for the OgnConvertActor based on the current configuration.
        /// </summary>
        /// <returns>Props for the OgnConvertActor based on the current configuration</returns>
        /// <exception cref="ArgumentOutOfRangeException">If Workers number is invalid</exception>
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

        /// <summary>
        /// Returns Props for the PublishActor
        /// </summary>
        /// <returns>Props for the PublishActor</returns>
        public Props CreatePublishActorProps()
        {
            if (_gatewayConfiguration.Workers <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_gatewayConfiguration.Workers));
            }

            return Props
                .Create(() => new PublishActor(_hubContext, _latestDataProvider))
                .WithRouter(new SmallestMailboxPool(_gatewayConfiguration.Workers));
        }
     }
}