﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class GetMyTilesRs : Response
    {
        public string GameId { get; set; }

        public List<string> Tiles { get; set; }

        public int TilesRemaining { get; set; }
    }
}