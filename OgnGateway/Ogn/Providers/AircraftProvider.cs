using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OgnGateway.Extensions;
using OgnGateway.Ogn.Config;
using OgnGateway.Ogn.Models;

namespace OgnGateway.Ogn.Providers
{
    /// <summary>
    /// Provider for all OGN aircraft
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
        private Dictionary<string, Aircraft>? _aircraftList;

        public AircraftProvider(AprsConfig aprsConfig)
        {
            _aprsConfig = aprsConfig ?? throw new ArgumentNullException(nameof(aprsConfig));
        }

        /// <summary>
        /// Initializes the provider and downloads / parses the data.
        /// _Must_ be called before trying to Load any aircraft
        /// </summary>
        /// <returns>Task indicating whether initialization is done</returns>
        public async Task Initialize()
        {
            _aircraftList = await FetchAircraftList();
        }

        /// <summary>
        /// Loads aircraft by given ID.
        /// If aircraft cannot be found, an empty Aircraft will be returned
        /// </summary>
        /// <param name="aircraftId">OGN ID of the aircraft</param>
        /// <returns>Representation of an Aircraft</returns>
        /// <exception cref="Exception">When Provider has not been initialized</exception>
        public Aircraft Load(string aircraftId)
        {
            aircraftId.EnsureNotEmpty();

            if (_aircraftList == null)
            {
                throw new Exception("Provider has not been initialized!");
            }

            return _aircraftList.ContainsKey(aircraftId) ? _aircraftList[aircraftId] : new Aircraft(aircraftId);
        }

        /// <summary>
        /// Loads and parses all aircraft from OGN into a list
        /// </summary>
        /// <returns>List of all aircraft known to OGN</returns>
        /// <exception cref="Exception">On invalid config or HTTP-errors</exception>
        private async Task<Dictionary<string, Aircraft>> FetchAircraftList()
        {
            _aprsConfig.AircraftListUrl.EnsureNotEmpty();

            var client = new HttpClient();
            var response = await client.GetAsync(_aprsConfig.AircraftListUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Request to aircraft database failed!");
            }

            var content = await response.Content.ReadAsStringAsync();

            var aircraftList = new Dictionary<string, Aircraft>();

            var insertResult = content
                .Replace("'", "")
                .Split('\n')
                .ToList()
                .Where(line => !line.StartsWith('#'))
                .Select(line => line.Split(','))
                .Where(values => values.Length >= 5)
                .Select(values => new Aircraft(values[1], values[4], values[3], values[2]))
                .All(aircraft => aircraftList.TryAdd(aircraft.Id, aircraft));

            if (!insertResult)
            {
                throw new Exception("Error during insertion of aircraft.");
            }

            return aircraftList;
        }
    }
}