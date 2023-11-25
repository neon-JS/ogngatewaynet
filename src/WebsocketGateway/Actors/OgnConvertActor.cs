namespace WebsocketGateway.Actors;

/// <summary>
/// Actor which can handle the parsing-events separately.
/// As it handles all necessary parsing-tasks, it will inform itself on the next step
/// and inform the IMessageProcessActor when finished parsing.
/// </summary>
public class OgnConvertActor : ReceiveActor
{
    public OgnConvertActor(
        IActorRefFactory actorSystem,
        IAircraftProvider aircraftProvider,
        IStreamConverter streamConverter,
        GatewayConfiguration gatewayConfiguration
    )
    {
        // When receiving the raw string from the listener, convert it to FlightData and pass it to the next Actor
        Receive<string>(message =>
        {
            var flightData = streamConverter.ConvertData(message);
            if (flightData == null)
            {
                // Ignore non-parseable messages
                return;
            }

            var aircraft = aircraftProvider.Load(flightData.AircraftId);
            if (!aircraft.Visible)
            {
                // The aircraft should not be visible, therefore drop the message.
                return;
            }

            var isFlying = flightData.Altitude >= gatewayConfiguration.MinimalAltitude
                           && flightData.Speed >= gatewayConfiguration.MinimalSpeed;

            var flightDataDto = new FlightDataDto(
                flightData.Speed,
                flightData.Altitude,
                flightData.VerticalSpeed,
                flightData.TurnRate,
                flightData.Course,
                flightData.Position,
                flightData.DateTime,
                new AircraftDto(aircraft),
                isFlying
            );

            // Pass the convertedMessage to the IMessageProcessActor so it can be further processed.
            actorSystem
                .ActorSelection($"user/{ActorControlHostedService._MESSAGE_PROCESS_ACTOR_NAME}")
                .Tell(flightDataDto, Self);
        });
    }
}