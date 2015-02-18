using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GetOnBoard.BL
{
    public static class Configs
    {
        public static string BaseSiteUrl
        {
            get
            {
                string config = ConfigurationManager.AppSettings["BaseSiteUrl"];
                return config;
            }
        }
    }
}