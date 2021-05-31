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
            }
        }

        class Rename_Named_Settlement : ConsoleCommand
        {
            public override string Name => "Rename_Named_Settlement";

            public override string Help => "Renames the settlement with the provided name. String oldSettlementName, String newSettlementName";

            public override void Run(string[] args)
            {
                SettlementManager.renameNamedSettlement( args[0], args[1] );
            }
        }

        class Emergency_Clean_ZDOs : ConsoleCommand
        {
            public override string Name => "Emergency_Clean_ZDOs";

            public override string Help => "ONLY IN EMERGENCIES: deletes all ZDO's currently there.";

            public override void Run(string[] args)
            {
                Console.instance.Print("not yet implemented");
            }
        }
    }
}
