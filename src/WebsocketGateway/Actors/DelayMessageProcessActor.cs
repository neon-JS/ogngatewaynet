namespace WebsocketGateway.Actors;

/// <summary>
/// Variant of the MessageProcessActor that queues and delays incoming messages for the configured timespan.
/// Event messages are published immediately.
/// </summary>
public class DelayMessageProcessActor : ReceiveActor
{
    /// <summary>
    /// When received, the DelayMessageProcessActor will push all its messages to the PublishActor
    /// </summary>
    public class PushMessages
    {
    }

    /// <summary>
    /// Contains all latest aircraft-data that have to be processed, identified by the aircraft-Id
    /// </summary>
    private readonly IDictionary<string, FlightDataDto> _messageQueue;

    private readonly IActorRefFactory _actorRefFactory;
    private readonly GatewayConfiguration _gatewayConfiguration;
    private readonly ILatestDataProvider _latestDataProvider;

    public DelayMessageProcessActor(
        IActorRefFactory actorRefFactory,
        GatewayConfiguration gatewayConfiguration,
        ILatestDataProvider latestDataProvider
    )
    {
        _messageQueue = new Dictionary<string, FlightDataDto>();

        _actorRefFactory = actorRefFactory;
        _gatewayConfiguration = gatewayConfiguration;
        _latestDataProvider = latestDataProvider;

        SetUpReceiver();
    }

    private void SetUpReceiver()
    {
        Receive<FlightDataDto>(message =>
        {
            var aircraftId = message.Aircraft.Id;
            var latestEntry = _latestDataProvider.Get(aircraftId);

            var isEvent = latestEntry is null
                          || latestEntry.Flying != message.Flying
                          || latestEntry.DateTime
                              .AddSeconds(_gatewayConfiguration.MaxAgeSeconds)
                              .IsInPast();

            if (isEvent)
            {
                // Immediately inform the PublishActor about this message as it's an event
                _actorRefFactory
                    .ActorSelection($"user/{ActorControlHostedService._PUBLISH_ACTOR_NAME}")
                    .Tell(message, Self);
            }
            else if (!_gatewayConfiguration.EventsOnly)
            {
                // Just queue the message so it can be handled later
                _messageQueue[aircraftId] = message;
            }
        });

        Receive<PushMessages>(_ =>
        {
            var entries = _messageQueue.ToList();
            _messageQueue.Clear();

            entries
                .Select(kv => kv.Value)
                .ForEach(entry =>
                    _actorRefFactory
                        .ActorSelection($"user/{ActorControlHostedService._PUBLISH_ACTOR_NAME}")
                        .Tell(entry, Self)
                );
        });
    }
}