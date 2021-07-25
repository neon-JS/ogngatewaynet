using System;
using System.Text.RegularExpressions;
using OgnGateway.Dtos;
using OgnGateway.Extensions.Primitives;

namespace OgnGateway.Services
{
    /// <summary>
    /// Converter for getting models from stream data messages
    /// </summary>
    public static class StreamConversionService
    {
        /// <summary>
        /// Main pattern for getting all needed information from a raw stream-line.
        /// </summary>
        /// <remarks>
        /// Note that at the part of "idXXYYYYYY", "XX" must not be 40 or higher!
        /// This is due to the fact that this 2-digit hex number contains the tracking-information as _binary_ in the
        /// form of "0bSTxxxxxx" and if S = 1 or T = 1, we should discard the message.
        /// So all "allowed" values are in the range of 0b00000000 - 0b00111111, or in hex: 0x00 - 0x3f,
        /// therefore we can discard all messages not in this range.
        /// <seealso href="https://github.com/dbursem/ogn-client-php/blob/master/lib/OGNClient.php#L87"/>
        /// </remarks>
        private const string LineMatchPattern =
            @".*?h([0-9.]*[NS])[/\\]([0-9.]*[WE]).*?(\d{3})/(\d{3})/A=(\d+).*?id[0-3]{1}[A-Fa-f0-9]{1}([A-Za-z0-9]+).*?([-0-9]+)fpm.*?([-.0-9]+)rot.*";

        /// <summary>
        /// Pattern for converting coordinate strings to valid numeric string
        /// (aka "remove all non-numeric chars")
        /// </summary>
        private const string CoordinateReplacePattern = @"[^\d]";

        /// <summary>
        /// Factor to convert knots to km/h
        /// </summary>
        private const float FactorKnotsToKmH = 1.852f;

        /// <summary>
        /// Factor to convert ft to m
        /// </summary>
        private const float FactorFtToM = 0.3048f;

        /// <summary>
        /// Factor to convert ft/min to m/s 
        /// </summary>
        private const float FactorFtMinToMSec = 0.00508f;

        /// <summary>
        /// Factor to convert "turns/2min" to "turns/min"
        /// </summary>
        private const float FactorTurnsTwoMinToTurnsMin = 0.5f;

        /// <summary>
        /// Tries converting a stream-line to FlightData model
        /// </summary>
        /// <param name="line">A line that was received by the OGN live stream</param>
        /// <returns>FlightData representation of the data</returns>
        public static FlightData? ConvertData(string line)
        {
            line.EnsureNotEmpty();

            var match = Regex.Match(line, LineMatchPattern);
            if (!match.Success)
            {
                return null;
            }

            var data = match.Groups;

            var latitude = ConvertCoordinateValue(data, 1);
            var longitude = ConvertCoordinateValue(data, 2);
            var course = Convert(data, 3);
            var speed = Convert(data, 4, FactorKnotsToKmH);
            var altitude = Convert(data, 5, FactorFtToM);
            var aircraftId = data[6].Value;
            var verticalSpeed = Convert(data, 7, FactorFtMinToMSec);
            var turnRate = Math.Abs(Convert(data, 8, FactorTurnsTwoMinToTurnsMin));

            return new FlightData(
                aircraftId,
                speed, altitude,
                verticalSpeed,
                turnRate,
                course,
                latitude,
                longitude,
                DateTime.Now
            );
        }

        /// <summary>
        /// Converts the match-result from the regex to a float and multiplies it with a given factor to convert units
        /// </summary>
        /// <param name="collection">Result of the regex match</param>
        /// <param name="index">Result index</param>
        /// <param name="factor">The factor that should be applied to the value</param>
        /// <returns></returns>
        private static float Convert(GroupCollection collection, int index, float factor = 1)
        {
            return (float) System.Convert.ToDouble(collection[index].Value) * factor;
        }

        /// <summary>
        /// Converts the strings representing coordinate values to a common float representation
        /// </summary>
        /// <param name="collection">Result of the regex match</param>
        /// <param name="index">Result index</param>
        /// <returns></returns>
        private static float ConvertCoordinateValue(GroupCollection collection, int index)
        {
            /* Latitude and longitude (by APRS-standard) are given as following: ddmm.mmD where d = "degree", m = "minute" and D = "direction".
             * Notice that minutes are decimals, so 0.5 minutes equal 0 minutes, 30 secs.
             * We'll separate degrees and minutes, so we can convert it to a "degree"-only value.
             */
            var rawValue = collection[index].Value;

            var numericValue = System.Convert.ToDouble(Regex.Replace(rawValue, CoordinateReplacePattern, ""));
            var orientation = rawValue[^1..];

            var degrees = Math.Floor(numericValue / 1_0000); // Separating   "dd" from "ddmmmm"
            var minutes = Math.Floor(numericValue % 1_0000) // Separating "mmmm" from "ddmmmm"
                          / 60 // because 60 minutes = 1 degree
                          / 100; // because of the removed decimal separator

            var coordinateValue = (float) (degrees + minutes);

            return orientation.Equals("S") || orientation.Equals("W")
                ? coordinateValue * -1 // S/W are seen as negative!
                : coordinateValue;
        }
    }
}