using System;
using System.Linq;
using log4net;
using Microsoft.Extensions.Configuration;
using WebApi.CityOfMountJuliet.Models.Data.Provider;
using WebApi.CommonCore.Helper;
using WebApi.CommonCore.KeyVault;
using WebApi.CommonCore.Models;

namespace WebApi.CityOfMountJuliet.Services.Payment
{
    internal class PaymentConversionService
    {
        private readonly PsTool _psTool;
        private readonly ILog Logger;
        private readonly IConfiguration _configuration;

        internal PaymentConversionService(PsTool psTool)
        {
            _psTool = psTool;
            //_configuration = configuration;
            Logger = LogManager.GetLogger(typeof(PaymentConversionService));
        }

        internal MessageResponse ProcessRequest(ConversionRequest request)
        {
            string errorMsg;
            if (!ValidateRequest(request, out errorMsg))
                return MessageResponse.info(errorMsg);
            try
            {
                var storageConnString = _configuration["StorageConnectionString"];
                var decryptKey = _configuration["AESKeyBLOB"];

                //download file
                var downloadResult = DownloadConversionFiles(request, storageConnString, decryptKey);
                if (!downloadResult.Success)
                    return MessageResponse.info(downloadResult.ErrorMessage);

                _psTool.SetCurrentConversionRequest(request);
                var sbStandardFile = _psTool.ProcessDataFile(downloadResult.PrimaryFileContent, downloadResult.RemittanceFileContent);
                var listErrors = _psTool.GetErrors();

                if (listErrors.Any())
                    return MessageResponse.error(listErrors);

                BlobHelper.UploadFile(storageConnString, request.containerName, request.blobOutputName, sbStandardFile, decryptKey);
                return MessageResponse.ok(request.blobOutputName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return MessageResponse.error(ex.Message);
            }
        }

        bool ValidateRequest(ConversionRequest request, out string msg)
        {
            msg = string.Empty;
            if (request == null)
            {
                msg = "[blobHeaderName, blobOutputName, containerName] are required.";
                return false;
            }
            request.containerName = request.containerName?.Trim();
            request.blobHeaderName = request.blobHeaderName?.Trim();
            request.blobOutputName = request.blobOutputName?.Trim();
            if (string.IsNullOrEmpty(request.blobHeaderName))
            {
                msg = "Blob file input is required.";
                return false;
            }
            if (string.IsNullOrEmpty(request.blobOutputName))
            {
                msg = "Blob file output is required.";
                return false;
            }
            if (string.IsNullOrEmpty(request.containerName))
            {
                msg = "Blob container is required.";
                return false;
            }
            return true;
        }

        DownloadConversionFilesResult DownloadConversionFiles(ConversionRequest request, string storageConnString, string decryptKey)
        {
            if (string.IsNullOrEmpty(decryptKey))
            {
                var msg = "AESKEY to encrypt/decrypt file is required.";
                return DownloadConversionFilesResult.FailFast(msg);
            }
            try
            {
                var primaryFileContent = BlobHelper.DownloadFileToArrayByte(storageConnString, request.containerName, request.blobHeaderName, decryptKey);
                if (primaryFileContent == null)
                {
                    var msg = $"Can't find the content of the BLOB file [{request.blobHeaderName}].";
                    DownloadConversionFilesResult.FailFast(msg);
                }
                byte[] remittanceFileContent = null;
                if (!string.IsNullOrEmpty(request.blobDetailName))
                    remittanceFileContent = BlobHelper.DownloadFileToArrayByte(storageConnString, request.containerName, request.blobDetailName, decryptKey);
                return DownloadConversionFilesResult.Successful(primaryFileContent, remittanceFileContent);
            }
            catch (ApplicationException ae)
            {
                Logger.Error(ae);
                return DownloadConversionFilesResult.FailFast(ae.Message);
            }
        }

        class DownloadConversionFilesResult
        {
            private DownloadConversionFilesResult() { }
            internal bool Success => PrimaryFileContent != null && string.IsNullOrEmpty(ErrorMessage);
            internal byte[] PrimaryFileContent { get; set; } = null;
            internal byte[] RemittanceFileContent { get; set; } = null;
            internal string ErrorMessage { get; set; }
            internal static DownloadConversionFilesResult FailFast(string errorMessage)
            {
                return new DownloadConversionFilesResult { ErrorMessage = errorMessage };
            }
            internal static DownloadConversionFilesResult Successful(byte[] primaryFileContent, byte[] remittanceFileContent)
            {
                return new DownloadConversionFilesResult
                {
                    PrimaryFileContent = primaryFileContent,
                    RemittanceFileContent = remittanceFileContent
                };
            }
        }
    }
}

