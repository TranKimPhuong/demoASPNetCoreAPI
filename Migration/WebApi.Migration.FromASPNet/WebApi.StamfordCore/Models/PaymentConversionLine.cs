using System;
using System.Collections.Generic;
using System.Text;
using WebApi.CommonCore.Helper;

namespace WebApi.StamfordCore.Models
{
    public class PaymentConversionLine
    {
        const string PREFIX_LETTER_STAR = "*";
        const string ZERO_AMOUNT = ".00";
        const string DEFAULT_ZERO_AMOUNT = "0.0";
        const int DEFAULT_LENGTH_DATE = 6;

        const string ERR_FIELD_FORMAT = "[{0}][{1},{2}]:{3}"; //[<field name>][<start index>,<length>]:<error message>

        public string PaymentNumber { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentAmount { get; set; }
        public string PayeeID { get; set; }
        public string PayeeName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }

        public bool IsErrortVendor { get; set; }

        //public VendorAddress PayeeAddress { get; set;  }

        public string UDF1 { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string UDF2 { get; set; }
        public string InvoiceNumber { get; set; }
        public string NetAmount { get; set; }
        public string InvoiceDate { get; set; }

        public int LineNo { get; set; }
        public string LineString { get; set; }

        private IList<string> _ListErrors = new List<string>();

        public PaymentConversionLine()
        {
            this.LineNo = 0;
            this.LineString = string.Empty;
            this._ListErrors = new List<string>();
        }

        public IList<string> ListErrors
        {
            get
            {
                return this._ListErrors;
            }
        }

        public bool HasErrors
        {
            get
            {
                return (this._ListErrors.Count > 0);
            }
        }

        public string ErrorMessage
        {
            get
            {
                return string.Format("Line No:{0} {1}", this.LineNo, string.Join(";", this._ListErrors));
            }
        }

        /// <summary>
        /// Constructor of payment line
        /// 
        /// </summary>
        /// <param name="LineNo"></param>
        /// <param name="lineString"></param>
        public PaymentConversionLine(int LineNo, string LineString)
            : this()
        {
            this.LineNo = LineNo;
            this.LineString = LineString;
        }

        public string GetHeaderDataLine(string firstCell = "H", string separatorCell = ",")
        {
            //"PaymentNumber", "PaymentDate", "PaymentAmount", "PayeeID"
            StringBuilder sbLine = new StringBuilder();
            sbLine.Append(firstCell);
            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.PaymentNumber));

            string PaymentDateMMDDYY = AppHelper.EscapeCSV(this.PaymentDate);
            string PaymentDateConverted = string.Empty;
            if (!string.IsNullOrEmpty(PaymentDateMMDDYY))
            {
                DateTime? oDate = PaymentDateMMDDYY.ToDate("MMddyy");
                if (oDate.HasValue) PaymentDateConverted = string.Format("{0:MM/dd/yyyy}", oDate.Value);
            }
            sbLine.Append(separatorCell).Append(PaymentDateConverted);

            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.CleanAmount(this.PaymentAmount)));
            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.PayeeID));

            return sbLine.ToString();
        }

        public string GetDetailDataLine(string firstCell = "D", string separatorCell = ",")
        {
            //"UDF1", "PurchaseOrderNumber", "UDF2", "InvoiceNumber", "NetAmount", "InvoiceDate"
            StringBuilder sbLine = new StringBuilder();
            sbLine.Append(firstCell);
            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.UDF1));
            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.PurchaseOrderNumber));
            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.UDF2));
            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.InvoiceNumber));
            sbLine.Append(separatorCell).Append(AppHelper.EscapeCSV(this.CleanAmount(this.NetAmount)));

            string InvoiceDateMMDDYY = AppHelper.EscapeCSV(this.InvoiceDate);
            string InvoiceDateConverted = string.Empty;
            if (!string.IsNullOrEmpty(InvoiceDateMMDDYY))
            {
                DateTime? oDate = InvoiceDateMMDDYY.ToDate("MMddyy");
                if (oDate.HasValue) InvoiceDateConverted = string.Format("{0:MM/dd/yyyy}", oDate.Value);
            }
            sbLine.Append(separatorCell).Append(InvoiceDateConverted);

            return sbLine.ToString();
        }

        public bool IsLineDetail()
        {
            if (!string.IsNullOrEmpty(this.LineString)
                && LineString.Length > 70 && LineString.Substring(70,1) == ".")
            {
                int startIndex = -1;

                //UDF1:7:14
                this.UDF1 = this.GetText("UDF1", startIndex + 7, 14);
                //PurchaseOrderNumber:24:6
                this.PurchaseOrderNumber = this.GetText("Purchase Order Number", startIndex + 24, 6);
                //UDF2:33:3
                this.UDF2 = this.GetText("UDF2", startIndex + 33, 3);
                //InvoiceNumber:38:22
                this.InvoiceNumber = this.GetText("Invoice Number", startIndex + 38, 22);
                //NetAmount:63:11
                this.NetAmount = this.GetText("Net Amount", startIndex + 63, 11);
                //InvoiceDate:75:6
                this.InvoiceDate = this.GetText("Invoice Date", startIndex + 75, 6);
                //format pad left for the field date MMDDYY
                if (!string.IsNullOrEmpty(this.InvoiceDate))
                {
                    //validate string date
                    this.InvoiceDate = this.ToDateStamford(this.InvoiceDate.PadLeft(DEFAULT_LENGTH_DATE, '0'));
                }

                return true;
            }
            return false;
        }

        private string ToDateStamford(string dateMMDDYY)
        {
            string outDate = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(dateMMDDYY))
                {
                    outDate = dateMMDDYY.PadLeft(6, '0');
                    DateTime? result = dateMMDDYY.ToDate();
                    if (result.HasValue == false)
                    {
                        int mm = 0;
                        int yy = 0;

                        Int32.TryParse(AppHelper.Left(dateMMDDYY, 2), out mm);
                        Int32.TryParse(AppHelper.Right(dateMMDDYY, 2), out yy);

                        if (mm > 0 && yy > 0)
                        {
                            int dd = DateTime.DaysInMonth(yy, mm);
                            result = new DateTime(yy, mm, dd);

                            outDate = result.Value.ToString("MMddyy");
                        }
                    }
                    else
                    {
                        outDate = result.Value.ToString("MMddyy");
                    }
                }
            }
            catch (Exception ex) { Console.Write(ex.Message); }

            return outDate;
        }

        //           INVOICE         PURCHASE ORDER  DESCRIPTION          ACCOUNT
        public bool IsLineOverflow()
        {
            return (!string.IsNullOrEmpty(this.LineString)
                    && this.LineString.StartsWith("           INVOICE")
                    && this.LineString.EndsWith("ACCOUNT"));
        }

        /// <summary>
        /// PaymentNumber:55:6
        /// </summary>
        /// <returns></returns>
        public bool IsLinePaymentNumber()
        {
            int Index = -1;
            int startIndex = Index + 55;
            int length = 0; //end of line

            //clear old value
            this.PaymentNumber = string.Empty;
            if (!string.IsNullOrEmpty(this.LineString) && this.LineString.Length > startIndex
                && char.IsWhiteSpace(this.LineString, 0))
            {
                string leftString = this.LineString.Left(startIndex);
                if (string.IsNullOrWhiteSpace(leftString))
                {
                    this.PaymentNumber = this.GetText("Payment Number", startIndex, length);
                    return (!string.IsNullOrEmpty(this.PaymentNumber));
                }
            }
            return false;
        }

        /// <summary>
        /// PaymentDate=35:6 and PaymentAmount=52:19
        /// </summary>
        /// <returns></returns>
        public bool IsLinePaymentDatePaymentAmount()
        {
            int Index = -1;
            int startIndexDate = Index + 35;
            int lengthDate = 6;

            int startIndexAmount = Index + 52;
            int lengthAmount = 19;

            this.PaymentDate = string.Empty;
            this.PaymentAmount = string.Empty;
            if (!string.IsNullOrEmpty(this.LineString)
                && this.LineString.Length > startIndexAmount
                && char.IsWhiteSpace(this.LineString, 0)
                && this.LineString.IndexOf(PREFIX_LETTER_STAR) == startIndexAmount)
            {
                string leftPadDate = this.LineString.Left(startIndexDate);
                if (string.IsNullOrWhiteSpace(leftPadDate))
                {
                    this.PaymentDate = this.GetText("Payment Date", startIndexDate, lengthDate);
                    this.PaymentAmount = this.GetText("Payment Amount", startIndexAmount, lengthAmount);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// PayeeID:2,9
        /// </summary>
        /// <returns></returns>
        public bool IsLinePayeeID()
        {
            this.PayeeID = string.Empty;
            int Index = -1;

            int startIndexPaymentAmount = Index + 71;

            int startIndexPayeeID = Index + 2;
            int lengthPayeeID = 9;
            this.PayeeID = string.Empty;
            if (!string.IsNullOrEmpty(this.LineString)
                && this.LineString.Length > startIndexPaymentAmount
                && char.IsWhiteSpace(this.LineString, 0)
                && this.LineString.IndexOf(PREFIX_LETTER_STAR) == startIndexPaymentAmount)
            {
                string payeeIdTest = this.GetText("Payee ID", startIndexPayeeID, lengthPayeeID);
                if (!string.IsNullOrEmpty(payeeIdTest) && this.HasErrors == false)
                {
                    this.PayeeID = payeeIdTest;
                    return true;
                }
            }
            return false;
        }

        public bool IsLinePayeeName()
        {
            int Index = -1;
            int startIndex = Index + 16;
            int length = 0;//end of line
            this.PayeeName = string.Empty;
            if (!string.IsNullOrEmpty(this.LineString) && this.LineString.Length > startIndex)
            {
                string leftString = this.LineString.Left(startIndex);
                if (string.IsNullOrWhiteSpace(leftString))
                {
                    this.PayeeName = this.GetText("Payee Name", startIndex, length);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// PayeeAddress2	14	16	CRLF
        /// </summary>
        /// <returns></returns>
        public bool IsLineAddress1()
        {
            int Index = -1;
            int startIndex = Index + 16;
            int length = 0;//end of line
            this.Address1 = string.Empty;
            if (!string.IsNullOrEmpty(this.LineString) && this.LineString.Length > startIndex)
            {
                string leftString = this.LineString.Left(startIndex);
                if (string.IsNullOrWhiteSpace(leftString))
                {
                    this.Address1 = this.GetText("Payee Address1", startIndex, length);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// PayeeAddress3	15	16	CRLF
        /// </summary>
        /// <returns></returns>
        public bool IsLineAddress2()
        {
            int Index = -1;
            int startIndex = Index + 16;
            int length = 0;//end of line
            this.Address2 = string.Empty;
            if (!string.IsNullOrEmpty(this.LineString) && this.LineString.Length > startIndex)
            {
                string leftString = this.LineString.Left(startIndex);
                if (string.IsNullOrWhiteSpace(leftString))
                {
                    this.Address2 = this.GetText("Payee Address2", startIndex, length);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Line:16
        /// city		16	22
        /// state		39	2
        /// zip		    42	9
        /// </summary>
        /// <returns></returns>
        public bool IsLineCityStateZip()
        {
            int Index = -1;
            int startIndex = Index + 16;
            int length = 0;//end of line

            int StartIndexCity = Index + 16; //16 --> 22
            int StartIndexState = Index + 39; //39 --> 2
            int StartIndexZip = Index + 42; //42 --> CRLF

            this.City = string.Empty;
            this.StateProvince = string.Empty;
            this.PostalCode = string.Empty;
            if (!string.IsNullOrEmpty(this.LineString) && this.LineString.Length > StartIndexZip)
            {
                string leftString = this.LineString.Left(startIndex);
                if (string.IsNullOrWhiteSpace(leftString))
                {
                    string StringCityStateZip = this.GetText("City/State/Zip", startIndex, length);
                    if (!string.IsNullOrEmpty(StringCityStateZip) && char.IsNumber(StringCityStateZip, StringCityStateZip.Length - 1))
                    {
                        try
                        {
                            this.City = GetText(this.LineString, "City", StartIndexCity, 22);
                            this.StateProvince = GetText(this.LineString, "State", StartIndexState, 2);
                            this.PostalCode = GetText(this.LineString, "Postal Code", StartIndexZip, 0);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            //ignore error this case
                            Console.WriteLine(ex.Message);
                        }

                        //int zipCode = 0;
                        return (!string.IsNullOrEmpty(this.City)
                                && !string.IsNullOrEmpty(this.StateProvince)
                                && !string.IsNullOrEmpty(this.PostalCode));
                    }
                }
            }
            return false;
        }

        public VendorInfomation ToVendorAddress
        {
            get
            {
                VendorInfomation oVendorAddress = new VendorInfomation();
                oVendorAddress.VendorKey = PayeeID;
                oVendorAddress.VendorName = this.PayeeName;
                oVendorAddress.Address1 = this.Address1;
                oVendorAddress.Address2 = this.Address2;
                oVendorAddress.Address3 = this.Address3;
                oVendorAddress.City = this.City;
                oVendorAddress.StateProvince = this.StateProvince;
                oVendorAddress.PostalCode = PostalCode;

                return oVendorAddress;
            }
        }

        private string CleanAmount(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                //prefix text
                string output = input.Replace(PREFIX_LETTER_STAR, string.Empty);

                //zero amount
                if (output.Equals(ZERO_AMOUNT)) return DEFAULT_ZERO_AMOUNT;

                //swap the letter subtract
                if (output.EndsWith("-"))
                {
                    output = output.Replace("-", string.Empty);
                    output = string.Format("-{0}", output);
                    return output;
                }

                return output;
            }
            return string.Empty;
        }

        private string GetText(string InputString, string FieldName, int StartIndex, int FieldLength)
        {
            if (!string.IsNullOrEmpty(InputString) && StartIndex < InputString.Length)
            {
                if (FieldLength > 0)
                {
                    string text = InputString.Substring(StartIndex, FieldLength);
                    return text.Trim();
                }
                else
                {
                    string text = InputString.Substring(StartIndex);
                    return text.Trim();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get a text from the input string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="inputString"></param>
        /// <param name="startIndex"></param> : The zero-based starting character position of a substring in this instance.
        /// <param name="length"></param> : The number of characters in the substring.
        /// <returns></returns>
        private string GetText(string FieldName, int StartIndex, int FieldLength)
        {
            try
            {
                return GetText(this.LineString, FieldName, StartIndex, FieldLength);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                string errorField = string.Format(ERR_FIELD_FORMAT, FieldName, (StartIndex + 1), FieldLength, ex.Message);
                this._ListErrors.Add(errorField);
            }
            return string.Empty;
        }
    }
}