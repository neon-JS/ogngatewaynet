using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebsocketGateway.Services;

namespace WebsocketGateway.Controllers;

[Route("websocket")]
public class WebsocketController : Controller
{
    private readonly IWebsocketService _websocketService;

    public WebsocketController(
        IWebsocketService websocketService
    )
    {
        _websocketService = websocketService;
    }

    [HttpGet]
    public async Task ConnectAsync()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        if (await HttpContext.WebSockets.AcceptWebSocketAsync() is not { } websocket)
        {
            HttpContext.Response.StatusCode = 500;
            return;
        }

        var websocketStatusChange = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => websocket.State);

        async void CloseWebsocketConnection()
        {
            if (websocket.State is WebSocketState.CloseReceived or WebSocketState.Open)
            {
                await websocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    null,
                    CancellationToken.None
                );
            }
        }

        async void SendWebsocketMessage(byte[] message) => await websocket.SendAsync(
            new ArraySegment<byte>(message),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        await _websocketService.Messages
            .WithLatestFrom(websocketStatusChange)
            .TakeWhile(tuple => tuple.Second == WebSocketState.Open)
            .Select(tuple => tuple.First)
            .Finally(CloseWebsocketConnection)
            .Do(SendWebsocketMessage);
    }
}