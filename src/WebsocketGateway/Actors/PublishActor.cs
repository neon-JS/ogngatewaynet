namespace WebsocketGateway.Actors;

/// <summary>
/// Actor which publishes incoming messages to the websocket-clients
/// </summary>
public class PublishActor : ReceiveActor
{
    public PublishActor(
        IWebsocketService websocketService,
        ILatestDataProvider latestDataProvider
    )
    {
        Receive<FlightDataDto>(message =>
        {
            latestDataProvider.Push(message);
            websocketService.Notify(message);
        });
    }
}