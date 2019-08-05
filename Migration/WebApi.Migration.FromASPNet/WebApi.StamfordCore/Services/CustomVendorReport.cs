using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebApi.CommonCore.Emails;
using WebApi.CommonCore.Helper;
using WebApi.StamfordCore.Models.Data;

namespace WebApi.Stamford.Services
{
    public class CustomVendorReport
    {
        static ILog LOGGER = LogManager.GetLogger("Stamford custom Vendor Report", typeof(CustomVendorReport));
        public void GetVendorsAndSendEmail()
        {
            var dao = DbServiceFactory.GetCurrent();
            if (dao == null)
            {
                throw new Exception("Can not get data connection");
            }
            var list = dao.ProcForListObject<VendorData>("[custom].usp_s_GetAllVendorWithPaymentMethod").ToList();
            LOGGER.Info($"Total {list.Count} vendors");
            if (list.Any())
            {
                StringBuilder builder = GenerateReportFileContent(list);
                SendEmail(builder);
            }
        }

        private void SendEmail(StringBuilder builder)
        {
            var mailMsg = CreateEmailItem(builder);
             EmailSender.Current.Send(mailMsg);
        }

        private EmailItem CreateEmailItem(StringBuilder builder)
        {
            string[] emailTo = GetEmailAddresses();
            var mailMsg = new EmailItem
            {
                From = "",
                Subject = "Vendor Listing",
                Content = "See attached report.",
                HtmlFormat = true,
            };
            mailMsg.AddTo(emailTo);
            byte[] fileInput = Encoding.UTF8.GetBytes(builder.ToString());

            mailMsg.AddAttachment(fileInput, "VendorListing.csv");
            return mailMsg;
        }

        private string[] GetEmailAddresses()
        {
            return ConfigHelper.GetString("custom.VendorReport.EmailTo").Split(new[] { '|' }, options: StringSplitOptions.RemoveEmptyEntries);
        }

        private StringBuilder GenerateReportFileContent(List<VendorData> list)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Vendor ID,Vendor Name,Payment Method CHECK,Payment Method ACH, Payment Method VCARD");
            foreach (var vendor in list)
            {
                builder.AppendLine($"\"{vendor.VendorId}\",\"{vendor.Name}\",{vendor.PaymentMethodCHECK}, {vendor.PaymentMethodACH}, {vendor.PaymentMethodVCARD} ");
            }
            return builder;
        }

        class VendorData
        {
            public string VendorId { get; set; }
            public string Name { get; set; }
            public string PaymentMethodCHECK { get; set; }
            public string PaymentMethodACH { get; set; }
            public string PaymentMethodVCARD { get; set; }
        }
    }
}