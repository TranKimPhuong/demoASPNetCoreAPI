using System;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApi.CityOfMountJuliet.Models.Data.Provider;
using WebApi.CityOfMountJuliet.Models.Library;
using WebApi.CommonCore.Helper;
using WebApi.CommonCore.KeyVault;
using WebApi.CommonCore.Models;

namespace WebApi.CityOfMountJuliet.Services.MasterData
{
    internal class MasterDataConversionService
    {
        static ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PsTool _psTool;
        private IConfiguration _configuration { get; set; }

        internal MasterDataConversionService(PsTool psTool, IConfiguration configuration)
        {
            _psTool = psTool;
            _configuration = configuration;
        }
        internal ActionResult<string> ProcessHttpRequest(InputFileProperties inputFile)
        {
            try
            {
                if (inputFile == null)
                {
                    return "File Not Found";
                }

                var inputContent = GetInputFileContentAndLogRequest(inputFile);

                var sb = _psTool.ProcessDataFile(inputContent);

                var errorList = _psTool.GetErrors();
                if (errorList.Any())
                    return errorList.ToString();

                var encryptedData = AESHelper.EncryptAES(sb.ToString(), _configuration["AESKeyBLOB"]);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return e.Message;
            }

            return "OK";
        }
        private byte[] GetInputFileContentAndLogRequest(InputFileProperties inputFile)
        {
            // Show all the key-value pairs.
            Logger.Info("CaseAction: " + inputFile.CaseAction);
            Logger.Info("VendorCreate[]: " + inputFile.VendorCreate);
            Logger.Info("AddressCreate[]: " + inputFile.AddressCreate);
            Logger.Info("VendorUpdate[]: " + inputFile.VendorUpdate);
            Logger.Info("AddressUpdate[]: " + inputFile.AddressUpdate);
            Logger.Info("containerName: " + inputFile.ContainerName);
            Logger.Info("blobName: " + inputFile.BlobName);
            Logger.Info("decrypt: " + inputFile.Decrypt);
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
            byte[] fileContent = null;

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileContent = ms.ToArray();
            }

            return AESHelper.DescryptAES(fileContent, decryptKey);
        }
    }
}
