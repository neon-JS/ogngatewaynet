using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OgnGateway.Dtos;
using OgnGateway.Extensions.Primitives;

namespace OgnGateway.Providers;

/// <summary>
/// Provider for all OGN aircraft coming from the DDB
/// </summary>
public class AircraftProvider : IAircraftProvider
{
    private const string _VALUE_YES = "Y";
    private const string _FIELD_ENCLOSURE = "'";
    private const char _FIELD_SEPARATOR = ',';
    private const char _IDENTIFIER_COMMENT = '#';
    private const char _LINE_BREAK = '\n';

    private const int _INDEX_AIRCRAFT_ID = 1;
    private const int _INDEX_TYPE = 2;
    private const int _INDEX_REGISTRATION = 3;
    private const int _INDEX_CALL_SIGN = 4;
    private const int _INDEX_TRACKED = 5;
    private const int _INDEX_IDENTIFIED = 6;

    /// <summary>
    /// Cached list containing all parsed aircraft
    /// </summary>
    private readonly Dictionary<string, Aircraft> _aircraftList;

    private readonly AprsConfig _aprsConfig;

    public AircraftProvider(
        AprsConfig aprsConfig
    )
    {
        _aircraftList = new Dictionary<string, Aircraft>();
        _aprsConfig = aprsConfig;
    }

    /// <summary>
    /// Loads aircraft by given ID.
    /// If aircraft cannot be found, an empty Aircraft will be returned.
    /// </summary>
    /// <param name="aircraftId">OGN ID of the aircraft</param>
    /// <returns>Representation of an Aircraft</returns>
    public Aircraft Load(string aircraftId)
    {
        aircraftId.EnsureNotEmpty();

        return _aircraftList.TryGetValue(aircraftId, out var value)
            ? value
            : new Aircraft(aircraftId);
    }

    /// <summary>
    /// Initializes the provider and downloads / parses the data.
    /// Must be called before trying to Load any aircraft
    /// </summary>
    /// <returns>Task indicating whether initialization is done</returns>
    /// <seealso href="https://github.com/glidernet/ogn-ddb/blob/master/README.md"/>
    /// <exception cref="Exception">On invalid config or HTTP-errors</exception>
    public async Task InitializeAsync()
    {
        _aprsConfig.DdbAircraftListUrl.EnsureNotEmpty();

        var client = new HttpClient();
        var response = await client.GetAsync(_aprsConfig.DdbAircraftListUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Request to aircraft database failed!");
        }

        var content = await response.Content.ReadAsStringAsync();

        var insertResult = content
            .Replace(_FIELD_ENCLOSURE, string.Empty)
            .Split(_LINE_BREAK)
            .Where(line => !line.StartsWith(_IDENTIFIER_COMMENT))
            .Select(line => line.Split(_FIELD_SEPARATOR))
            .Select(values => values.Select(v => v.Trim()).ToList())
            .Where(values => values.Count >= 7)
            .Where(values => !string.IsNullOrWhiteSpace(values[_INDEX_AIRCRAFT_ID]))
            .Select(values => new Aircraft(
                values[_INDEX_AIRCRAFT_ID],
                values[_INDEX_CALL_SIGN],
                values[_INDEX_REGISTRATION],
                values[_INDEX_TYPE],
                values[_INDEX_TRACKED].Equals(_VALUE_YES) && values[_INDEX_IDENTIFIED].Equals(_VALUE_YES)
            ))
            .All(aircraft => _aircraftList.TryAdd(aircraft.Id, aircraft));

        if (!insertResult)
        {
            throw new Exception("Error during insertion of aircraft.");
        }
    }
}