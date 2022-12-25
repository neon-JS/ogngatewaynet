using System.Text.Json;
using System.Text;
using System.Reactive.Subjects;
using System;

namespace WebsocketGateway.Services;

public class WebsocketService : IWebsocketService
{
    public IObservable<byte[]> Messages => _messages;
    private readonly Subject<byte[]> _messages;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public WebsocketService()
    {
        _messages = new Subject<byte[]>();
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };
    }

    public void Notify(byte[] message)
    {
        _messages.OnNext(message);
    }

    public void Notify(object serializable)
    {
        var json = JsonSerializer.Serialize(serializable, _jsonSerializerOptions);
        byte[] message = new UTF8Encoding().GetBytes(json);
        Notify(message);
    }
}