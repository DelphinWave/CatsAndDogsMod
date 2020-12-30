using System;
using System.Collections.Generic;

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
            var petType = "unknown type";
            var breed = 0;
            var petName = "";
            var farmerName = "";
            switch (command)
            {
                case "list_pets":
                    ModEntry.GetAllPets().ForEach(delegate (Pet pet) {
                        petType = "unknown type";
                        if (pet.GetType() == typeof(Cat)) petType = "cat";
                        if (pet.GetType() == typeof(Dog)) petType = "dog";
                        ModEntry.SMonitor.Log($"- {pet.id} {pet.displayName}, {petType}, owner: {pet.loveInterest}", LogLevel.Info);
                    });
                    return;
                case "add_cat":
                    breed = 0;
                    if(args.Length == 1)
                    {
                        try
                        {
                            breed = Int32.Parse(args[0]);
                            if (breed < 0 || breed > 2)
                            {
                                ModEntry.SMonitor.Log($"{args[0]} is an invalid breed value. Must be 0, 1, or 2.", LogLevel.Error);
                                return;
                            }
                        }
                        catch
                        {
                            ModEntry.SMonitor.Log($"{args[0]} is an invalid breed value. Must be 0, 1, or 2.", LogLevel.Error);
                            return;
                        }
                    }
                    else if (args.Length > 1)
                    {
                        ModEntry.SMonitor.Log($"add_cat only takes one argument, the breed number.", LogLevel.Error);
                        return;
                    }
                    ModEntry.InitializeCat(breed);
                    ModEntry.ShowAdoptPetDialog("cat");
                    return;
                case "add_dog":
                    breed = 0;
                    if (args.Length == 1)
                    {
                        try
                        {
                            breed = Int32.Parse(args[0]);
                            if(breed < 0 || breed > 2)
                            {
                                ModEntry.SMonitor.Log($"{args[0]} is an invalid breed value. Must be 0, 1, or 2.", LogLevel.Error);
                                return;
                            }
                        }
                        catch
                        {
                            ModEntry.SMonitor.Log($"{args[0]} is an invalid breed value. Must be 0, 1, or 2.", LogLevel.Error);
                            return;
                        }
                    }
                    else if (args.Length > 1)
                    {
                        ModEntry.SMonitor.Log($"add_dog only takes one argument, the breed number.", LogLevel.Error);
                        return;
                    }
                    ModEntry.InitializeDog(breed);
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
                        ModEntry.SMonitor.Log($"- {farmer.displayName}: {farmer.uniqueMultiplayerID}", LogLevel.Info);
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
