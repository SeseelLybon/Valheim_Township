using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Configuration;
using UnityEngine;


using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using Logger = Jotunn.Logger;

namespace Township
{
    namespace Commands
    {
        class Rename_Local_Settlement : ConsoleCommand
        {
            public override string Name => "Rename_Local_Settlement";

            public override string Help => "Renames the settlement the player is currently standing in.";

            public override void Run(string[] args)
            {
                SettlementManager.renameLocalSettlement( Player.m_localPlayer.transform.position, args[0]);
                Jotunn.Logger.LogDebug("Ran command");
            }
        }

        class Rename_Named_Settlement : ConsoleCommand
        {
            public override string Name => "Rename_Named_Settlement";

            public override string Help => "Renames the settlement with the provided name. String oldSettlementName, String newSettlementName";

            public override void Run(string[] args)
            {
                SettlementManager.renameNamedSettlement( args[0], args[1] );
                Jotunn.Logger.LogDebug("Ran command");
            }
        }

        class Print_All_Settlements : ConsoleCommand
        {
            public override string Name => "Print_All_Settlements";

            public override string Help => "Prints a list with the name and stats of all settlementManagers";

            public override void Run(string[] args)
            {
                SettlementManager.printAllSettlements();
                Jotunn.Logger.LogDebug("Ran command");
            }
        }

        class ShowAllExpandersOnMinimap : ConsoleCommand
        {
            public override string Name => "ShowAllExpandersOnMinimap";

            public override string Help => "show all expanders on minimap (DEBUG)";

            public override void Run(string[] args)
            {
                Minimap_Extension.Instance.ShowAllExpanders();
                Jotunn.Logger.LogDebug("Ran command");
            }
        }

        class HideAllExpandersOnMinimap : ConsoleCommand
        {
            public override string Name => "HideAllExpandersOnMinimap";

            public override string Help => "Hide all expanders on minimap (DEBUG)";

            public override void Run(string[] args)
            {
                Minimap_Extension.Instance.HideAllExpanders();
                Jotunn.Logger.LogDebug("Ran command");
            }
        }

        class ShowSettlementOnMinimapByName : ConsoleCommand
        {
            public override string Name => "ShowSettlementOnMinimapByName";
            public override string Help => "Shows a settlement's label and expander's area's on the minimap (DEBUG)";
            public override void Run(string[] args)
            {
                SettlementManager.ShowSettlementOnMinimapByName( args[0] );
                Jotunn.Logger.LogDebug("Ran command");
            }
        }

        class HideSettlementOnMinimapByName : ConsoleCommand
        {
            public override string Name => "HideSettlementOnMinimapByName";
            public override string Help => "Shows a settlement's label and expander's area's on the minimap (DEBUG)";
            public override void Run(string[] args)
            {
                SettlementManager.HideSettlementOnMinimapByName( args[0] );
                Jotunn.Logger.LogDebug("Ran command");
            }
        }
    }
}
