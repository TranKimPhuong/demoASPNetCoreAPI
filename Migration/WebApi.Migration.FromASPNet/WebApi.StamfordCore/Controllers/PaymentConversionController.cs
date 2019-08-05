using Microsoft.AspNetCore.Mvc;
using WebApi.CommonCore.Models;
using WebApi.StamfordCore.Services;

namespace WebApi.StamfordCore.Controllers
{
    [ApiController]
    public class PaymentConversionController : ControllerBase
    {
        // POST: api/PaymentConversion
        public MessageResponse Post([FromBody]ConversionRequest reqConversion)
        {
            ConversionService service = new ConversionService();
            return service.convert(reqConversion);
        }        
    }
}
