using WebApi.CommonCore.Models;
using Microsoft.AspNetCore.Mvc;
using WebApi.CityOfMountJuliet.Services.Payment;
using Microsoft.Extensions.Configuration;

namespace WebApi.CityOfMountJuliet.Controllers
{
    [Route("api/PaymentConversion")]
    [ApiController]
    public class PaymentConversionController : ControllerBase
    {
        //IConfiguration con;
        // POST: api/PaymentConversion/Import
        [HttpPost]
        [Route("Import")]
        //ko cho truyền IConfiguration configuration chổ này??????????????????????
        public MessageResponse PaymentImport([FromForm]ConversionRequest request)
        {

            PaymentConversionService service = new PaymentConversionService(new PaymentPsTool());
            return service.ProcessRequest(request);
        }

    }
}