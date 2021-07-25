using Akka.Actor;
using OgnGateway.Dtos;
using OgnGateway.Providers;
using OgnGateway.Services;
using WebsocketGateway.Dtos;
using WebsocketGateway.Services;

namespace WebsocketGateway.Actors
{
    /// <summary>
    /// Actor which can handle the parsing-events separately.
    /// As it handles all necessary parsing-tasks, it will inform itself on the next step
    /// and inform the IMessageProcessActor when finished parsing.
    /// </summary>
    public class OgnConvertActor : ReceiveActor
    {
        public OgnConvertActor(
            IActorRefFactory actorSystem,
            AircraftProvider aircraftProvider,
            GatewayConfiguration gatewayConfiguration
        )
        {
            // When receiving the raw string from the listener, convert it to FlightData and pass it to the next Actor
            Receive<string>(message =>
            {
                var convertedMessage = StreamConversionService.ConvertData(message);
                if (convertedMessage == null)
                {
                    // Ignore non-parseable messages
                    return;
                }

                // The next step also happens on this Actor so tell another "Self" to handle the FlightData
                actorSystem.ActorSelection(Self.Path).Tell(convertedMessage, Self);
            });

            // When receiving FlightData, convert it into FlightDataDto and pass it to the next actor
            Receive<FlightData>(message =>
            {
                var aircraft = aircraftProvider.Load(message.AircraftId);
                if (!aircraft.Visible)
                {
                    // The aircraft should not be visible, therefore drop the message.
                    return;
                }

                var flying = message.Altitude >= gatewayConfiguration.MinimalAltitude
                             && message.Speed >= gatewayConfiguration.MinimalSpeed;

                var convertedMessage = new FlightDataDto(
                    message.Speed,
                    message.Altitude,
                    message.VerticalSpeed,
                    message.TurnRate,
                    message.Course,
                    message.Position,
                    message.DateTime,
                    new AircraftDto(aircraft),
                    flying
                );

                // Pass the convertedMessage to the IMessageProcessActor so it can be further processed.
                actorSystem
                    .ActorSelection($"user/{ActorControlService.MessageProcessActorName}")
                    .Tell(convertedMessage, Self);
            });
        }
    }
}
