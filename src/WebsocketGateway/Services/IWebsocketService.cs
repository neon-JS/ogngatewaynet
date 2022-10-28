using System;

namespace WebsocketGateway.Services
{
    public interface IWebsocketService
    {
        IObservable<byte[]> Messages { get; }

        void Notify(byte[] message);
        void Notify(object serializable);
    }
}