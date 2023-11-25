namespace OgnGateway.Services;

public interface IStreamConverter
{
    FlightData? ConvertData(string line);
}