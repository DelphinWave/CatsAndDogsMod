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

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            
            // SMAPI Commands
            helper.ConsoleCommands.Add("list_pets", "Lists the names of all pets on your farm.", Framework.CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("add_cat", "Adds a cat of given breed. Breed is a number between 0-2. This will give you an in-game naming prompt.", Framework.CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("add_dog", "Adds a dog of given breed. Breed is a number between 0-2. This will give you an in-game naming prompt.", Framework.CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("remove_pet", "Removes pet of given name from your farm.", Framework.CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("list_farmers", "Lists the names and Multiplayer ID of all farmers", Framework.CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("give_pet", "Specify pet name and farmer name that you want to give pet to", Framework.CommandHandler.OnCommandReceived);
            
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
                    SMonitor.Log($"{petToRemove.displayName} has been removed", LogLevel.Debug);
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
            SMonitor.Log($"{petToRemove.displayName} has been removed", LogLevel.Debug);
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
    }
}
