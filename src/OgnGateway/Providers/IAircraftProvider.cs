namespace OgnGateway.Providers;

public interface IAircraftProvider
{
    Aircraft Load(string aircraftId);
}