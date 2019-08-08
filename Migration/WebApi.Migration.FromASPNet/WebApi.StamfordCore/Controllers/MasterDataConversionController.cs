using log4net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using WebApi.CommonCore.Helper;
using WebApi.CommonCore.KeyVault;
using WebApi.CommonCore.Models;
using WebApi.StamfordCore.Services;

namespace WebApi.StamfordCore.Controllers
{
    [ApiController]
    public class MasterDataConversionController : ControllerBase
    {
        static ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        [HttpPost]
        public async Task<MessageResponse> Index()
        {
            MessageResponse res = new MessageResponse();
            try
            {
                Logger.Info("Initial execute import vendor master data");

                if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                {
                    return MessageResponse.info("Invalid media type - multipart");
                }

                string root = Environment.GetEnvironmentVariable("TEMP");

              
                var provider = new MultipartFormDataStreamProvider(root);

                await Request.ReadAsMultipartAsync(provider);

                string sitename = string.Empty;
                int CaseAction = 0;
                ArrayList DataSetCreateVendor = null;
                ArrayList DataSetCreateAddress = null;
                ArrayList DataSetUpdateVendor = null;
                ArrayList DataSetUpdateAddress = null;
                string containerName = string.Empty;
                string blobName = string.Empty;
                bool decrypt = false;
                bool isGetFromBlob = false;
                // Show all the key-value pairs.
                foreach (var key in provider.FormData.AllKeys)
                {
                    foreach (var val in provider.FormData.GetValues(key))
                    {
                        switch (key)
                        {
                            case "CaseAction":
                                CaseAction = Int32.Parse(val.ToString());
                                Logger.Info("CaseAction : " + val.ToString());
                                break;
                            case "VendorCreate":
                                DataSetCreateVendor = new ArrayList(val.ToString().Split(','));
                                DataSetCreateVendor.Add("Line");
                                Logger.Info("VendorCreate[] : " + val.ToString());
                                break;
                            case "AddressCreate":
                                DataSetCreateAddress = new ArrayList(val.ToString().Split(','));
                                DataSetCreateAddress.Add("Line");
                                DataSetCreateAddress.Add("VendorKey");
                                Logger.Info("AddressCreate[] : " + val.ToString());
                                break;
                            case "VendorUpdate":
                                DataSetUpdateVendor = new ArrayList(val.ToString().Split(','));
                                DataSetUpdateVendor.Add("Line");
                                Logger.Info("VendorUpdate[] : " + val.ToString());
                                break;
                            case "AddressUpdate":
                                DataSetUpdateAddress = new ArrayList(val.ToString().Split(','));
                                DataSetUpdateAddress.Add("Line");
                                DataSetUpdateAddress.Add("VendorKey");
                                Logger.Info("AddressUpdate[] : " + val.ToString());
                                break;
                            case "ContainerName":
                                containerName = val.ToString();
                                Logger.Info("containerName : " + val.ToString());
                                break;
                            case "BlobName":
                                blobName = val.ToString();
                                Logger.Info("blobName : " + val.ToString());
                                break;
                            case "Decrypt":
                                decrypt = bool.Parse(val.ToString());
                                Logger.Info("decrypt : " + val.ToString());
                                break;
                            case "isGetFromBlob":
                                isGetFromBlob = bool.Parse(val.ToString());
                                Logger.Info("isGetFromBlob : " + val.ToString());
                                break;
                        }

                    }
                }
                ObjParameter result = GetByteFile(provider, isGetFromBlob, containerName, blobName);
                Logger.Info("End parse parameter from request");


                ExcelService excel = new ExcelService(result.byteArr,result.filename);

                res = excel.ReadDataFromExcel();
                Logger.Info("Done execute import vendor master data");
              

                return res;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                Logger.Error("Error : " + e.ToString());
                res.code = (int)HttpStatusCode.InternalServerError;
                res.messages.Add(AppHelper.GetErrorMessage(e));
                return res;
            }
        }
      
        private ObjParameter GetByteFile(MultipartFormDataStreamProvider provider, bool isGetFromBlob, string containerName, string blobName)
        {

            ObjParameter package = new ObjParameter();  
            string path = string.Empty;
            string StandardFilePath = Path.GetRandomFileName();

            if (!isGetFromBlob)
            {
                // This illustrates how to get the file names.
                foreach (MultipartFileData file in provider.FileData)
                {                  
                    path = file.LocalFileName;
                    package.filename = file.Headers.ContentDisposition.FileName;
                }
             
                byte[] FileDecrypted = AESHelper.DescryptAES(System.IO.File.ReadAllBytes(path), Vault.Current.AESKeyBLOB);
                package.byteArr = FileDecrypted;

            }
            else
            {
                byte[] _ByteArray=BlobHelper.DownloadFileToArrayByte(Vault.Current.StorageConnectionString,containerName, blobName, Vault.Current.AESKeyBLOB);
                package.byteArr = _ByteArray;
                package.filename = blobName;

            }
            return package;
        }
 
        internal class ObjParameter
        {
            public byte[] byteArr { get; set; }
            public string filename { get; set; }
        }
    }
}
