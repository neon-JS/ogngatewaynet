using System.Threading.Tasks;
using OgnGateway.Dtos;

namespace OgnGateway.Providers
{
    public interface IAprsConfigProvider
    {
        Task<AprsConfig> LoadAsync();
    }
}