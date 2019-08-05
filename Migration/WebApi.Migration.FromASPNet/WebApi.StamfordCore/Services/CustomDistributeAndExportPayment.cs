using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using WebApi.CommonCore.Helper;
using WebApi.StamfordCore.Models.Data;
using WebApi.CommonCore.KeyVault;
using Microsoft.Azure.ServiceBus;

namespace WebApi.StamfordCore.Services
{
    internal class CustomDistributeAndExportPayment
    {
        ILog Logger = LogManager.GetLogger("CustomDistributeAndExportPayment", typeof(CustomDistributeAndExportPayment));

        const int MAX_DETAIL_PER_PAGE = 40;
        const int PAYMENT_MAX_LINE = 51;
        public void Process(int fileId, string siteName)
        {
            try
            {
                Logger.Info("begin CustomDistributeAndExportPayment process");
                var dao = DbServiceFactory.GetCurrent();
                var parameters = new Dictionary<string, object>();
                parameters.Add("@I_vFileId", fileId);
                var list = dao.ProcForListObject<PaymentData>("[custom].usp_s_GetPaymentByFileID", parameters).ToList();

                var grouped = list.GroupBy(s => new
                {
                    TransactionId = s.TransactionId,
                    PaymentNumber = s.PaymentNumber,
                    PaymentMethod = s.PaymentMethod,
                    PaymentDate = s.PaymentDate,
                    PaymentAmount = s.PaymentAmount,
                    VendorID = s.VendorID,
                    VendorName = s.VendorName,
                    PayeeAddressLine1 = s.PayeeAddressLine1,
                    PayeeAddressLine2 = s.PayeeAddressLine2,
                    PayeeAddressLine3 = s.PayeeAddressLine3,
                    PayeeAddressLine4 = s.PayeeAddressLine4,
                    PaymentBatchId = s.PaymentBatchId,
                }).Select(s => new Payment
                {
                    Details = s.ToList(),
                    Header = new PaymentHeader
                    {
                        TransactionId = s.Key.TransactionId,
                        PaymentNumber = s.Key.PaymentNumber,
                        PaymentMethod = s.Key.PaymentMethod,
                        PaymentDate = s.Key.PaymentDate,
                        PaymentAmount = s.Key.PaymentAmount,
                        VendorID = s.Key.VendorID,
                        VendorName = s.Key.VendorName,
                        PayeeAddressLine1 = s.Key.PayeeAddressLine1,
                        PayeeAddressLine2 = s.Key.PayeeAddressLine2,
                        PayeeAddressLine3 = s.Key.PayeeAddressLine3,
                        PayeeAddressLine4 = s.Key.PayeeAddressLine4,
                        PaymentBatchId = s.Key.PaymentBatchId,
                    }
                }).OrderBy(s => s.Header.VendorName);
                if (list.Any())
                {
                    var batchId = list.First(s => s.PaymentBatchId.HasValue).PaymentBatchId;
                    var exportTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var checkFileOutputName = $"CHECK_{exportTime}_{batchId}.txt";
                    var eftFileOutputName = $"EFT_{exportTime}_{batchId}.txt";
                    //gennerate check file
                    var checkPaymentList = grouped.Where(s => s.Header.PaymentMethod == 0).ToList();
                    Logger.Info($"check payment count: {checkPaymentList.Count})");

                    StringBuilder checkFileContent = GenerateCheckFileContent(checkPaymentList);

                    //gennerate eft file
                    var eftPaymentList = grouped.Where(s => s.Header.PaymentMethod != 0).ToList();

                    Logger.Info("eft payment count: " + eftPaymentList.Count);
                    StringBuilder eftFileContent = GenerateEftFileContent(eftPaymentList);

                    //upload to blob
                    if (checkFileContent.Length > 0)
                    {
                        UploadFileToBlobStorage(checkFileOutputName, checkFileContent, siteName);
                        //send msb
                        SendMessageToMsgBus(checkFileOutputName, siteName);
                    }
                    else
                        Logger.Info("No file is exported because there is no CHECK payment records");

                    if (eftFileContent.Length > 0)
                    {
                        UploadFileToBlobStorage(eftFileOutputName, eftFileContent, siteName);
                        SendMessageToMsgBus(eftFileOutputName, siteName);
                    }
                    else
                        Logger.Info("No file is exported because there is no EFT payment records");

                    //delete CHECK
                    if (checkPaymentList.Any())
                    {
                        parameters = new Dictionary<string, object>();
                        parameters.Add("@I_vBatchId", batchId);
                        Logger.Info($"call [custom].[usp_d_DeleteCheckPaymentFromBatch] with batchId = {batchId}");
                        dao.ProcForScalar("[custom].[usp_d_DeleteCheckPaymentFromBatch]", parameters);
                    }
                }
                else
                {
                    Logger.Info("No batch created with file: " + fileId);
                }
                Logger.Info("end CustomDistributeAndExportPayment process");
            }
            catch (Exception e)
            {
                Logger.Error("error:", e);
            }
        }

        private void UploadFileToBlobStorage(string blobName, StringBuilder fileContent, string siteName)
        {
            var storageConn = Vault.Current.StorageConnectionString;

            var containerName = this.GetContainerName(siteName);
            Logger.Info($"Export to container: [{containerName}], blob name: [{blobName}]");

            BlobHelper.UploadFile(storageConn, containerName, blobName, fileContent);

        }
        private void SendMessageToMsgBus(string blobName,string siteName)
        {
            var serviceConn = Vault.Current.ServiceBusConnectionString;
            var msg = new Message();
            msg.UserProperties["BlobName"] = blobName;
            var queueName = GetQueueName(siteName);
            Logger.Info($"Begin send msg to queue name: [{queueName}]");
            MessageHelper.SendMessageToServiceBus(serviceConn, queueName, msg);
            Logger.Info($"Send msg to queue name: [{queueName}] successfully");
        }

        private string GetQueueName(string siteName)
        {
            return Vault.Current.CreateCustomExportQueue(siteName);
        }

        private string GetContainerName(string siteName)
        {
            return Vault.Current.CreateCustomExportBlobContainer(siteName);
        }

        private StringBuilder GenerateEftFileContent(List<Payment> list)
        {
            return GeneratePaymentFileContent(list);
        }

        private StringBuilder GenerateCheckFileContent(List<Payment> list)
        {
            return GeneratePaymentFileContent(list);
        }
        private StringBuilder GeneratePaymentFileContent(List<Payment> list)
        {
            var builder = new StringBuilder();
            foreach (var payment in list)
            {
                var detailCount = payment.Details.Count();
                if (detailCount <= 40)
                {
                    BuildFileContentWithoutOv(builder, payment, detailCount);
                }
                else
                {
                    BuildFileContentWithOv(builder, payment, detailCount);
                }

            }
            return builder;
        }

        private void BuildFileContentWithOv(StringBuilder builder, Payment payment, int detailCount)
        {
            int pageCount = detailCount / MAX_DETAIL_PER_PAGE;
            for (int pageIndex = 0; pageIndex <= pageCount; pageIndex++)
            {
                BuildPaymentHeader(builder, payment);
                foreach (var detail in payment.Details.Skip(pageIndex * MAX_DETAIL_PER_PAGE).Take(MAX_DETAIL_PER_PAGE))
                {

                    BuildPaymentDetailLine(builder, detail);
                }
                if (pageIndex < pageCount)
                {
                    builder.AppendLine("     OVERFLOW");
                }
                else
                {
                    for (int i = 10; i < PAYMENT_MAX_LINE - detailCount + MAX_DETAIL_PER_PAGE * pageIndex; i++)
                    {
                        builder.AppendLine();
                    }
                }
            }


        }
        string padding = new string(' ', 5);
        private void BuildFileContentWithoutOv(StringBuilder builder, Payment payment, int detailCount)
        {

            BuildPaymentHeader(builder, payment);
            foreach (var detail in payment.Details)
            {
                BuildPaymentDetailLine(builder, detail);
            }
            for (int i = 10; i < PAYMENT_MAX_LINE - detailCount; i++)
            {
                builder.AppendLine();
            }
        }
        private void BuildPaymentHeader(StringBuilder builder, Payment payment)
        {
            var paymentMethodText = payment.Header.PaymentMethod == 0 ? "CHECK" :
                                payment.Header.PaymentMethod == 1 ? "ACH" :
                                payment.Header.PaymentMethod == 2 ? "VCARD" : string.Empty;
            builder.Append("1".PadRight(5))
                   .Append(payment.Header.PaymentNumber?.Truncate(6))
                   .Append(" ")
                   .Append(paymentMethodText.Truncate(5))
                   .AppendLine();

            builder.Append(padding).Append(payment.Header.PaymentDate?.ToString("MM/dd/yy")).AppendLine();
            builder.Append(padding).Append("$" + payment.Header.PaymentAmount.ToString("0.00")).AppendLine();
            builder.Append(padding).Append(payment.Header.VendorID?.Truncate(10)).AppendLine();
            builder.Append(padding).Append(payment.Header.VendorName?.Truncate(40)).AppendLine();

            //sort address
            var listAdd = new List<string>();
            listAdd.Add(payment.Header.PayeeAddressLine1?.Truncate(40)?.Trim());
            listAdd.Add(payment.Header.PayeeAddressLine2?.Truncate(40)?.Trim());
            listAdd.Add(payment.Header.PayeeAddressLine3?.Truncate(40)?.Trim());
            listAdd.Add(payment.Header.PayeeAddressLine4?.Truncate(40)?.Trim());
            listAdd = listAdd.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var currentAddressCount = listAdd.Count;
            for (int i = 0; i < 4 - currentAddressCount; i++)
            {
                listAdd.Add(string.Empty);
            }
            foreach (var address in listAdd)
            {
                builder.Append(padding).Append(address).AppendLine();
            }
            builder.AppendLine();
        }
        private void BuildPaymentDetailLine(StringBuilder builder, PaymentData detail)
        {
            builder.Append(padding).Append(detail.Code.Truncate(14).PadRight(15))
                   .Append(detail.PurchaseOrder.Truncate(6).PadRight(7))
                   .Append(detail.InvoiceNumber.Truncate(22).PadRight(23))
                   .Append("" + detail.NetAmount.ToString("0.00").Truncate(15).PadRight(15))
                   .Append(detail.InvoiceDate?.ToString("MMddyy").Truncate(6).PadRight(7) ?? new string(' ', 7))
                   .Append(detail.APPYR.PadRight(3))
                   .AppendLine();
        }

        class PaymentData
        {
            public long TransactionId { get; set; }
            public string PaymentNumber { get; set; }
            public decimal PaymentMethod { get; set; }
            public DateTime? PaymentDate { get; set; }
            public decimal PaymentAmount { get; set; }
            public string VendorID { get; set; }
            public string VendorName { get; set; }
            public string PayeeAddressLine1 { get; set; }
            public string PayeeAddressLine2 { get; set; }
            public string PayeeAddressLine3 { get; set; }
            public string PayeeAddressLine4 { get; set; }
            public int? PaymentBatchId { get; set; }
            public string InvoiceNumber { get; set; }
            public string Code { get; set; }
            public string PurchaseOrder { get; set; }
            public decimal NetAmount { get; set; }
            public DateTime? InvoiceDate { get; set; }
            public string APPYR { get; set; }
        }
        class Payment
        {
            public PaymentHeader Header { get; set; }
            public List<PaymentData> Details { get; set; }
        }
        class PaymentHeader
        {
            public long TransactionId { get; set; }
            public string PaymentNumber { get; set; }
            public decimal PaymentMethod { get; set; }
            public DateTime? PaymentDate { get; set; }
            public decimal PaymentAmount { get; set; }
            public string VendorID { get; set; }
            public string VendorName { get; set; }
            public string PayeeAddressLine1 { get; set; }
            public string PayeeAddressLine2 { get; set; }
            public string PayeeAddressLine3 { get; set; }
            public string PayeeAddressLine4 { get; set; }
            public int? PaymentBatchId { get; set; }
        }
    }
}