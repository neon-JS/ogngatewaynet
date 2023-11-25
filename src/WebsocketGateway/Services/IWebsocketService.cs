namespace WebsocketGateway.Services;

public interface IWebsocketService
{
    IObservable<byte[]> Messages { get; }

    void Notify(object serializable);
}