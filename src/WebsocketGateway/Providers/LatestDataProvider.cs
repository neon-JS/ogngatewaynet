namespace WebsocketGateway.Providers;

/// <summary>
/// A provider that is used to hold active airplane-data.
/// </summary>
public class LatestDataProvider(GatewayConfiguration gatewayConfiguration) : ILatestDataProvider
{
    /// <summary>
    /// The currently active flight-data.
    /// </summary>
    private readonly IDictionary<string, FlightDataDto> _activeFlightData = new Dictionary<string, FlightDataDto>();

    /// <summary>
    /// We need our current configuration to determine which outdated flight-data we should drop.
    /// </summary>
    private readonly GatewayConfiguration _gatewayConfiguration = gatewayConfiguration;

    public void Push(FlightDataDto flightData)
    {
        _activeFlightData[flightData.Aircraft.Id] = flightData;
    }

    public FlightDataDto? Get(string aircraftId)
    {
        return _activeFlightData.TryGetValue(aircraftId, out var flightData)
            ? flightData
            : null;
    }

    /// <summary>
    /// Returns the currently active flight-data.
    /// </summary>
    /// <returns>currently active flight-data</returns>
    public IReadOnlyList<FlightDataDto> GetLatestData()
    {
        /* Removes outdated entries. We call this method on demand as an interval-call would not make sense.*/
        _activeFlightData
           .Select(entry => entry.Value)
           .Where(dto => dto.DateTime.AddSeconds(_gatewayConfiguration.MaxAgeSeconds).IsInPast())
           .ForEach(dto => _activeFlightData.Remove(dto.Aircraft.Id));

        return _activeFlightData.Values.ToList();
    }
}