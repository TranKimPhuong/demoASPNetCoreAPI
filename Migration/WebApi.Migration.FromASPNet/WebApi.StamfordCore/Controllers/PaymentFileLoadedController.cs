using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Web.Http;
using WebApi.CommonCore.Helper;
using WebApi.StamfordCore.Models.Data;
using WebApi.StamfordCore.Services;

namespace WebApi.Stamford.Controllers
{
    public class WebHookRequest
    {
        public string siteName { get; set; }
        public int fileId { get; set; }
        public override string ToString()
        {
            return "Request: " + Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    [AllowAnonymous]
    public class PaymentFileLoadedController : ControllerBase
    {
        static ILog Logger = LogManager.GetLogger("Stamford PaymentFileLoaded web hook", typeof(PaymentFileLoadedController));
        [AllowAnonymous]
        public IActionResult Index([FromQuery]string access_token, [FromBody]WebHookRequest request)
        {
            var token = ConfigHelper.GetString("PaymentFileLoaded.access_token");
            if (token.ToLowerInvariant() != access_token.ToLowerInvariant())
                return BadRequest();

            try
            {
                Logger.Info(request.ToString());
                var dao = DbServiceFactory.GetCurrent();
                if (dao == null)
                    throw new NullReferenceException("Can not get database connection");

                var customExporter = new CustomDistributeAndExportPayment();
                customExporter.Process(request.fileId, request.siteName);

                Logger.Info("Execute successful.");
                return Ok("successful");
            }
            catch (Exception ex)
            {
                Logger.Info("Execute failed.");
                Logger.Error(ex);
                throw;
            }
        }
    }
}
