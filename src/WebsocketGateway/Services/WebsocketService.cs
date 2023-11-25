namespace WebsocketGateway.Services;

public class WebsocketService : IWebsocketService
{
    public IObservable<byte[]> Messages => _messages;
    private readonly Subject<byte[]> _messages = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public void Notify(object serializable)
    {
        var json = JsonSerializer.Serialize(serializable, _jsonSerializerOptions);
        var message = new UTF8Encoding().GetBytes(json);
        _messages.OnNext(message);
    }
}