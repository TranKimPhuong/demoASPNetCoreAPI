using System;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Text;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebApi.CityOfMountJuliet.Models.Data.Provider;
using WebApi.CityOfMountJuliet.Models.Library;
using WebApi.CommonCore.Helper;
using WebApi.CommonCore.Models;

namespace WebApi.CityOfMountJuliet.Services.MasterData
{
    internal class MasterDataConversionService
    {
        static ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PsTool _psTool;
        private readonly IConfiguration _configuration;

        internal MasterDataConversionService(PsTool psTool, IConfiguration configuration)
        {
            _psTool = psTool;
            _configuration = configuration;
        }
        internal MessageResponse ProcessHttpRequest(InputFileProperties inputFile)
        {
            try
            {
                if (inputFile == null)
                {
                    return MessageResponse.error("File Not Found");
                }

                var inputContent = GetInputFileContentAndLogRequest(inputFile);

                var sb = _psTool.ProcessDataFile(inputContent);

                var errorList = _psTool.GetErrors();
                if (errorList.Any())
                    return MessageResponse.error(errorList);

                var encryptedData = AESHelper.EncryptAES(sb.ToString(), _configuration["AESKeyBLOB"]);
                return MessageResponse.ok(encryptedData);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return MessageResponse.error(e.Message);
            }
        }
        private byte[] GetInputFileContentAndLogRequest(InputFileProperties inputFile)
        {
            // Show all the key-value pairs.
            if (!string.IsNullOrEmpty(inputFile.CaseAction))
                Logger.Info("CaseAction: " + inputFile.CaseAction);
            //if (!string.IsNullOrEmpty(inputFile.VendorCreate))
            //    Logger.Info("VendorCreate[]: " + inputFile.VendorCreate);
            //if (!string.IsNullOrEmpty(inputFile.AddressCreate))
            //    Logger.Info("AddressCreate[]: " + inputFile.AddressCreate);
            //if (!string.IsNullOrEmpty(inputFile.VendorUpdate))
            //    Logger.Info("VendorUpdate[]: " + inputFile.VendorUpdate);
            //if (!string.IsNullOrEmpty(inputFile.AddressUpdate))
            //    Logger.Info("AddressUpdate[]: " + inputFile.AddressUpdate);
            if (!string.IsNullOrEmpty(inputFile.ContainerName))
                Logger.Info("containerName: " + inputFile.ContainerName);
            if (!string.IsNullOrEmpty(inputFile.BlobName))
                Logger.Info("blobName: " + inputFile.BlobName);
            if (!string.IsNullOrEmpty(inputFile.Decrypt))
                Logger.Info("decrypt: " + inputFile.Decrypt);
            if (!string.IsNullOrEmpty(inputFile.SiteName))
                Logger.Info("SiteName: " + inputFile.SiteName);
            var isGetFromBlob = string.IsNullOrEmpty(inputFile.isGetFromBlob) ? false : bool.Parse(inputFile.isGetFromBlob);
            Logger.Info("isGetFromBlob : " + inputFile.isGetFromBlob);

            var byteArr = GetByteFile(inputFile.File, isGetFromBlob, inputFile.ContainerName, inputFile.BlobName);

            return byteArr;
        }

        private byte[] GetByteFile(IFormFile file, bool isGetFromBlob, string containerName, string blobName)
        {
            var decryptKey = _configuration["AESKeyBLOB"];

            if (isGetFromBlob)
            {
                var storageAccountNameShared = _configuration["StorageAccountNameShared"];
                var storageAccountKeyShared = _configuration["StorageAccountKeyShared"];
                var aESSecretKey = _configuration["AESSecretKey"];
                var storageSharedConnectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccountNameShared};AccountKey={AESHelper.DescryptAES(storageAccountKeyShared, aESSecretKey)}";

                return BlobHelper.DownloadFileToArrayByte(storageSharedConnectionString, containerName, blobName, decryptKey);
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            byte[] fileContent = null;

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);             
                fileContent = ms.ToArray();
            }

            //Issue: ko decrypt đc, input file ko phải complete block 
            return AESHelper.DescryptAES(fileContent, decryptKey);
            //return fileContent;
        }
    }
}
