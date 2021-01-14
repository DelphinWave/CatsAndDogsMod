using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewModdingAPI;
using StardewValley;

namespace CatsAndDogsMod.Framework
{
    class PlayerWarpedMessage
    {
        public long playerId;

        public PlayerWarpedMessage(long playerId)
        {
            this.playerId = playerId;
        }
    }
}
