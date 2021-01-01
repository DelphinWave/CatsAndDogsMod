# Cats and Dogs
What this mod lets you do
- Add cats and dogs to your farm
- Choose any of the 3 breeds fore each type of pet
- In Multiplayer, the Host can assign the pet owner and on rainy days the pet will be inside the owner's house

# Known Issues
- The pets will all spawn in the same outdoor location and get stuck to each other. This problem can be fixed with the Pet Spawn Location mod.
  - With the Pet Spawn Location mod you can toggle if pets collide together. If you have a bug where pets get stuck in a big group, try removing pet collisions.
- The pet portrait in player inventory menu displays the portrait of the pet type selected during farm creation even if it's not the same animal type as your pet. I'd like to fix this in a future version.
- Cannot remove pets that have a space in their name - a future version may have workaround using id instead of name
- Cannot give_pet with a space in their name or to a farmer with a space in their name - a future version may have a workaround using id instead of name
- Commands need to be run before starting split-screen multiplayer session
- Naming your pet the same as existing NPCs may have unintended consequences

# SMAPI Commands
- `list_pets` : Lists the name, id, type and owner of all pets on your farm.
- `add_cat` : Adds a cat, with optional breed parameter (0-2). This command gives in-game confirmation and naming dialog box
- `add_dog` : Adds a dog, with optional breed parameter (0-2). This command gives in-game confirmation naming dialog box
- `remove_pet` : Remove's specified pet from your farm, requires pet name as parameter. This command gives in-game confirmation dialog box
- `list_farmers` : Lists all farmers and multiplayer ids
- `give_pet` : Assigns a new owner to a pet. Requires 2 parameters, the pet name and the farmer name

# Example SMAPI Commands
### Adding a cat, the number (between 0-2) indicates the breed/skin
`add_cat`
`add_cat 1`
`add_cat 2`

### Adding a dog, the number (between 0-2) indicates the breed/skin
`add_dog`
`add_dog 1`
`add_dog 2`


### Removing a pet, where PetName is the name of your pet
*Note, this does not work if your pet has a space in their name*
`remove_pet PetName`

### Giving a pet to a multiplayer farmhand, where `PetName` is the name of the pet and `FarmerName` is the name of the farmhand
*Note, this does not work if the names have spaces*
`give_pet PetName FarmerName`
