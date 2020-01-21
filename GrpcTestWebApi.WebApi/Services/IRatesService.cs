using System.Linq;
using System.Threading.Tasks;
using GrpcTestWebApi.Common;

namespace GrpcTestWebApi.WebApi.Services
{
    public interface IRatesService
    {
        Task<IQueryable<DtoRate>> GetRatesAsync();
    }
}
