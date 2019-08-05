using log4net;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApi.CommonCore.Helper;
using WebApi.CommonCore.KeyVault;
using WebApi.CommonCore.Models;
using WebApi.StamfordCore.Models;
using WebApi.StamfordCore.Services.Payment;

namespace WebApi.StamfordCore.Services
{
    public class ConversionService
    {
        static ILog LOGGER = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        const string SEPARATOR_CELL = ",";
        const string FIRST_CELL_HEADER = "H";
        const string FIRST_CELL_HEADER_DETAIL = "D";
        private string[] HEADER_COLUMNS = new string[] { "PaymentNumber", "PaymentDate", "PaymentAmount", "PayeeID" };
        private string[] DETAIL_COLUMNS = new string[] { "UDF1", "PurchaseOrderNumber", "UDF2", "InvoiceNumber", "NetAmount", "InvoiceDate" };
        const string ERROR_LINE_FIELD_VENDOR = "Line No:{0} is missing a value for [{1}]. This field is required for check payments.";
        //const string ERROR_LINE_FIELD_VENDOR_DUPLICATE = "Line No:{0} [Payee ID / VendorID] is existed a value for [{1}]. This field is required for check payments.";

        public List<string> ListErrors { get; set; }
        public List<string> ListErrorVendor = new List<string>();
        public List<VendorInfomation> ListVendorAddress { get; set; }
        public List<PaymentConversionLine> ListVendors = new List<PaymentConversionLine>();

        public MessageResponse convert(ConversionRequest reqValue)
        {
            LOGGER.Info("Conversion processing...");
            if (reqValue == null || string.IsNullOrEmpty(reqValue.blobHeaderName) || string.IsNullOrEmpty(reqValue.blobOutputName) || string.IsNullOrEmpty(reqValue.containerName))
            {
                return MessageResponse.info("[blobHeaderName, blobOutputName, containerName] are required.");
            }
            string BlobContainer = reqValue.containerName.Trim();
            string BlobInputName = reqValue.blobHeaderName.Trim();
            string BlobOutputName = reqValue.blobOutputName.Trim();
            string AESKey = Vault.Current.AESKeyBLOB;
            string storageConnectionString = Vault.Current.StorageConnectionString;
                

            //validate parameter
            if (string.IsNullOrEmpty(BlobContainer)) return MessageResponse.info("Blob container is required.");
            if (string.IsNullOrEmpty(BlobInputName)) return MessageResponse.info("Blob file input is required.");
            if (string.IsNullOrEmpty(BlobOutputName)) return MessageResponse.info("Blob file output is required.");
            if (string.IsNullOrEmpty(AESKey)) return MessageResponse.info("AESKEY to encrypt/decrypt file is required.");

            try
            {
                // Retrieve reference to a previously created container.
                CloudBlobContainer blobContainer = BlobHelper.GetCloudBlobContainer(storageConnectionString, BlobContainer);
                Task<bool> IsExitBlobContainer = blobContainer.ExistsAsync();
                if (blobContainer == null || !IsExitBlobContainer.Result) return MessageResponse.info(string.Format("Can't find the BLOB container [{0}].", BlobContainer));
                
                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlobInput = blobContainer.GetBlockBlobReference(BlobInputName);
                Task<bool> IsExitblockBlobInput = blockBlobInput.ExistsAsync();
                if (blockBlobInput == null || !IsExitblockBlobInput.Result) return MessageResponse.info(string.Format("Can't find the BLOB file [{0}].", BlobInputName));

                //download file
                byte[] fileInput = BlobHelper.DownloadFileToArrayByte(blockBlobInput, AESKey);
                if (fileInput == null) return MessageResponse.info(string.Format("Can't find the content of the BLOB file [{0}].", BlobInputName));

                //do conversion
                StringBuilder sbStandardFile = this.doConvert(fileInput);

                //no error: write the file standard
                if (this.ListErrorVendor == null || this.ListErrorVendor.Count == 0)
                {
                    //upload the file standard file to azure
                    BlobHelper.UploadFile(blobContainer, BlobOutputName, sbStandardFile, AESKey);

                    //return the blob output name of file saved
                    //LOGGER.Info(string.Format("File [{0}] converted to the file [{1}] and saved successfully in the BLOB container [{2}]", BlobInputName, BlobOutputName, BlobContainer));
                    return MessageResponse.ok(reqValue.blobOutputName);
                }


                //report errors if have
                if (this.ListErrors == null) this.ListErrors = new List<string>();
                if (this.ListErrorVendor.Count > 0) this.ListErrors.AddRange(this.ListErrorVendor);
                return MessageResponse.error(this.ListErrors);

            }
            catch (Exception ex)
            {
                LOGGER.Error(ex);
                throw;

            }
        }

        public StringBuilder doConvert(byte[] fileInput)
        {
            //clear the variable Errors
            this.ListErrors = new List<string>();
            this.ListErrorVendor = new List<string>();
            ListVendorAddress = new List<VendorInfomation>();
            
            //stanadard file
            StringBuilder StandardFile = new StringBuilder();
            StandardFile.AppendLine(this.GetHeader());
            StandardFile.AppendLine(this.GetHeaderDetail());

            //count line
            int CountLine = 0;
            //input string line
            string strLine = null;
            using (MemoryStream stream = new MemoryStream(fileInput))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    //IList<string> ListPaymentNumber = new List<string>();
                    IList<string> ListPayeeId = new List<string>();
                    IList<PaymentConversionLine> ListDetail = null;
                    PaymentConversionLine oHeader = null;
                    while ((strLine = reader.ReadLine()) != null)
                    {
                        //count line;
                        CountLine++;
                        //ignore line is BLANK
                        if (string.IsNullOrEmpty(strLine)) continue;

                        PaymentConversionLine oTestHeader = new PaymentConversionLine(CountLine, strLine);
                        if (oTestHeader.IsLinePaymentNumber())
                        {
                            //initialize header & detail
                            if (oHeader == null) oHeader = new PaymentConversionLine();
                            if (ListDetail == null) ListDetail = new List<PaymentConversionLine>();

                            if (oTestHeader.HasErrors) this.ListErrors.Add(oTestHeader.ErrorMessage);
                            else oHeader.PaymentNumber = oTestHeader.PaymentNumber;

                            //next line is header overflow or not
                            strLine = reader.ReadLine();
                            CountLine++;
                            PaymentConversionLine oTestLine = new PaymentConversionLine(CountLine, strLine);

                            if (oTestLine.IsLinePayeeID())
                            {
                                if (oTestLine.HasErrors) this.ListErrors.Add(oTestLine.ErrorMessage);
                                else
                                {
                                    oHeader.PayeeID = oTestLine.PayeeID;
                                    if (string.IsNullOrEmpty(oHeader.PayeeID))
                                    {
                                        this.ListErrorVendor.Add(string.Format(ERROR_LINE_FIELD_VENDOR, CountLine, "Payee ID"));
                                        oHeader.IsErrortVendor = true;
                                    }
                                }

                                //Line payment date
                                string strLinePaymentDate = reader.ReadLine();
                                CountLine++;
                                PaymentConversionLine oTestLinePaymentDatePaymentAmount = new PaymentConversionLine(CountLine, strLinePaymentDate);
                                if (oTestLinePaymentDatePaymentAmount.IsLinePaymentDatePaymentAmount())
                                {
                                    if (oTestLinePaymentDatePaymentAmount.HasErrors) this.ListErrors.Add(oTestLinePaymentDatePaymentAmount.ErrorMessage);
                                    else
                                    {
                                        oHeader.PaymentDate = oTestLinePaymentDatePaymentAmount.PaymentDate;
                                        oHeader.PaymentAmount = oTestLinePaymentDatePaymentAmount.PaymentAmount;
                                    }
                                }

                                //parse vendor address
                                this.doConvertVendor(reader, ref CountLine, ref oHeader);

                                //build standard file
                                StandardFile.AppendLine(oHeader.GetHeaderDataLine());
                                if (ListDetail != null && ListDetail.Count > 0)
                                {
                                    foreach (PaymentConversionLine oDetailData in ListDetail)
                                    {
                                        StandardFile.AppendLine(oDetailData.GetDetailDataLine());
                                    }
                                    //reset variable                                
                                    ListDetail = null;
                                }

                                if (!oHeader.IsErrortVendor)
                                {
                                    if (!string.IsNullOrEmpty(oHeader.Address1) || !string.IsNullOrEmpty(oHeader.City) || !string.IsNullOrEmpty(oHeader.StateProvince) || !string.IsNullOrEmpty(oHeader.PostalCode))
                                    {
                                        this.ListVendorAddress.Add(oHeader.ToVendorAddress);
                                    }
                                }
                                ListVendors.Add(oHeader);
                                oHeader = null;
                            }
                            else if (oTestLine.IsLineOverflow())
                            {
                                this.doConvertDetail(reader, ref CountLine, ref ListDetail, ref oHeader, ref StandardFile);
                            }
                            else if (oTestLine.IsLineDetail())
                            {
                                if (oTestLine.HasErrors) this.ListErrors.Add(oTestLine.ErrorMessage);
                                else ListDetail.Add(oTestLine);
                                this.doConvertDetail(reader, ref CountLine, ref ListDetail, ref oHeader, ref StandardFile);
                            } //if (oLineTest.IsLineOverflow())
                        }//if (oTestHeader.IsLinePaymentNumber() && !ListPaymentNumber.Contains(oTestHeader.PaymentNumber))

                        //TODO List Vendor Address in DB

                    } //loop while
                   
                }
            }

            #region add/update vendor from payment file
            //update vendor
            var error = string.Empty;
            var module = new CreateVendorsFromPayments(ListVendors);
            if (!module.ProcessCreateVendors(out error))
            {
                throw new Exception(error);
            }
            #endregion

            #region Added logic for Adding Vendor Address information from Data File for Check payments.
            //LOGGER.Info("Begin: Logic for Adding Vendor Address information from Data File for Check payments....");
            //DataSet dsVendor = new DataSet();
            //int BatchSize = 500;
            //int j = 0;
            //DataTable newdbVendor = ListVendorAddress.AsDataTable();

            //#region validate
            //List<string> lstErr = ValidateLogiCreateVendor(ref newdbVendor);
            //#endregion
            //if (newdbVendor.Rows.Count > 0)
            //{
            //    DataTable new_tableSmallVendor = newdbVendor.Clone();
            //    if (newdbVendor.Rows.Count <= BatchSize)
            //    {

            //        new_tableSmallVendor = newdbVendor.Copy();
            //        dsVendor.Tables.Add(new_tableSmallVendor);

            //    }
            //    else
            //    {
            //        for (int i = 0; i < newdbVendor.Rows.Count; i++)
            //        {
            //            new_tableSmallVendor.NewRow();
            //            new_tableSmallVendor.ImportRow(newdbVendor.Rows[i]);
            //            if ((i + 1) == newdbVendor.Rows.Count)
            //            {

            //                dsVendor.Tables.Add(new_tableSmallVendor.Copy());
            //                new_tableSmallVendor.Rows.Clear();
            //            }
            //            else if (++j == BatchSize)
            //            {
            //                dsVendor.Tables.Add(new_tableSmallVendor.Copy());
            //                new_tableSmallVendor.Rows.Clear();
            //                j = 0;
            //            }
            //        }
            //    }


            //    string ResultReturn = string.Empty;
            //    Parallel.For(0, dsVendor.Tables.Count, index =>
            //    {
            //        ResultReturn = CreateVendor(dsVendor.Tables[index]);
            //        if (ResultReturn.Length > 0)
            //        {
            //            lstErr.Add(ResultReturn);
            //        }
            //    });
            //    LOGGER.Info("End: Logic for Adding Vendor Address information from Data File for Check payments....");
            //}
            #endregion

            return StandardFile;
        }

        private void doConvertVendor(StreamReader reader, ref int CountLine, ref PaymentConversionLine oHeader)
        {
            //Line payee name
            string strLinePayeeName = reader.ReadLine();
            CountLine++;
            PaymentConversionLine oTestLinePayeeName = new PaymentConversionLine(CountLine, strLinePayeeName);
            if (oTestLinePayeeName.IsLinePayeeName())
            {
                if (oTestLinePayeeName.HasErrors) this.ListErrors.Add(oTestLinePayeeName.ErrorMessage);
                else
                {
                    oHeader.PayeeName = oTestLinePayeeName.PayeeName;
                    if (string.IsNullOrEmpty(oHeader.PayeeName))
                    {
                        this.ListErrorVendor.Add(string.Format(ERROR_LINE_FIELD_VENDOR, CountLine, "Payee Name"));
                        oHeader.IsErrortVendor = true;
                    }
                }
            }

            //loop max 3 times to parse vendor address
            string strLineAddress = string.Empty;
            int loopMax = 3;
            int loopIndex = 0;
            bool IsAddress1 = false;
            bool IsAddress2 = false;
            while ((strLineAddress = reader.ReadLine()) != null && loopIndex < loopMax)
            {
                loopIndex++;
                //Line payee address1
                CountLine++;
                PaymentConversionLine oTestLineAddress = new PaymentConversionLine(CountLine, strLineAddress);
                if (oTestLineAddress.IsLineCityStateZip() && (IsAddress1 == true || IsAddress2 == true))
                {
                    if (oTestLineAddress.HasErrors) this.ListErrors.Add(oTestLineAddress.ErrorMessage);
                    else
                    {
                        oHeader.City = oTestLineAddress.City;
                        oHeader.StateProvince = oTestLineAddress.StateProvince;
                        oHeader.PostalCode = oTestLineAddress.PostalCode;

                        if (string.IsNullOrEmpty(oHeader.City))
                        {
                            this.ListErrorVendor.Add(string.Format(ERROR_LINE_FIELD_VENDOR, CountLine, "City"));
                            oHeader.IsErrortVendor = true;
                        }
                        if (string.IsNullOrEmpty(oHeader.StateProvince))
                        {
                            this.ListErrorVendor.Add(string.Format(ERROR_LINE_FIELD_VENDOR, CountLine, "State"));
                            oHeader.IsErrortVendor = true;
                        }
                        if (string.IsNullOrEmpty(oHeader.PostalCode))
                        {
                            this.ListErrorVendor.Add(string.Format(ERROR_LINE_FIELD_VENDOR, CountLine, "Postal Code"));
                            oHeader.IsErrortVendor = true;
                        }
                    }
                    break;//stop to parse vendor address
                }
                else
                {
                    if (oTestLineAddress.IsLineAddress1() && IsAddress1 == false)
                    {
                        IsAddress1 = true;
                        if (oTestLineAddress.HasErrors) this.ListErrors.Add(oTestLineAddress.ErrorMessage);
                        else
                        {
                            oHeader.Address1 = oTestLineAddress.Address1;
                            if (string.IsNullOrEmpty(oHeader.Address1))
                            {
                                this.ListErrorVendor.Add(string.Format(ERROR_LINE_FIELD_VENDOR, CountLine, "Payee Address1"));
                                oHeader.IsErrortVendor = true;
                            }
                        }
                    }
                    else if (oTestLineAddress.IsLineAddress2() && IsAddress2 == false)
                    {
                        IsAddress2 = true;
                        if (oTestLineAddress.HasErrors) this.ListErrors.Add(oTestLineAddress.ErrorMessage);
                        else
                        {
                            oHeader.Address2 = oTestLineAddress.Address2;
                        }
                    }
                }
            } //while vendor address
        }

        private void doConvertDetail(StreamReader reader, ref int CountLine,
                                     ref IList<PaymentConversionLine> ListDetail, ref PaymentConversionLine oHeader,
                                     ref StringBuilder StandardFile)
        {
            string strLine = null;
            //parse next line detail
            while ((strLine = reader.ReadLine()) != null)
            {
                CountLine++;
                //ignore line is BLANK
                if (string.IsNullOrEmpty(strLine)) continue;

                PaymentConversionLine oTestLineDetail = new PaymentConversionLine(CountLine, strLine);
                if (oTestLineDetail.IsLineDetail())
                {
                    if (oTestLineDetail.HasErrors) this.ListErrors.Add(oTestLineDetail.ErrorMessage);
                    else ListDetail.Add(oTestLineDetail);
                }
                else if (oTestLineDetail.IsLinePayeeID())
                {
                    if (oTestLineDetail.HasErrors) this.ListErrors.Add(oTestLineDetail.ErrorMessage);
                    else
                    {
                        oHeader.PayeeID = oTestLineDetail.PayeeID;
                        if (string.IsNullOrEmpty(oHeader.PayeeID))
                        {
                            this.ListErrorVendor.Add(string.Format(ERROR_LINE_FIELD_VENDOR, CountLine, "Payee ID"));
                            oHeader.IsErrortVendor = true;
                        }
                    }

                    //Line payment date
                    string strLinePaymentDate = reader.ReadLine();
                    CountLine++;
                    PaymentConversionLine oTestLinePaymentDatePaymentAmount = new PaymentConversionLine(CountLine, strLinePaymentDate);
                    if (oTestLinePaymentDatePaymentAmount.IsLinePaymentDatePaymentAmount())
                    {
                        if (oTestLinePaymentDatePaymentAmount.HasErrors) this.ListErrors.Add(oTestLinePaymentDatePaymentAmount.ErrorMessage);
                        else
                        {
                            oHeader.PaymentDate = oTestLinePaymentDatePaymentAmount.PaymentDate;
                            oHeader.PaymentAmount = oTestLinePaymentDatePaymentAmount.PaymentAmount;
                        }
                    }

                    //parse vendor address
                    this.doConvertVendor(reader, ref CountLine, ref oHeader);

                    //build standard file
                    StandardFile.AppendLine(oHeader.GetHeaderDataLine());
                    if (ListDetail != null && ListDetail.Count > 0)
                    {
                        foreach (PaymentConversionLine oDetailData in ListDetail)
                        {
                            StandardFile.AppendLine(oDetailData.GetDetailDataLine());
                        }
                        //reset variable                                
                        ListDetail = null;
                    }

                    if (!oHeader.IsErrortVendor)
                    {
                        if (!string.IsNullOrEmpty(oHeader.Address1) || !string.IsNullOrEmpty(oHeader.City) || !string.IsNullOrEmpty(oHeader.StateProvince) || !string.IsNullOrEmpty(oHeader.PostalCode))
                        {
                            this.ListVendorAddress.Add(oHeader.ToVendorAddress);
                        }

                    }
                    ListVendors.Add(oHeader);
                    oHeader = null;
                    break;//end of detail line
                }
            } //whilte loop detail
        }
        private string GetHeader()
        {
            StringBuilder sbHeader = new StringBuilder();
            sbHeader.Append(FIRST_CELL_HEADER);
            foreach (string name in HEADER_COLUMNS) sbHeader.Append(SEPARATOR_CELL).Append(name);

            return sbHeader.ToString();
        }

        private string GetHeaderDetail()
        {
            StringBuilder sbDetail = new StringBuilder();
            sbDetail.Append(FIRST_CELL_HEADER_DETAIL);
            foreach (string name in DETAIL_COLUMNS) sbDetail.Append(SEPARATOR_CELL).Append(name);

            return sbDetail.ToString();
        }
    }
}