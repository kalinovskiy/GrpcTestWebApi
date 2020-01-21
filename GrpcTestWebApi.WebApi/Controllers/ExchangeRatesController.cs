using System.Linq;
using System.Threading.Tasks;
using GrpcTestWebApi.Common;
using GrpcTestWebApi.WebApi.Services;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace GrpcTestWebApi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRatesController : ControllerBase
    {
        private IRatesService _ratesService;

        public ExchangeRatesController(IRatesService ratesService)
        {
            _ratesService = ratesService;
        }

        // GET api/values
        [HttpGet]
        [EnableQuery]
        public async Task<IQueryable<DtoRate>> Get()
        {
            return await _ratesService.GetRatesAsync();
        }
    }
}
