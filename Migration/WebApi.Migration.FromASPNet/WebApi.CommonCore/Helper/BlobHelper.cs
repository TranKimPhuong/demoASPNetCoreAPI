using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.CommonCore.Helper
{
    public static class BlobHelper
    {
        static ILog LOGGER = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public static byte[] DownloadFileToArrayByte(string blobConnectionString, string blobContainerName, string blobName, string decryptKey = null)
        {
            // Retrieve reference to a previously created container.
            CloudBlobContainer blobContainer = GetCloudBlobContainer(blobConnectionString, blobContainerName);
            Task<bool> IsExistBlobContainer = blobContainer.ExistsAsync();
            if (blobContainer == null || !IsExistBlobContainer.Result) throw new System.ArgumentException(string.Format("Can't find the BLOB container [{0}].", blobContainerName));
            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blobFile = blobContainer.GetBlockBlobReference(blobName);
            Task<bool> IsExistBlobFile = blobFile.ExistsAsync();
            if (blobFile == null || !IsExistBlobFile.Result) throw new System.ArgumentException(string.Format("Can't find the BLOB name [{0}].", blobName));

            return DownloadFileToArrayByte(blobFile, decryptKey);
        }

        public static CloudBlobContainer GetCloudBlobContainer(string blobConnectionString, string blobContainerName)
        {
            if (string.IsNullOrEmpty(blobConnectionString)) throw new System.ArgumentException("Blob connection string is required.");
            //Get the file from BLOB Storage 
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve reference to a previously created container.
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

            return blobContainer;
        }
        
        public static byte[] DownloadFileToArrayByte(CloudBlockBlob blobFile, string decryptKey = null)
        {
            byte[] aFile = null;
            if (blobFile != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    blobFile.DownloadToStreamAsync(ms);
                    if (ms != null)
                    {
                        aFile = ms.ToArray();
                        if (!string.IsNullOrEmpty(decryptKey)) aFile = AESHelper.DescryptAES(aFile, decryptKey);
                    }                    
                }
            }            
            return aFile;
        }

        //upload file
        public static void UploadFile(string blobConnectionString, string blobContainerName, string blobName, string blobContentFilePath, string encryptKey = null)
        {
            CloudBlobContainer blobContainer = GetCloudBlobContainer(blobConnectionString, blobContainerName);
            byte[] aBlobContent = File.ReadAllBytes(blobContentFilePath);
            //encrypt with AESKey
            if (!string.IsNullOrEmpty(encryptKey)) aBlobContent = AESHelper.EncryptAES(aBlobContent, encryptKey);
            UploadFile(blobContainer, blobName, aBlobContent);
        }

        public static void UploadFile(string blobConnectionString, string blobContainerName, string blobName, StringBuilder blobContent, string encryptKey = null)
        {
            CloudBlobContainer blobContainer = GetCloudBlobContainer(blobConnectionString, blobContainerName);
            UploadFile(blobContainer, blobName, blobContent, encryptKey);
        }
        public static void UploadFile(string blobConnectionString,string blobContainerName, string blobName, byte[] arrbytes, string encryptKey = null)
        {
            CloudBlobContainer blobContainer = GetCloudBlobContainer(blobConnectionString, blobContainerName);
            //encrypt with AESKey
            if (!string.IsNullOrEmpty(encryptKey)) arrbytes = AESHelper.EncryptAES(arrbytes, encryptKey);
            UploadFile(blobContainer, blobName, arrbytes);

        }
        public static void UploadFile(CloudBlobContainer blobContainer, string blobName, StringBuilder blobContent, string encryptKey = null)
        {
            string inputString = string.Empty;
            if (blobContent != null) inputString = blobContent.ToString();
            byte[] aBlobContent = Encoding.UTF8.GetBytes(inputString);

            //encrypt with AESKey
            if (!string.IsNullOrEmpty(encryptKey)) aBlobContent = AESHelper.EncryptAES(aBlobContent, encryptKey);

            UploadFile(blobContainer, blobName, aBlobContent);
        }
        
        public static void UploadFile(CloudBlobContainer blobContainer, string blobName, byte[] aBlobContent)
        {
            //validate parameter
            Task<bool> IsExistBlobContainer = blobContainer.ExistsAsync();
            if (blobContainer == null || !IsExistBlobContainer.Result) throw new System.ArgumentException("Blob container is required.");
            if (string.IsNullOrEmpty(blobName)) throw new System.ArgumentException("Blob name is required.");

            CloudBlockBlob blockBlobOutput = blobContainer.GetBlockBlobReference(blobName);
            using (var ms = new MemoryStream(aBlobContent, true))
            {
                blockBlobOutput.UploadFromStreamAsync(ms);
                
                LOGGER.InfoFormat("File [{0}] uploaded to the Blob container [{1}] successfully.", blobName, blobContainer.Name);
            }
        }
    }
}
