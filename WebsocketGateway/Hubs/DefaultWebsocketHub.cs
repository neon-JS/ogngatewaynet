using Microsoft.AspNetCore.SignalR;

namespace WebsocketGateway.Hubs
{
    /// <summary>
    /// This hub is needed as a dummy because IHubContext needs a Hub type.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DefaultWebsocketHub: Hub
    {
    }
}