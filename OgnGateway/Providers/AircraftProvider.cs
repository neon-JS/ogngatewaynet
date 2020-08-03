using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OgnGateway.Dtos;
using OgnGateway.Extensions.Primitives;

namespace OgnGateway.Providers
{
    /// <summary>
    /// Provider for all OGN aircraft coming from the DDB
    /// </summary>
    public class AircraftProvider
    {
        /// <summary>
        /// Current configuration as it contains the url that should be called
        /// </summary>
        private readonly AprsConfig _aprsConfig;

        /// <summary>
        /// Cached list containing all parsed aircraft
        /// </summary>
        private readonly Dictionary<string, Aircraft> _aircraftList;

        public AircraftProvider(AprsConfig aprsConfig)
        {
            _aprsConfig = aprsConfig
                          ?? throw new ArgumentNullException(nameof(aprsConfig));
            _aircraftList = new Dictionary<string, Aircraft>();
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

            return _aircraftList.ContainsKey(aircraftId)
                ? _aircraftList[aircraftId]
                : new Aircraft(aircraftId);
        }

        /// <summary>
        /// Initializes the provider and downloads / parses the data.
        /// Must be called before trying to Load any aircraft
        /// </summary>
        /// <returns>Task indicating whether initialization is done</returns>
        /// <seealso href="https://github.com/glidernet/ogn-ddb/blob/master/README.md"/>
        /// <exception cref="Exception">On invalid config or HTTP-errors</exception>
        public async Task Initialize()
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
                .Replace("'", "")
                .Split('\n')
                .ToList()
                .Where(line => !line.StartsWith('#'))
                .Select(line => line.Split(','))
                .Where(values => values.Length >= 7)
                .Select(values =>
                {
                    //              tracked (Y/N)            identified (Y/N)
                    var isVisible = values[5].Equals("Y") && values[6].Trim().Equals("Y");
                    return new Aircraft(values[1], values[4], values[3], values[2], isVisible);
                })
                .All(aircraft => _aircraftList.TryAdd(aircraft.Id, aircraft));

            if (!insertResult)
            {
                throw new Exception("Error during insertion of aircraft.");
            }
        }
    }
}