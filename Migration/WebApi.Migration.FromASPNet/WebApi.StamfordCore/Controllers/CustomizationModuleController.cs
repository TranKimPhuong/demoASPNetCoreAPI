using System;
using System.Web.Http;
using log4net;
using Microsoft.AspNetCore.Mvc;
using WebApi.CommonCore.Models;
using WebApi.Stamford.Services;
using WebApi.StamfordCore.Services;

namespace WebApi.StamfordCore.Controllers
{
    [Route("api/CustomizationModule")]
    [ApiController]
    public class CustomizationModuleController : ControllerBase
    {
        static ILog LOGGER = LogManager.GetLogger("Stamford custom Vendor Report", typeof(CustomDistributeAndExportPayment));

        [HttpPost, Route("VendorReport")]
        public MessageResponse VendorReport()
        {
            var response = new MessageResponse() { code = 200 };
            LOGGER.Info("Begin module.");
            try
            {
                var module = new CustomVendorReport();
                module.GetVendorsAndSendEmail();
            }
            catch (Exception ex)
            {
                response.code = 500;
                response.addMessage("An error has occurred.");
                LOGGER.Error(ex.Message, ex);
                return response;
            }
            response.addMessage("Successfully.");
            LOGGER.Info("Successfully.");
            return response;
        }
    }
}
