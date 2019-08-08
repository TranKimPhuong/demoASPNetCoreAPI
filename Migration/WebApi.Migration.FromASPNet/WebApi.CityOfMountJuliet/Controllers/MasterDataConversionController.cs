using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using WebApi.CommonCore.Models;
using WebApi.CityOfMountJuliet.Services.MasterData;
using WebApi.CityOfMountJuliet.Models.Library;

namespace WebApi.CityOfMountJuliet.Controllers
{
    [Route("api/MasterDataConversion/Import")]
    [ApiController]
    public class MasterDataConversionController : ControllerBase
    {
        //POST: api/MasterDataConversion/Import/
        [HttpPost]
        public IActionResult Import([FromForm] InputFileProperties inputFileProperties)
        {
            var service = new MasterDataConversionService(new MasterDataPsTool());
            service.ProcessHttpRequest(inputFileProperties);
            return Ok();
        }
    }
}
