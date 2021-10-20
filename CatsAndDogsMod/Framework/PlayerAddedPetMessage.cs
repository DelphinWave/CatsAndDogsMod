using StardewValley.Characters;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CatsAndDogsMod.Framework
{
    class PlayerAddedPetMessage
    {
        public string petName;
        public string petDisplayName;
        public string petOwner;
        public string petSkinId;
        public string petType;

        public PlayerAddedPetMessage(string name, string displayName, string owner, string skinId, string type)
        {
            petName = name;
            petDisplayName = displayName;
            petOwner = owner;
            petSkinId = skinId;
            petType = type;
        }
    }
}
