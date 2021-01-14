using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using CatsAndDogsMod.Framework;

namespace CatsAndDogsMod
{
    // TODO:
    // - Use better spawning location
    // - Pet portrait update
    // - Pet sleep in owner's house
    // - Handle ids instead of names to avoid issue with spaces

    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static IManifest SModManifest;

        internal static readonly string PlayerWarpedHomeMessageId = "PlayerHome";

        private static Pet newPet;
        private static Farmer player;
        private static Dictionary<string, Farmer> allFarmers = new Dictionary<string, Farmer>();

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModEntry.SMonitor = Monitor;
            ModEntry.SHelper = helper;
            ModEntry.SModManifest = ModManifest;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;

            // SMAPI Commands
            helper.ConsoleCommands.Add("list_pets", "Lists the names of all pets on your farm.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("add_cat", "Adds a cat of given breed. Breed is a number between 0-2. This will give you an in-game naming prompt.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("add_dog", "Adds a dog of given breed. Breed is a number between 0-2. This will give you an in-game naming prompt.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("remove_pet", "Removes pet of given name from your farm.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("list_farmers", "Lists the names and Multiplayer ID of all farmers", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("give_pet", "Specify pet name and farmer name that you want to give pet to", CommandHandler.OnCommandReceived);
            
        }

        
        /*********
        ** Private methods
        *********/

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            player = Game1.player;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;
            GenerateAllFarmersDict();
            if (Game1.isRaining)
            {
                foreach(Pet pet in GetAllPets())
                {
                    WarpToOwnerFarmHouse(pet);
                }
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            Farmer player = e.Player;
            
            // If it's earlier than 8 or the warped location isn't the player's farmhouse
            if(Game1.timeOfDay < 2000 || e.NewLocation != Utility.getHomeOfFarmer(e.Player))
                return;

            if (!Context.IsMainPlayer)
            {
                // Send message to main player
                SHelper.Multiplayer.SendMessage(
                    message: new PlayerWarpedMessage(player.UniqueMultiplayerID),
                    messageType: PlayerWarpedHomeMessageId,
                    modIDs: new[] { SModManifest.UniqueID }
                    );
                return;
            }

            // Main Player ------------------------------------------
            HandleTeleportingPetsHomeAtNight(player);

        }

        public void HandleTeleportingPetsHomeAtNight(Farmer player)
        {
            if (!isPetOwner(player)) // TODO: make sure that no pets teleported to their house?
                return;

            bool isAPetOnBed = false;

            foreach (Pet pet in GetAllPets())
            {
                if (pet.loveInterest == player.displayName)
                {
                    WarpToOwnerFarmHouse(pet);
                    if (isAPetOnBed)
                    {
                        pet.isSleepingOnFarmerBed.Value = false;
                        WarpPetAgain(pet, player);

                    }
                    if (pet.isSleepingOnFarmerBed.Value)
                        isAPetOnBed = true;
                }
            }
        }

        /// <summary> Raised after a mod message is received over the network. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.Type == PlayerWarpedHomeMessageId && Context.IsMainPlayer && e.FromModID == SModManifest.UniqueID)
            {
                PlayerWarpedMessage message = e.ReadAs<PlayerWarpedMessage>();
                HandleTeleportingPetsHomeAtNight(Game1.getFarmer(message.playerId));
                return;
                
            }
        }

        /// <summary>
        /// Adds the pet to the farm
        /// adds a space to the pet name to avoid conflict with villager names
        /// </summary>
        /// <param name="petName">User-provided name for the pet</param>
        private static void AddPet(string petName)
        {
            if(newPet == null)
            {
                SMonitor.Log($"Something went wrong adding the new pet \"{petName}\". No pet was added", LogLevel.Error);
                Game1.drawObjectDialogue($"{petName} could not be adopted");
                return;
            }
            newPet.Name = petName + " ";
            newPet.displayName = petName + " ";
            WarpToOwnerFarmHouse(newPet);
            Game1.drawObjectDialogue($"{petName} has been adopted");
            newPet = null;
        }

        /// <summary>
        /// Handles dialog box for removing pet
        /// </summary>
        /// <param name="petName">Name of pet to be removed</param>
        internal static void RemovePet(string petName)
        {
            Pet petToRemove = null;

            GetAllPets().ForEach(delegate (Pet pet) {
                if (pet.displayName.Replace(" ", string.Empty) == petName)
                    petToRemove = pet;
            });

            if (petToRemove != null)
            {
                if (Game1.getFarm().characters.Contains(petToRemove))
                    RemoveOutdoorPet(petToRemove);
                else
                    RemoveIndoorPet(petToRemove);
            }
            else
            {
                Game1.drawObjectDialogue($"{petName} could not be removed");
                SMonitor.Log($"{petName} could not be removed", LogLevel.Error);
                return;
            }
            SMonitor.Log($"Pet list after Remove performed", LogLevel.Info);
            GetAllPets().ForEach(delegate (Pet pet) {
                SMonitor.Log($"- {pet.displayName}", LogLevel.Info);
            });
        }

        /// <summary>
        /// Removes a pet from the game when the pet is indoors
        /// </summary>
        /// <param name="petToRemove">Pet object to remove from game</param>
        private static void RemoveIndoorPet(Pet petToRemove)
        {
            GetFarmHouses().ForEach(delegate (FarmHouse farmHouse)
            {
                if (farmHouse.characters.Contains(petToRemove))
                {
                    farmHouse.characters.Remove(petToRemove);
                    Game1.drawObjectDialogue($"{petToRemove.displayName} has been removed");
                    SMonitor.Log($"{petToRemove.displayName} has been removed", LogLevel.Info);
                }
            });
        }

        /// <summary>
        /// Removes a pet from the game when the pet is outside on the farm
        /// </summary>
        /// <param name="petToRemove">Pet object to remove from the game</param>
        private static void RemoveOutdoorPet(Pet petToRemove)
        {
            Game1.getFarm().characters.Remove(petToRemove);
            Game1.drawObjectDialogue($"{petToRemove.displayName} has been removed");
            SMonitor.Log($"{petToRemove.displayName} has been removed", LogLevel.Info);
        }

        /// <summary>
        /// Gets all FarmHouses owned by current players. This is needed to find pets that are indoors
        /// </summary>
        /// <returns>List of FarmHouse objects</returns>
        private static List<FarmHouse> GetFarmHouses()
        {
            List<FarmHouse> farmHouses = new List<FarmHouse>();
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                farmHouses.Add(Utility.getHomeOfFarmer(farmer));
            }
            return farmHouses;
        }

        /// <summary>
        /// Generates Dictionary with all farmers. Farmer Name as key, farmer instance as value
        /// TODO: use multiplayerId because this will break if 2 farmers have same name
        /// </summary>
        private static void GenerateAllFarmersDict()
        {
            allFarmers.Clear();
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer != null && farmer.displayName != null && farmer.displayName != "")
                {
                    allFarmers.Add(farmer.displayName, farmer);
                }
            }
        }

        /// <summary>
        /// Warps pet to the owner's farmhouse/cabin if owner is known. Otherwise, warps to host farmhouse
        /// </summary>
        /// <param name="pet">Instance of pet to warp</param>
        private static void WarpToOwnerFarmHouse(Pet pet)
        {
            if (pet.loveInterest != null)
            {
                if(allFarmers.ContainsKey(pet.loveInterest))
                    pet.warpToFarmHouse(allFarmers[pet.loveInterest]);
                else
                    pet.warpToFarmHouse(Game1.MasterPlayer);
            }
            else
            {
                pet.warpToFarmHouse(Game1.MasterPlayer);
            }
        }

        /// <summary>
        /// Helper function for getting a sprite's texture name
        /// </summary>
        /// <param name="pet">"cat" or "dog"</param>
        /// <param name="breed">breed id for selecting pet texture</param>
        /// <returns>string value for textureName</returns>
        private static string GetPetTextureName(string pet, int breed)
        {
            return $"Animals\\{pet}" + ((breed == 0) ? "" : string.Concat(breed));
        }

        /// <summary>
        /// Dialog Box for Adopting a pet
        /// </summary>
        internal static void ShowAdoptPetDialog(string petType)
        {
            Game1.activeClickableMenu = new ConfirmationDialog($"Would you like to adopt a {petType}?", (who) =>
            {
                if (Game1.activeClickableMenu is ConfirmationDialog cd)
                    cd.cancel();

                // Name Input Dialog
                Game1.activeClickableMenu = new NamingMenu(AddPet, $"What will you name it?");
            });
        }

        /// <summary>
        /// Gets all pets currently on Farm or in FarmHouses
        /// </summary>
        /// <returns>List of Pet objects</returns>
        internal static List<Pet> GetAllPets()
        {
            List<Pet> pets = new List<Pet>();
            foreach (NPC j in Game1.getFarm().characters)
            {
                if (j is Pet)
                {
                    pets.Add(j as Pet);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC i in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (i is Pet)
                    {
                        pets.Add(i as Pet);
                    }
                }
            }
            return pets;
        }

        internal static List<string> GetAllPetOwnerNames()
        {
            List<string> ownerNames = new List<string>();
            foreach(Pet pet in GetAllPets())
            {
                if (pet.loveInterest != null && !ownerNames.Contains(pet.loveInterest))
                {
                    ownerNames.Add(pet.loveInterest);
                }
            }

            return ownerNames;
        }

        /// <summary>
        /// helper to determine if a given farmer is a pet owner
        /// </summary>
        /// <param name="farmer">farmer object to check</param>
        /// <returns>true if farmer is a pet owner</returns>
        internal static bool isPetOwner(Farmer farmer)
        {
            return GetAllPetOwnerNames().Contains(farmer.displayName);
        }

        /// <summary>
        /// Initializes newPet to be a cat
        /// </summary>
        /// <param name="breed">breed id for selecting pet texture</param>
        internal static void InitializeCat(int breed)
        {
            newPet = new Cat(0, 0, breed)
            {
                Name = $"cat{breed}",
                displayName = $"cat{breed}",
                Sprite = new AnimatedSprite(GetPetTextureName("cat", breed), 0, 32, 32),
                Position = new Vector2(0, 0),
                DefaultPosition = new Vector2(0, 0),
                Breather = false,
                willDestroyObjectsUnderfoot = false,
                HideShadow = true,
                loveInterest = player.displayName // to handle pet owner
            };
        }

        /// <summary>
        /// Initializes newPet to be a dog
        /// </summary>
        /// <param name="breed">breed id for selecting pet texture</param>
        internal static void InitializeDog(int breed)
        {
            newPet = new Dog(0, 0, breed)
            {
                Name = $"dog{breed}",
                displayName = $"dog{breed}",
                Sprite = new AnimatedSprite(GetPetTextureName("dog", breed), 0, 32, 32),
                Position = new Vector2(0, 0),
                DefaultPosition = new Vector2(0, 0),
                Breather = false,
                willDestroyObjectsUnderfoot = false,
                HideShadow = true,
                loveInterest = player.displayName // to handle pet owner
            };
        }

        /// <summary>
        /// For assigning an owner to a pet. Owner is stored in loveInterest property of NPC
        /// </summary>
        /// <param name="petName">Name of pet to get new owner</param>
        /// <param name="farmerName">Name of farmer to assign as new owner</param>
        internal static void AssignPetOwner(string petName, string farmerName)
        {
            var petExists = false;
            var farmerExists = false;

            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer.displayName == farmerName)
                {
                    farmerExists = true;
                    break;
                }
            }

            if (farmerExists)
            {
                foreach (Pet pet in GetAllPets())
                {
                    if (pet.displayName.Replace(" ", string.Empty) == petName)
                    {
                        petExists = true;
                        pet.loveInterest = farmerName;
                        SMonitor.Log($"{petName}'s new owner is {farmerName}.", LogLevel.Info);
                        break;
                    }
                }

                if (!petExists)
                {
                    SMonitor.Log($"Could not find pet with name {petName}.", LogLevel.Error);
                    return;
                }
            }
            else
            {
                SMonitor.Log($"Could not find farmer with name {farmerName}.", LogLevel.Error);
                return;
            }
            
        }

        /// <summary>
        /// To warp pet to owner's farmhouse and ensure that only 1 pet will sleep on bed, we need to warp pets a second time if there is a pet on bed
        /// </summary>
        /// <param name="pet">pet to warp</param>
        /// <param name="owner">pet owner</param>
        private void WarpPetAgain(Pet pet, Farmer owner)
        {
            pet.isSleepingOnFarmerBed.Value = false;
            FarmHouse farmHouse = Utility.getHomeOfFarmer(owner);
            Vector2 sleepTile = Vector2.Zero;
            int tries = 0;
            sleepTile = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 5));
            List<Furniture> rugs = new List<Furniture>();
            foreach (Furniture house_furniture in farmHouse.furniture)
            {
                if ((int)house_furniture.furniture_type == 12)
                {
                    rugs.Add(house_furniture);
                }
            }

            if (Game1.random.NextDouble() <= 0.30000001192092896)
            {
                sleepTile = Utility.PointToVector2(farmHouse.getBedSpot()) + new Vector2(0f, 2f);
            }
            else if (Game1.random.NextDouble() <= 0.5)
            {
                Furniture rug = Utility.GetRandom(rugs, Game1.random);
                if (rug != null)
                {
                    sleepTile = new Vector2(rug.boundingBox.Left / 64, rug.boundingBox.Center.Y / 64);
                }
            }
            for (; tries < 50; tries++)
            {
                if (farmHouse.canPetWarpHere(sleepTile) && farmHouse.isTileLocationTotallyClearAndPlaceable(sleepTile) && farmHouse.isTileLocationTotallyClearAndPlaceable(sleepTile + new Vector2(1f, 0f)) && !farmHouse.isTileOnWall((int)sleepTile.X, (int)sleepTile.Y))
                {
                    break;
                }
                sleepTile = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 4));
            }
            if (tries < 50)
            {
                Game1.warpCharacter(pet, farmHouse, sleepTile);
                pet.CurrentBehavior = 1;
            }
            else
            {
                SMonitor.Log($"warp pet back to start location", LogLevel.Debug);
                pet.faceDirection(2);
                Game1.warpCharacter(pet, "Farm", (Game1.getLocationFromName("Farm") as Farm).GetPetStartLocation());
            }
            pet.UpdateSleepingOnBed();
            pet.Halt();
            pet.Sprite.CurrentAnimation = null;
            pet.OnNewBehavior();
            pet.Sprite.UpdateSourceRect();
        }
    }
}
