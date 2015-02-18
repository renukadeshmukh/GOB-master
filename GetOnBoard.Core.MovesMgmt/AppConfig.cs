using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.MovesMgmt
{
    public class AppConfig
    {
        private static string botuserId = string.Empty;
        public static string BotUserId
        {
            get
            {
                if (string.IsNullOrEmpty(botuserId))
                {
                    botuserId = ConfigurationManager.AppSettings["BotUserId"];
                    if (string.IsNullOrEmpty(botuserId))
                    {
                        throw new Exception("Bot configurations not found!");
                    }
                }
                return botuserId;
            }
        }
    }
}
