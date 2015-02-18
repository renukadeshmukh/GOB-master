using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SDK = Appacitive.Sdk;

namespace GetOnBoard.Data.Provider.Appacitive.AppHelper
{
    public class AppConfigurations
    {
        public static string AppacitiveKey
        {
            get
            {
                string config = ConfigurationManager.AppSettings["AppacitiveKey"];
                return config;
            }
        }

        public static SDK.Environment AppacitiveEnviornment
        {
            get
            {
                string config = ConfigurationManager.AppSettings["AppacitiveEnviornment"];
                if (string.Equals(config, "Live", StringComparison.CurrentCultureIgnoreCase))
                {
                    return SDK.Environment.Live;
                }
                return SDK.Environment.Sandbox;
            }
        }

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

        public static int MaxTilesPerPlayer
        {
            get
            {
                string config = ConfigurationManager.AppSettings["MaxTilesPerPlayer"];
                int maxTilesPerPlayer = 0;
                if (!int.TryParse(config, out maxTilesPerPlayer))
                {
                    maxTilesPerPlayer = 36;
                }
                return maxTilesPerPlayer;
            }
        }

        public static int TileSetCountPerGame
        {
            get
            {
                string config = ConfigurationManager.AppSettings["TileSetCountPerGame"];
                int tileSetCountPerGame = 0;
                if (!int.TryParse(config, out tileSetCountPerGame))
                {
                    tileSetCountPerGame = 36;
                }
                return tileSetCountPerGame;
            }
        }

        public static bool IsLoggingEnabled
        {
            get
            {
                string config = ConfigurationManager.AppSettings["EnableLogging"];
                if (string.IsNullOrEmpty(config) || string.Equals(config, "Y", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(config, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static int LogCallsSlowerThan
        {
            get
            {
                string config = ConfigurationManager.AppSettings["LogCallsSlowerThan"];
                int ms = 600;
                if (!int.TryParse(config, out ms))
                {
                    ms = 600;
                }
                return ms;
            }
        }
    }
}
