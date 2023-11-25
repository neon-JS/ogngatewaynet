namespace WebsocketGateway.Factories;

/// <summary>
/// Factory which returns the configuration (as <see cref="Akka.Actor.Props"/>) for out Actors
/// </summary>
public class ActorPropsFactory(
    GatewayConfiguration gatewayConfiguration,
    IActorRefFactory actorRefFactory,
    IAircraftProvider aircraftProvider,
    ILatestDataProvider latestDataProvider,
    IWebsocketService websocketService,
    IStreamConverter streamConverter
) : IActorPropsFactory
{
    public Props CreateMessageProcessActorProps()
    {
        // DO NOT use a Pool of Actors as we have a certain state in this actor which should be kept between the calls.
        return gatewayConfiguration.HasInterval()
            ? Props.Create(() => new DelayMessageProcessActor(
                    actorRefFactory,
                    gatewayConfiguration,
                    latestDataProvider
                )
            )
            : Props.Create(() => new InstantMessageProcessActor(
                    actorRefFactory,
                    gatewayConfiguration,
                    latestDataProvider
                )
            );
    }

    public Props CreateOgnConvertActorProps()
    {
        if (gatewayConfiguration.Workers <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gatewayConfiguration.Workers));
        }

        return Props
           .Create(() => new OgnConvertActor(
                    actorRefFactory,
                    aircraftProvider,
                    streamConverter,
                    gatewayConfiguration
                )
            )
           .WithRouter(new SmallestMailboxPool(gatewayConfiguration.Workers));
    }

    public Props CreatePublishActorProps()
    {
        if (gatewayConfiguration.Workers <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gatewayConfiguration.Workers));
        }

        return Props
           .Create(() => new PublishActor(websocketService, latestDataProvider))
           .WithRouter(new SmallestMailboxPool(gatewayConfiguration.Workers));
    }
}