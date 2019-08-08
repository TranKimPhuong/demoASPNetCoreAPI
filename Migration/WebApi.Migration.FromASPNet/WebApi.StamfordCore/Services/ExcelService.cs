using ExcelDataReader;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using WebApi.CommonCore.Helper;
using WebApi.CommonCore.KeyVault;
using WebApi.CommonCore.Models;
using WebApi.StamfordCore.Models;

namespace WebApi.StamfordCore.Services
{
    public class ExcelService
    {
        readonly byte[] _byteArr;
        readonly string _filename;
        static ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string H_TITLE = @"""VR""" + "," 
                                     + @"""VendorId""" + ","
                                     + @"""Name""" + "," 
                                     + @"""Status""" + "," 
                                     + @"""Is1099""" + "," 
                                     + @"""DefaultRemitToId""" + "," 
                                     + @"""MainAddressId""" + "," 
                                     + @"""HoldStatus""";
        private const string D_TITLE = @"""AR""" + ","
                                     + @"""AddressName""" + "," 
                                     + @"""Address1""" + "," 
                                     + @"""Address2""" + "," 
                                     + @"""Address3""" + "," 
                                     + @"""City""" + ","
                                     + @"""StateProvince""" + ","
                                     + @"""PostalCode""" + ","
                                     + @"""Contact""" + "," 
                                     + @"""PhoneNumber"""  /* + ","
                                     + @"""PaymentMethod""" */;

       
        public ExcelService(byte[] byteArr,string filename)
        {
            _byteArr = byteArr;
            _filename = filename;
        }
        public MessageResponse ReadDataFromExcel()
        {
           
            MessageResponse res = new MessageResponse();

            var outPayment = new List<string>();

            MemoryStream reader = new MemoryStream(_byteArr);

            

            //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(reader);

            Logger.Info("rowsZip");
            //4. DataSet - Create column names from first row

            DataSet result = excelReader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });

            DataTable tblResult = result.Tables[0];
            tblResult.Columns.Add("LineNo", typeof(int));
           

            for (int y = 0; y< tblResult.Rows.Count; y++)
            {
                tblResult.Rows[y]["LineNo"] = y + 1;
            }

            DataTable dtCloned = tblResult.Clone();
            dtCloned.Columns["Vendor ID"].DataType = typeof(string);
            foreach (DataRow row in tblResult.Rows)
            {
                if (row[0] != DBNull.Value)
                {
                    dtCloned.ImportRow(row);
                }
            }
           
            //validate
            int check = 0;
            DataTable selectedTableVendorID = null;
            var rowsVendorID = dtCloned.AsEnumerable()
                .Where(r => string.IsNullOrEmpty(r.Field<string>("Vendor ID")) == true);

            DataTable selectedTableVendorName = null;
            var rowsVendorName = dtCloned.AsEnumerable()
               .Where(r => string.IsNullOrEmpty(r.Field<string>("Name")) == true);

            var rowsVendorIDLess9 = dtCloned.AsEnumerable()
               .Where(r => r.Field<string>("Vendor ID").Length < 9);
          

            
            if (rowsVendorID.Any())
            {
                selectedTableVendorID = rowsVendorID.CopyToDataTable();
                foreach (DataRow row in selectedTableVendorID.Rows)
                {
                    res.messages.Add("Vendor ID required at line : " + row["LineNo"].ToString());
                }
                check++;
            }
           
            if (rowsVendorName.Any())
            {
                selectedTableVendorName = rowsVendorName.CopyToDataTable();
                foreach (DataRow row in selectedTableVendorName.Rows)
                {
                    res.messages.Add("Vendor Name required at line : " + row["LineNo"].ToString());
                }
                check++;
            }
          
            for (int i = dtCloned.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = dtCloned.Rows[i];
                if (string.IsNullOrEmpty(dr["Address - 1"].ToString()) && string.IsNullOrEmpty(dr["Address - 2"].ToString()) && string.IsNullOrEmpty(dr["Address - 3"].ToString()))
                {
                    dr.Delete();
                    dtCloned.AcceptChanges();
                }
                else if (string.IsNullOrEmpty(dr["City"].ToString()))
                {
                    dr.Delete();
                    dtCloned.AcceptChanges();
                }
                else if (string.IsNullOrEmpty(dr["State"].ToString()))
                {
                    dr.Delete();
                    dtCloned.AcceptChanges();
                }
                else if (string.IsNullOrEmpty(dr["Zip + 4 + 2"].ToString()))
                {
                    dr.Delete();
                    dtCloned.AcceptChanges();
                }
                else if (dr["Vendor ID"].ToString().Length<9)
                {
                    dr.Delete();
                    dtCloned.AcceptChanges();
                }
            }


            if (check>0)
           {
               res.code = (int)HttpStatusCode.InternalServerError;
           }
           else
           {


               Trace.WriteLine(H_TITLE);
               Trace.WriteLine(D_TITLE);

               outPayment.Add(H_TITLE);
               outPayment.Add(D_TITLE);
               string Address1 = string.Empty;
               

                foreach (DataRow row in dtCloned.Rows)
               {
                   Vendor V = new Vendor();
                   VendorAddress A = new VendorAddress();
                    Address1 = string.Empty;
                    foreach (DataColumn dc in dtCloned.Columns)
                   {
                        if (!string.IsNullOrEmpty(dc.ColumnName))
                        {
                            switch (dc.ColumnName)
                            {
                                case "Vendor ID":
                                    V.VendorId = row[dc].ToString();
                                    break;
                                case "Name":
                                    V.Name = row[dc].ToString();
                                    break;
                                case "Address - 1":
                                    Address1 = row[dc].ToString().Replace(Convert.ToChar(System.Convert.ToInt32(160)), ' ');
                                    A.Address1 = Address1;
                                    break;
                                case "Address - 2":
                                    A.Address2 = row[dc].ToString();
                                    break;
                                case "Address - 3":
                                    A.Address3 = row[dc].ToString();
                                    break;
                                case "City":
                                    A.City = row[dc].ToString().Trim().TrimEnd(',');
                                    break;
                                case "State":
                                    A.StateProvince = row[dc].ToString();
                                    break;
                                case "Zip + 4 + 2":
                                    A.PostalCode = row[dc].ToString();
                                    break;
                                case "Contact Name":
                                    A.Contact = row[dc].ToString();
                                    break;
                                case "Phone - 1":
                                    A.PhoneNumber = row[dc].ToString();
                                    break;
                            }
                        }
                   }

                   outPayment.Add(@"""VR""" + "," + V.ToString());
                   outPayment.Add(@"""AR""" + "," + A.ToString());



                   Trace.WriteLine(@"""VR""" + "," + V.ToString());
                   Trace.WriteLine(@"""AR""" + "," + A.ToString());
               }
               StringBuilder sb = new StringBuilder();
               foreach (var line in outPayment)
               {
                   sb.AppendFormat("{0}{1}", line, Environment.NewLine);
               }

              //var outputBytes = Encoding.UTF8.GetBytes(sb.ToString());
              string DecryptText = AESHelper.EncryptAES(sb.ToString(), Vault.Current.AESKeyBLOB);
              //BlobHelper.UploadFile(ConfigHelper.GetConnectionString("storage.payment.conversion"), "stamforddev-export", "vendorlisting.csv", outputBytes);

               res.code = (int)HttpStatusCode.OK;
               res.data = DecryptText;
            }
           //6. Free resources (IExcelDataReader is IDisposable)
           excelReader.Close();
           return res;

       }
    }
}
