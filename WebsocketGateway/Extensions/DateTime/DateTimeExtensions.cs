using System;

namespace WebsocketGateway.Extensions.DateTime
{
    /// <summary>
    /// Class containing extension methods for the DateTime object
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns whether the given DateTime is in past
        /// </summary>
        /// <param name="dateTime">DateTime to check</param>
        /// <returns>if DateTime is in past</returns>
        /// <exception cref="ArgumentNullException">when DateTime is null</exception>
        public static bool IsInPast(this System.DateTime dateTime)
        {
            if (dateTime == null) throw new ArgumentNullException(nameof(dateTime));

            return dateTime.CompareTo(System.DateTime.Now) == -1;
        }
    }
}