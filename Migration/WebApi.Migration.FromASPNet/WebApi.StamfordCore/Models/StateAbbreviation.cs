using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.StamfordCore.Models
{
    public class StateAbbreviation
    {
        static Dictionary<string, string> STATES = new Dictionary<string, string>() {
                //US
                {"Alabama","AL"},
                {"Alaska","AK"},
                {"Arizona","AZ"},
                {"Arkansas","AR"},
                {"California","CA"},
                {"Colorado","CO"},
                {"Connecticut","CT"},
                {"Delaware","DE"},
                {"District of Columbia","DC"},
                {"Florida","FL"},
                {"Georgia","GA"},
                {"Hawaii","HI"},
                {"Idaho","ID"},
                {"Illinois","IL"},
                {"Indiana","IN"},
                {"Iowa","IA"},
                {"Kansas","KS"},
                {"Kentucky","KY"},
                {"Louisiana","LA"},
                {"Maine","ME"},
                {"Maryland","MD"},
                {"Massachusetts","MA"},
                {"Michigan","MI"},
                {"Minnesota","MN"},
                {"Mississippi","MS"},
                {"Missouri","MO"},
                {"Montana","MT"},
                {"Nebraska","NE"},
                {"Nevada","NV"},
                {"New Hampshire","NH"},
                {"New Jersey","NJ"},
                {"New Mexico","NM"},
                {"New York","NY"},
                {"North Carolina","NC"},
                {"North Dakota","ND"},
                {"Ohio","OH"},
                {"Oklahoma","OK"},
                {"Oregon","OR"},
                {"Pennsylvania","PA"},
                {"Rhode Island","RI"},
                {"South Carolina","SC"},
                {"South Dakota","SD"},
                {"Tennessee","TN"},
                {"Texas","TX"},
                {"Utah","UT"},
                {"Vermont","VT"},
                {"Virginia","VA"},
                {"Washington","WA"},
                {"West Virginia","WV"},
                {"Wisconsin","WI"},
                {"Wyoming","WY"},
                //Canada
                {"Alberta", "AB"},
                {"British Columbia", "BC"},
                {"Manitoba", "MB"},
                {"New Brunswick", "NB"},
                {"Newfoundland and Labrador", "NL"},
                {"Northwest Territories", "NT"},
                {"Nova Scotia", "NS"},
                {"Nunavut", "NU"},
                {"Ontario", "ON"},
                {"Prince Edward Island", "PE"},
                {"Quebec", "QC"},
                {"Saskatchewan", "SK"},
                {"Yukon", "YT"}
        };
      
        public StateAbbreviation()
        {
         

        }
        public static bool isRightAbbreviation(string abbr)
        {
            return (!string.IsNullOrEmpty(abbr) && STATES.ContainsValue(abbr));
        }
        public static string ToAbbreviation(string abbr)
        {

            if (!string.IsNullOrEmpty(abbr))
            {

                if (STATES.ContainsKey(abbr.ToUpper()))
                    return (STATES[abbr]);
                /* error handler is to return an empty string rather than throwing an exception */
                return abbr.ToUpper();
            }
            return "";
        }
    }
}