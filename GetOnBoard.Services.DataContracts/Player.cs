using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts
{
    public class Player : Account
    {
        public Player() { }
        public Player(Account copy)
        {
            this.UserName = copy.UserName;
            this.AccessToken = copy.AccessToken;
            this.Email = copy.Email;
            this.FirstName = copy.FirstName;
            this.LastName = copy.LastName;
            this.Id = copy.Id;
            this.Password = copy.Password;
            this.Points = copy.Points;
            this.MaxGameLimit = copy.MaxGameLimit;
            this.Level = copy.Level;
        }

        public bool IsActive { get; set; }

        public bool IsHost { get; set; }

        public List<string> Tiles { get; set; }

        public int TilesRemaining { get; set; }
    }
}
