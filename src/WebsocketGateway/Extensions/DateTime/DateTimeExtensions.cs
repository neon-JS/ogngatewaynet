namespace WebsocketGateway.Extensions.DateTime;

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
        return dateTime.CompareTo(System.DateTime.Now) == -1;
    }
}