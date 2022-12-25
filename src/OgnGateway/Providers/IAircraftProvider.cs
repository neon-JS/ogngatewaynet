using System.Threading.Tasks;
using OgnGateway.Dtos;

namespace OgnGateway.Providers;

public interface IAircraftProvider
{
    Task InitializeAsync();
    Aircraft Load(string aircraftId);
}