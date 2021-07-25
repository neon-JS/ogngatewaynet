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
        private const string ValueYes = "Y";
        private const string FieldEnclosure = "'";
        private const char FieldSeparator = ',';
        private const char IdentifierComment = '#';
        private const char LineBreak = '\n';

        private const int IndexAircraftId = 1;
        private const int IndexType = 2;
        private const int IndexRegistration = 3;
        private const int IndexCallSign = 4;
        private const int IndexTracked = 5;
        private const int IndexIdentified = 6;

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
            _aprsConfig = aprsConfig;
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
                .Replace(FieldEnclosure, string.Empty)
                .Split(LineBreak)
                .Where(line => !line.StartsWith(IdentifierComment))
                .Select(line => line.Split(FieldSeparator))
                .Select(values => values.Select(v => v.Trim()).ToList())
                .Where(values => values.Count >= 7)
                .Where(values => !string.IsNullOrWhiteSpace(values[IndexAircraftId]))
                .Select(values => new Aircraft(
                    values[IndexAircraftId],
                    values[IndexCallSign],
                    values[IndexRegistration],
                    values[IndexType],
                    values[IndexTracked].Equals(ValueYes) && values[IndexIdentified].Equals(ValueYes)
                ))
                .All(aircraft => _aircraftList.TryAdd(aircraft.Id, aircraft));

            if (!insertResult)
            {
                throw new Exception("Error during insertion of aircraft.");
            }
        }
    }
}
