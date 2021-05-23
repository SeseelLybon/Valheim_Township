# Township



## Phase 1 - Birth of a Settlement
Using totems to define a settlement area
### Heart totems
are the core of a settlement from which the SMAI rules
### Expanders
extend the sphere of influence of the SMAI
	
### Extender totem
Fulfills CraftingStation requirements in the local area, 
you don't have to place down a ton of ugly tables everywhere.
They won't come cheap, while having a larger area of coverage,
they don't allow crafting



## Phase 2 - Defination
### Definers
There will be many types,
initially the SMAI just keeps a count of how many are in the settlement.
For the time being they won't have functionality, I'd just like to have a nice selection of them

#### Primary:
Warehouse, House, Wall, Gate, Dock, Farm
#### Secondary:
Restaurant, Hotel, Kitchen, Lighthouse, Animal pen, Workshop, Factory, Watchtower, Townhall
#### Teritary Definers could get done:
For the AI; Roadmarkers, Walkways, Bridges, Mine, Fishing spot, patrol routes, guard spots
Manors, shops, things that are visual like graveyards?


### Sub-Definers
Need to be connected to the appropreate Definer;
Piers for Dock Definers, Rooms for Hotel Definers,
Gates and Wall-walkways for wall Definers

### Map Definers
A kind of definer you place outside of Settlements for specific purposes.
#### Naval connections
Marking a river for naval connections, marking lighthouses, small ports, saver/alternative routes, anti-routes
#### Land connections
marking (safe) roads for land connections, bridges, save spots, ect
#### Borders
Mostly to drawing border lines on the world map
#### Misc.
Pretty sure I'll add something here eventially



## Phase 3 - The worst part; GUI
Adding a GUI to show the info neater, will need it later for NPC's.
### The Settlement Manager
#### Main tab
Rename settlement, activate through here, basic info
#### Info tab
Simplified info of other tabs (amount of vilagers, avg happiness, main buildings)
#### Building tab
List of all buildings and subdefiners in the settlement
#### Villager tab
List of all the villagers in the settlement
#### Job tab
List of all the jobs in the settlement with the ability to assign an amount of people to it
Fishing, Farming, Mining, Cooking, Factory work, Hauling, Repairing, Guarding, 
#### Internal politics tab
Things like policies for players to engange, struggles, demands, desires, ect
#### Extenral Politics/Diplomacy
Make a faction, declare fealty to a faction, war, peace, trade, ect.

### in-world visuals
Adding visuals to how Expanders, Extenders, Definers and Sub-Definers are connected to each other, showing build and SOI area's. (kind of like CraftingStation upgrades showing the sparkly line and CraftingStation placement, but toggleable)
Having settlements (and SOI) be marked on the map.


## Phase 4 - making the settlement actually do something
The first thing I want to implement is something like Crafting-From-Containers.
While a player is in a settlement's SOI, they'll pull resources from the warehouse when building/crafting/refuling
Look into having the SMAI automatically refuel torches, campfires, bonfires and ect. (untill the npc's are implemented)
Maybe have the SMAI also do simple labour like filling smelters with ore and ingots (and depositing output in warehouses)
fill mills and spinners, cook foods

## Phase 4 - Work, my minions, WORK *whip crack*
Adding basic mobs that'll do menial labour.
Mostly hooking into my Settlement system to find out where to go, work, take from and bring stuff to.
Using the Jobs tab of the SMAI gui to assign amounts of minions to the job. I'll probably use ModAILib for this.

This is mostly threading on SlaveGreylings territory so if the reader wants to skip to this phase; [I highly recommend you do so as it's some awesome work](https://www.nexusmods.com/valheim/mods/970?)



## Phase 5 - that time your minions made a union against you
Upgrading minions to villagers

### Needs & Desires
Villagers need a few base items to stay with you;

#### Food
Self explanitory.
Villagers store a small amount of food in houses.
Quality and Quantity depends on the policies.

#### Shelter & Comfort
Each villager needs a bed (in a House Definer's range) of a certain minimum comfort.

#### Contentment
Villagers need to be content for them to stay.
Contentment can be improved by having access to their base needs.
It can be lowered by things like high taxes

### Jobs
Tons of jobs, making them more advanced, ect.
So many ideas, so much work...

#### Farming
#### Fishing
#### Crafting
#### Cooking
#### Factory work
#### Repairing
#### Innkeeping
#### Hauling
Run around fulfilling orders. EI. wood for the lighthouse, coal for factories, moving goods from the docks to the warehouses.
#### Animal keeper
#### Guard/Police
Keeping the peace within and around the settlement.
Walk around the streets, walk on the walls ect, take place on watchtowers.
Some sub system to have them patrol 
#### Soldier/Military

#### Skipper
#### Caravaner
#### Firekeeper
##### General Firekeeper
Walks around in search of torches, bonfires, hearts and ect to keep lit
##### Lighthouse Firekeeper
Because lighthouses are troublesome for pathfinding, they'll get their own system
#### Shopkeepers






## Phase x - 
	Settlement Biome
		Have diffirent kinds of Settlement Biome's spawn around the Heart and Expanders.
		Outpost, Village, Town, City as the totem is further away removed from nature.

## Phase x - Cabinfever
	Throwing people out of their settlements
		Add in reasons for people to still venture out;
			Farming:
				- Don't allow for certain plants to grow within a settlement (e.i Mushrooms, thistle)
				- Have pests spread among monoculture plants
				- Over foraging/hunting; Remove spawners around a settlement so that people have to go further for certain things.
			Have pests spread in settlements:
				- Settlements as a biome; rats, plagues, ect. Have jobs dealing with them
				- But also new stuff
			Powered Heart?
			NPC needs:
				- if a settlement gets big enough, NPCs get complacent/demand luxery goods, like Rimworld.




## Phase x - Age of PvP
	Implement things that help with managing PvPvE; factions, player driven politics and economy,  protection and oppertunities
		Factions
			Things in a settlement can't happen/be done as long as you're not part of that faction, regardless of internal persmissions
			General colour coding and map sharing; people in the same faction share info, outside not.
			NPC's can be part of a faction



### Credits
(if I keep track of them)
