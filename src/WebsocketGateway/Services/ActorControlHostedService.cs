namespace WebsocketGateway.Services;

/// <summary>
/// Controller that handles the working Actors which will parse and pass on incoming messages from the raw OGN-stream
/// </summary>
public class ActorControlHostedService(
    IStreamProvider streamProvider,
    IActorPropsFactory actorPropsFactory,
    ActorSystem actorSystem,
    GatewayConfiguration gatewayConfiguration
) : IHostedService
{
    public const string _MESSAGE_PROCESS_ACTOR_NAME = "MessageProcess";
    public const string _PUBLISH_ACTOR_NAME = "Publish";
    private const string _OGN_CONVERT_ACTOR_NAME = "OgnConvert";

    /// <summary>
    /// Disposable Subscription that must exist during the lifetime of this service.
    /// </summary>
    private IDisposable? _stream;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        actorSystem
           .ActorOf(actorPropsFactory.CreatePublishActorProps(), _PUBLISH_ACTOR_NAME);
        var messageProcessActor = actorSystem
           .ActorOf(actorPropsFactory.CreateMessageProcessActorProps(), _MESSAGE_PROCESS_ACTOR_NAME);
        var ognActor = actorSystem
           .ActorOf(actorPropsFactory.CreateOgnConvertActorProps(), _OGN_CONVERT_ACTOR_NAME);

        _stream = streamProvider.Stream.Subscribe(message => ognActor.Tell(message));

        if (gatewayConfiguration.HasInterval())
        {
            actorSystem.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.Zero,
                TimeSpan.FromSeconds(gatewayConfiguration.GetIntervalSeconds()),
                messageProcessActor,
                new DelayMessageProcessActor.PushMessages(),
                null
            );
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stream?.Dispose();
        return actorSystem.Terminate();
    }
}