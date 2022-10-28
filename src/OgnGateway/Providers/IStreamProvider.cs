using System;

namespace OgnGateway.Providers
{
    public interface IStreamProvider
    {
        IObservable<string> Stream { get; }
    }
}