using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.GameMgmt
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
                    tileSetCountPerGame = 12;
                }
                return tileSetCountPerGame;
            }
        }
    }
}
