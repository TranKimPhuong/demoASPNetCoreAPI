using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Xml;

namespace WebApi.CommonCore.Helper
{
    public static class ConfigHelper
    {
        static ILog LOGGER = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static string AUTH_WEB_API = "auth.web.api";
        static string SECURITY_SECRET = "security.secret";

        [Obsolete("Remove after key vault migration complete for all project.")]

        public static string WEB_API_KEY
        {
            get
            {
                return GetString(AUTH_WEB_API);
            }
        }
        [Obsolete("Remove after key vault migration complete for all project.")]

        public static string SECRET_KEY
        {
            get
            {
                return GetString(SECURITY_SECRET);
            }
        }

        public static string GetString(string Key)
        {
            return AppHelper.ToString(ConfigurationManager.AppSettings[Key]);
        }
    }    
}
