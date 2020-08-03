using System;
using System.Collections.Generic;
using Akka.Actor;
using WebsocketGateway.Dtos;
using WebsocketGateway.Services;

namespace WebsocketGateway.Actors
{
    /// <summary>
    /// Variant of the MessageProcessActor that publishes incoming messages immediately.
    /// </summary>
    public class InstantMessageProcessActor : ReceiveActor
    {
        /// <summary>
        /// Contains the last received aircraft-data for an aircraft
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _latestAircraftMessages;

        /// <summary>
        /// Akka.NET ActorSystem that is needed to find the next Actor if necessary
        /// </summary>
        private readonly ActorSystem _actorSystem;

        /// <summary>
        /// Our current configuration
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        public InstantMessageProcessActor(
            ActorSystem actorSystem,
            GatewayConfiguration gatewayConfiguration
        )
        {
            _actorSystem = actorSystem
                           ?? throw new ArgumentNullException(nameof(actorSystem));
            _gatewayConfiguration = gatewayConfiguration
                                    ?? throw new ArgumentNullException(nameof(gatewayConfiguration));
            _latestAircraftMessages = new Dictionary<string, FlightDataDto>();

            SetUpReceiver();
        }

        /// <summary>
        /// Sets up the Actor so it handles incoming messages
        /// </summary>
        private void SetUpReceiver()
        {
            Receive<FlightDataDto>(message =>
            {
                var aircraftId = message.Aircraft.Id;

                var isEvent = !_latestAircraftMessages.ContainsKey(aircraftId)
                              || _latestAircraftMessages[aircraftId].IsFlying != message.IsFlying;

                _latestAircraftMessages[aircraftId] = message;

                if (isEvent || !_gatewayConfiguration.EventsOnly)
                {
                    // Immediately inform the PublishActor about this message
                    _actorSystem
                        .ActorSelection($"user/{ActorControlService.PublishActorName}")
                        .Tell(message, Self);
                }
            });
        }
    }
}