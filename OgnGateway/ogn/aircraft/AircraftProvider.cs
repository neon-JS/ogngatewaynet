using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OgnGateway.extensions;
using OgnGateway.ogn.config;
using OgnGateway.ogn.models;

namespace OgnGateway.ogn.aircraft
{
    /// <summary>
    /// Provider for all OGN aircrafts
    /// </summary>
    public class AircraftProvider
    {
        /// <summary>
        /// Current configuration as it contains the url that should be called
        /// </summary>
        private readonly Config _config;
        
        /// <summary>
        /// Cached list containing all parsed aircrafts
        /// </summary>
        private Dictionary<string, Aircraft>? _aircraftList;

        public AircraftProvider(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Initializes the provider and downloads / parses the data.
        /// _Must_ be called before trying to Load any aircrafts
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
            if (_aircraftList == null)
            {
                throw new Exception("Provider has not been initialized!");
            }
            
            try
            {
                return _aircraftList[aircraftId];
            }
            catch (KeyNotFoundException)
            {
                // It's not uncommon to find aircrafts that are not in the OGN database.
                // In this case just return an empty one!
                return new Aircraft(aircraftId);
            }
        }

        /// <summary>
        /// Loads and parses all aircrafts from OGN into a list
        /// </summary>
        /// <returns>List of all Aircrafts known to OGN</returns>
        /// <exception cref="Exception">On invalid config or HTTP-errors</exception>
        private async Task<Dictionary<string, Aircraft>> FetchAircraftList()
        {
            _config.AircraftListUrl.EnsureNotEmpty();

            var client = new HttpClient();
            var response = await client.GetAsync(_config.AircraftListUrl);

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