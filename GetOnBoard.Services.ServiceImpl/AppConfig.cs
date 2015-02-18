using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.ServiceImpl
{
    public class AppConfig
    {
        private static int maxAllowedGamesPerUser;
        public static int MaxAllowedGamesPerUser
        {
            get
            {
                if (maxAllowedGamesPerUser == 0)
                {
                    string config = ConfigurationManager.AppSettings["MaxAllowedGamesPerUser"];
                    maxAllowedGamesPerUser = 15;
                    if (!int.TryParse(config, out maxAllowedGamesPerUser))
                    {
                        maxAllowedGamesPerUser = 15;
                    }
                }
                return maxAllowedGamesPerUser;
            }
        }

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
