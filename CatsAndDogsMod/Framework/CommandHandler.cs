using System;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;

namespace CatsAndDogsMod.Framework
{
    class CommandHandler
    {
        /// <summary>
        /// Handles SMAPI commands
        /// </summary>
        /// <param name="command">The command entered in SMAPI console</param>
        /// <param name="args">The arguments entered with the command</param>
        internal static void OnCommandReceived(string command, string[] args)
        {
            if (!ModEntry.IsEnabled)
                return;

            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
            {
                ModEntry.SMonitor.Log("Your farm has not loaded yet, please try command again once farm is loaded", LogLevel.Warn);
                return;
            }

            if (!Context.IsMainPlayer)
            {
                ModEntry.SMonitor.Log("Only the host can write commands && commands are not currently supported during split-screen multiplayer", LogLevel.Warn);
                return;
            }
                
            var petType = "unknown type";
            var petName = "";
            var farmerName = "";
            switch (command)
            {
                case "list_pets":
                    ModEntry.GetAllPets().ForEach(delegate (Pet pet) {
                        if (pet is Cat) petType = "cat";
                        if (pet is Dog) petType = "dog";
                        var owner = pet.modData.ContainsKey(ModEntry.MOD_DATA_OWNER) ? pet.modData[ModEntry.MOD_DATA_OWNER] : "unknown";
                        var skinId = pet.modData.ContainsKey(ModEntry.MOD_DATA_SKIN_ID) ? pet.modData[ModEntry.MOD_DATA_SKIN_ID] : "none";
                        ModEntry.SMonitor.Log($"{pet.displayName}, {petType}, owner: {owner}, skinId: {skinId}", LogLevel.Info);
                    });
                    return;
                case "add_cat":
                    ModEntry.InitializeCat();
                    ModEntry.ShowAdoptPetDialog("cat");
                    return;
                case "add_dog":
                    ModEntry.InitializeDog();
                    ModEntry.ShowAdoptPetDialog("dog");
                    return;
                case "remove_pet":
                    if(args.Length == 0)
                    {
                        ModEntry.SMonitor.Log($"You must specify the name of the pet to remove. Try list_pets to see all valid names", LogLevel.Error);
                        return;
                    }
                    else if (args.Length > 1)
                    {
                        ModEntry.SMonitor.Log($"remove_pet only takes one argument, the name of the pet you wish to remove", LogLevel.Error);
                        return;
                    }
                    petName = args[0];
                    Game1.activeClickableMenu = new ConfirmationDialog($"Are you sure you want to remove {petName}?", (who) =>
                    {
                        if (Game1.activeClickableMenu is ConfirmationDialog cd)
                            cd.cancel();

                        ModEntry.RemovePet(petName);
                    });
                    return;
                case "list_farmers":
                    foreach(Farmer farmer in Game1.getAllFarmers())
                        ModEntry.SMonitor.Log($"- {farmer.displayName}: {farmer.UniqueMultiplayerID}", LogLevel.Info);
                    return;
                case "give_pet":
                    if(args.Length < 2 || args.Length > 2)
                    {
                        ModEntry.SMonitor.Log($"give_pet requires 2 arguments, the name of the pet you wish to give and the name of the farmer you want to give the pet to", LogLevel.Error);
                        return;
                    }
                    petName = args[0];
                    farmerName = args[1];
                    ModEntry.AssignPetOwner(petName, farmerName);
                    return;
                default:
                    ModEntry.SMonitor.Log($"Unknown command '{command}'.", LogLevel.Error);
                    return;
            }
        }
    }
}
