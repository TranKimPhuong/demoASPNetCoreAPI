using WebApi.CommonCore.Models;
using Microsoft.AspNetCore.Mvc;
using WebApi.CityOfMountJuliet.Services.Payment;

namespace WebApi.CityOfMountJuliet.Controllers
{
    [Route("api/PaymentConversion")]
    [ApiController]
    public class PaymentConversionController : ControllerBase
    {
        // POST: api/PaymentConversion/Import
        [HttpPost]
        [Route("Import")]
        public MessageResponse PaymentImport([FromBody]ConversionRequest request)
        {
            PaymentConversionService service = new PaymentConversionService(new PaymentPsTool());
            return service.ProcessRequest(request);
        }

    }
}