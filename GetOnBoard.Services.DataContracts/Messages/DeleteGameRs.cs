﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class DeleteGameRs : Response
    {
        public string GameId { get; set; }
    }
}
