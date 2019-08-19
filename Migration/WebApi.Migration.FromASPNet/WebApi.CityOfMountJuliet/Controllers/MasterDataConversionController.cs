using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using WebApi.CommonCore.Models;
using WebApi.CityOfMountJuliet.Services.MasterData;
using WebApi.CityOfMountJuliet.Models.Library;
using Microsoft.Extensions.Configuration;

namespace WebApi.CityOfMountJuliet.Controllers
{
    [Route("api/MasterDataConversion/Import")]
    [ApiController]
    public class MasterDataConversionController : ControllerBase
    {
        //POST: api/MasterDataConversion/Import/
        [HttpPost]
        public MessageResponse Import(InputFileProperties inputFileProperties, IConfiguration configuration)
        {
            var service = new MasterDataConversionService(new MasterDataPsTool(), configuration);
            return service.ProcessHttpRequest(inputFileProperties);
        }
    }
}
