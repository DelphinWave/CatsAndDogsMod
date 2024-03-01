# Cats and Dogs
What this mod lets you do
- Add multiple cats and dogs to your farm
- In Multiplayer, the Host can assign the pet owner and on rainy days the pet will be inside the owner's house

# Known Issues
- The pets will all spawn in the same outdoor location and get stuck to each other. This problem can be fixed with the Pet Spawn Location mod.
  - With the Pet Spawn Location mod you can toggle if pets collide together. If you have a bug where pets get stuck in a big group, try removing pet collisions.
- The pet portrait in player inventory menu displays the portrait of the pet type selected during farm creation even if it's not the same animal type as your pet. I'd like to fix this in a future version.
- Commands need to be run before starting split-screen multiplayer session
- Naming your pet the same as existing NPCs may have unintended consequences

# SMAPI Commands
- `list_pets` : Lists the name, id, type and owner of all pets on your farm.
- `add_cat` : Adds a cat. This command gives in-game confirmation and naming dialog box
- `add_dog` : Adds a dog. This command gives in-game confirmation naming dialog box
- `remove_pet` : Removes specified pet from your farm, requires pet name as parameter. This command gives in-game confirmation dialog box
- `list_farmers` : Lists all farmers and multiplayer ids
- `give_pet` : Assigns a new owner to a pet. Requires 2 parameters, the pet name and the farmer name

# Example SMAPI Commands
### Adding a cat

`add_cat`



### Adding a dog

`add_dog`



### Removing a pet, where PetName is the name of your pet
*Note, use quotes if your pet has a space in their name*

`remove_pet PetName`
`remove_pet "Pet Name"`

### Giving a pet to a multiplayer farmhand, where PetName is the name of the pet and FarmerName is the name of the farmhand
*Note, use quotes if the names have spaces*

`give_pet PetName FarmerName`
`give_pet "Pet Name" "Farmer Name"`
