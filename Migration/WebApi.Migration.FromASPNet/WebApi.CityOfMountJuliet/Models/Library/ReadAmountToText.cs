namespace WebApi.CityOfMountJuliet.Models.Library
{
    internal class ReadAmountToText
    {
        #region ReadTextCurrency

        private static void WordsInit(ref string[] pWrds)
        {
            pWrds[1] = "One";
            pWrds[2] = "Two";
            pWrds[3] = "Three";
            pWrds[4] = "Four";
            pWrds[5] = "Five";
            pWrds[6] = "Six";
            pWrds[7] = "Seven";
            pWrds[8] = "Eight";
            pWrds[9] = "Nine";
            pWrds[10] = "Ten";
            pWrds[11] = "Eleven";
            pWrds[12] = "Twelve";
            pWrds[13] = "Thirteen";
            pWrds[14] = "Fourteen";
            pWrds[15] = "Fifteen";
            pWrds[16] = "Sixteen";
            pWrds[17] = "Seventeen";
            pWrds[18] = "Eighteen";
            pWrds[19] = "Nineteen";
            pWrds[20] = "Twenty";
            pWrds[21] = "Thirty";
            pWrds[22] = "Forty";
            pWrds[23] = "Fifty";
            pWrds[24] = "Sixty";
            pWrds[25] = "Seventy";
            pWrds[26] = "Eighty";
            pWrds[27] = "Ninety";
        }

        private static string WordsXlate(int piVal, string[] pWrds)
        {
            string functionReturnValue = null;
            if (piVal < 21)
                functionReturnValue = pWrds[piVal];
            else
                functionReturnValue = pWrds[18 + (piVal / 10)];
            return functionReturnValue;
        }

        private static string ConvertIncludeCent(decimal pcAmount)
        {
            string functionReturnValue = null;
            //UPGRADE_WARNING: Lower bound of array Words was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            string[] Words = new string[28];
            string sAmount = null;
            string sNum = null;
            int iVal = 0;
            int i = 0;
            int iTemp = 0;

            if (pcAmount > 999999999.99m)
            {
                functionReturnValue = "VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID";
                return functionReturnValue;
            }

            if (pcAmount == 0)
            {
                functionReturnValue = "Zero Dollar And 00 Cent";
                return functionReturnValue;
            }

            if (pcAmount < 0.01m)
            {
                functionReturnValue = "";
                return functionReturnValue;
            }

            functionReturnValue = "";
            sAmount = pcAmount.ToString("000000000.00");
            WordsInit(ref Words);
            for (i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    sNum = sAmount.Substring(0, 3);
                }
                else if (i == 1)
                {
                    sNum = sAmount.Substring(3, 3);
                }
                else
                {
                    sNum = sAmount.Substring(6, 3);
                }

                if (int.Parse(sNum.Substring(0, 1)) > 0)
                {
                    iVal = int.Parse(sNum.Substring(0, 1));
                    //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    functionReturnValue = functionReturnValue + WordsXlate(iVal, Words) + " Hundred ";
                }
                iTemp = int.Parse(sNum.Substring(1, 2));
                if (iTemp > 0)
                {
                    if (iTemp > 20)
                    {
                        iVal = int.Parse(sNum.Substring(1, 1) + "0");
                        //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        functionReturnValue = functionReturnValue + WordsXlate(iVal, Words);
                        if (int.Parse(sNum.Substring(2, 1)) > 0)
                        {
                            iVal = int.Parse(sNum.Substring(2, 1));
                            //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            functionReturnValue = functionReturnValue + "-" + WordsXlate(iVal, Words) + " ";
                        }
                        else
                        {
                            functionReturnValue = functionReturnValue + " ";
                        }
                    }
                    else
                    {
                        iVal = int.Parse(iTemp.ToString().TrimStart(' '));
                        //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        functionReturnValue = functionReturnValue + WordsXlate(iVal, Words) + " ";
                    }
                }
                if (pcAmount > 999999.99m & i == 0)
                {
                    functionReturnValue = functionReturnValue + "Million ";
                }
                if (pcAmount > 999.99m & i == 1 & int.Parse(sNum) > 0)
                {
                    functionReturnValue = functionReturnValue + "Thousand ";
                }
            }

            if (pcAmount < 1)
            {
                functionReturnValue = "Zero";
            }

            if (sAmount.Substring(sAmount.Length - 2, 2) == "00")
            {
                functionReturnValue = functionReturnValue + " Dollar And 00/100 Cent";
            }
            else
            {
                functionReturnValue = functionReturnValue + " Dollar And " + sAmount.Substring(sAmount.Length - 2, 2) + "/100 Cents";
            }

            if (pcAmount > 1)
                functionReturnValue = functionReturnValue.Replace("Dollar", "Dollars");
            return functionReturnValue;
        }

        private static string ConvertNoIncludeCent(decimal pcAmount)
        {
            string functionReturnValue = null;
            //UPGRADE_WARNING: Lower bound of array Words was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            string[] Words = new string[28];
            string sAmount = null;
            string sNum = null;
            int iVal = 0;
            int i = 0;
            int iTemp = 0;

            if (pcAmount > 999999999.99m)
            {
                functionReturnValue = "VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID***VOID";
                return functionReturnValue;
            }

            if (pcAmount == 0)
            {
                functionReturnValue = "00/100 Dollars";
                return functionReturnValue;
            }

            if (pcAmount < 0.01m)
            {
                functionReturnValue = "";
                return functionReturnValue;
            }

            functionReturnValue = "";
            sAmount = pcAmount.ToString("000000000.00");
            WordsInit(ref Words);
            for (i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    sNum = sAmount.Substring(0, 3);
                }
                else if (i == 1)
                {
                    sNum = sAmount.Substring(3, 3);
                }
                else
                {
                    sNum = sAmount.Substring(6, 3);
                }

                if (int.Parse(sNum.Substring(0, 1)) > 0)
                {
                    iVal = int.Parse(sNum.Substring(0, 1));
                    //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    functionReturnValue = functionReturnValue + WordsXlate(iVal, Words) + " Hundred ";
                }
                iTemp = int.Parse(sNum.Substring(1, 2));
                if (iTemp > 0)
                {
                    if (iTemp > 20)
                    {
                        iVal = int.Parse(sNum.Substring(1, 1) + "0");
                        //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        functionReturnValue = functionReturnValue + WordsXlate(iVal, Words);
                        if (int.Parse(sNum.Substring(2, 1)) > 0)
                        {
                            iVal = int.Parse(sNum.Substring(2, 1));
                            //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            functionReturnValue = functionReturnValue + "-" + WordsXlate(iVal, Words) + " ";
                        }
                        else
                        {
                            functionReturnValue = functionReturnValue + " ";
                        }
                    }
                    else
                    {
                        iVal = int.Parse(iTemp.ToString().TrimStart(' '));
                        //UPGRADE_WARNING: Couldn't resolve default property of object wordsXlate(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        functionReturnValue = functionReturnValue + WordsXlate(iVal, Words) + " ";
                    }
                }
                if (pcAmount > 999999.99m & i == 0)
                {
                    functionReturnValue = functionReturnValue + "Million ";
                }
                if (pcAmount > 999.99m & i == 1 & int.Parse(sNum) > 0)
                {
                    functionReturnValue = functionReturnValue + "Thousand ";
                }
            }

            if (pcAmount < 1)
            {
                functionReturnValue = "";
            }
            else
            {
                functionReturnValue = functionReturnValue + " And ";
            }

            if (sAmount.Substring(sAmount.Length - 2, 2) == " 00")
            {
                functionReturnValue = functionReturnValue + " 00/100 " + "Dollar";
            }
            else
            {
                functionReturnValue = functionReturnValue + sAmount.Substring(sAmount.Length - 2, 2) + "/100 " + "Dollar";
            }

            if (pcAmount != 1)
                functionReturnValue = functionReturnValue + "s";
            return functionReturnValue;
        }

        internal static string Convert(decimal pcAmout, bool includeCent)
        {
            return includeCent ? ConvertIncludeCent(pcAmout) : ConvertNoIncludeCent(pcAmout);
        }
        #endregion
    }
}