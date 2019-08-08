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
        private IConfiguration _configuration { get; set; }
        public MasterDataConversionController(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        //POST: api/MasterDataConversion/Import/
        [HttpPost]
        public IActionResult Import([FromForm] InputFileProperties inputFileProperties)
        {
            var service = new MasterDataConversionService(new MasterDataPsTool(), _configuration);
            service.ProcessHttpRequest(inputFileProperties);
            return Ok();
        }
    }
}
