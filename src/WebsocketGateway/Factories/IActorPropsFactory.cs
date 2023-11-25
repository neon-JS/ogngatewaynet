namespace WebsocketGateway.Factories;

public interface IActorPropsFactory
{
    Props CreateMessageProcessActorProps();
    Props CreateOgnConvertActorProps();
    Props CreatePublishActorProps();
}