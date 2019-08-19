using log4net;
using Microsoft.Extensions.Configuration;
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
        public static string GetString(string Key)
        {
            return AppHelper.ToString(ConfigurationManager.AppSettings[Key]);
        }
    }    
}
