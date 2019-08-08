using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.CityOfMountJuliet.Models.Library;
using WebApi.CityOfMountJuliet.Services.MasterData;

namespace WebApi.CityOfMountJuliet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataConversion1Controller : ControllerBase
    {
        [HttpPost]
        [Route("Import")]
        public async Task<IActionResult> Import([FromForm] InputFileProperties inputFileProperties)
        {
            return Ok();
        }
    }
}