using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Hosting;
using OgnGateway.Providers;
using WebsocketGateway.Actors;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.Dtos;
using WebsocketGateway.Factories;

namespace WebsocketGateway.Services;

/// <summary>
/// Controller that handles the working Actors which will parse and pass on incoming messages from the raw OGN-stream
/// </summary>
public class ActorControlHostedService : IHostedService
{
    public const string MessageProcessActorName = "MessageProcess";
    public const string PublishActorName = "Publish";
    private const string _OGN_CONVERT_ACTOR_NAME = "OgnConvert";

    /// <summary>
    /// Disposable Subscription that must exist during the lifetime of this service.
    /// </summary>
    private IDisposable? _stream;

    private readonly IStreamProvider _streamProvider;
    private readonly IActorPropsFactory _actorPropsFactory;
    private readonly ActorSystem _actorSystem;
    private readonly GatewayConfiguration _gatewayConfiguration;

    public ActorControlHostedService(
        IStreamProvider streamProvider,
        IActorPropsFactory actorPropsFactory,
        ActorSystem actorSystem,
        GatewayConfiguration gatewayConfiguration
    )
    {
        _streamProvider = streamProvider;
        _actorPropsFactory = actorPropsFactory;
        _actorSystem = actorSystem;
        _gatewayConfiguration = gatewayConfiguration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _actorSystem
            .ActorOf(_actorPropsFactory.CreatePublishActorProps(), PublishActorName);
        var messageProcessActor = _actorSystem
            .ActorOf(_actorPropsFactory.CreateMessageProcessActorProps(), MessageProcessActorName);
        var ognActor = _actorSystem
            .ActorOf(_actorPropsFactory.CreateOgnConvertActorProps(), _OGN_CONVERT_ACTOR_NAME);

        _stream = _streamProvider.Stream.Subscribe(message => ognActor.Tell(message));

        if (_gatewayConfiguration.HasInterval())
        {
            _actorSystem.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.Zero,
                TimeSpan.FromSeconds(_gatewayConfiguration.GetIntervalSeconds()),
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
        return _actorSystem.Terminate();
    }
}