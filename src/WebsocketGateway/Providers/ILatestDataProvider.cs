namespace WebsocketGateway.Providers;

public interface ILatestDataProvider
{
    FlightDataDto? Get(string aircraftId);
    IReadOnlyList<FlightDataDto> GetLatestData();
    void Push(FlightDataDto flightData);
}