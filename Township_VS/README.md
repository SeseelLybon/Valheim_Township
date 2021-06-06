# Township


All the Code/indepth related stuff.


## Flow of code

### Loading

The code basically starts with the hook TownshipManager.OnZNetAwake().
Here I fetch all ZDO's of a type and then create the associated objects with the data in the ZDO.
For this, each object is condstructed with the appropreate constructor which assumes the ZDO already exists.

### Placing an Extemder

When placing an extender, the ExtenderBody component is used, which is the 'body' part.
Opposite to the above, a new ZDO is created and an ExpanderSoul is created (with it's own ZDO) and the two are accosiated and connected.
The Body mostly functions as a relay between the physical plane (the player doing something with the Piece) and the metaphysical (the Soul).
The Soul is where most of the data storage and processing happens. If something physical must happen, it'll relay that to it's Body.

The Soul will always be loaded and *cannot* be unloaded. 
The Body isn't always loaded and doesn't need to be as the Soul will handle most of the communication and data.

In further writing, I'll use "Expander" but will mean both the Soul and Body depending on context.

### Activating a lone Extenders

When placed, an Extender doesn't do anything and needs to be activated.

The player 'uses' the Extender to see if there's any SettlementManagers available nearby to connect to.
If there are none, the Expander constructs a new SettlementManager and registeres itself to it.

When deactivated, the Expander will unregister itself from the SettlementManager. If there are no other Expanders connected to that SettlementManager, it'll remove itself.
(May implement a cooldown time in case of accidental removal)

### Activating a couple of Extenders

When Extender B is activated near Extender A, most likely B can register itself to the SettlementManager of A.
Sharing the same SettlementManager, these two together now form a bigger settlement.
As many Extenders as needed to cover the whole settlement can be connected together this way.

Each time Extenders are activated/deactivated, they'll do a Depth-first search based on distance to Extenders around them.
The main use is to find Extenders that have no longer a connection with the main cluster and de-activate them.




## TODO & Stuck


### Invalid or Unused ZDO's

The current way ZDO's are created and connected to ExpanderSouls and SettlementManagers is heavily flawed.
The game doesn't expect this and will claim on various occassions that the ZDO's are not used.
If this error pops up in the log and the game is saved, the world is permanently corrupt.

### Multiplayer

In SettlementManager, there's a function "think" on which I call "invokerepeat".
Here is where settlement wide things are handled. For example; how much wood is produced/used, which jobs need to be done, giving NPCs/AI orders ect.

Because of the strange position SettlementManager and ExpanderSoul inhabit, they don't have access to a ZNetView. As such they can't call RPC's.

There's also the issue of who runs the SettlementManager.think code.
I originally wanted to run this code on the server, but it seems that this is impossible.
