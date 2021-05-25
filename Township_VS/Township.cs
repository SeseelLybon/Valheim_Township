// JotunnModStub
// a Valheim mod skeleton using Jötunn
// 
// File:    JotunnModStub.cs
// Project: JotunnModStub
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

using HarmonyLib;

using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
//using JotunnModExample.ConsoleCommands;
using Logger = Jotunn.Logger;

namespace Township
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Major)]

    internal sealed class TownshipManager : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.Township";
        public const string PluginName = "Township";

        // Fun      - F is for 'FFS', U is of 'Uhm...', N is for 'NullReferenceException'
        // Phase    - Liquid, Gas, Solid, Plasma, Goth
        // Major    - Milestone within a phase
        // Minor    - Patches or changes or just tweaks.
        public const string PluginVersion = "0.1.8.0";
        // Phase    - getting basic totems working
        // Major    - Reworking how everything works
        

        // Singleton stuff - boy, I hope my teachers don't see this
        // sourced from https://csharpindepth.com/articles/singleton#lock
        private TownshipManager() { }
        //private static readonly object managerLock = new object(); 
        private static Lazy<TownshipManager> instance = new Lazy<TownshipManager>(() => new TownshipManager());
        public static TownshipManager Instance { get { return instance.Value; } }
        //public static List<ExpanderSoul>  = new List<ExpanderSoul>();


        public readonly string settlemanangerprefabname = "SettleManager";
        public readonly string expandersoulprefabname = "ExpanderSoul";


        private void Awake()
        {

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogWarning($"Hello World, from the Township plugin");


            ItemManager.OnVanillaItemsAvailable += addPieces;

            //ItemManager.OnVanillaItemsAvailable += addDefinerX1;
            //ItemManager.OnVanillaItemsAvailable += addDefinerX2;
            //ItemManager.OnVanillaItemsAvailable += addSubdefinerY1;



            loadLocilizations();
        }

        List<ZDO> SettlementManagerZDOs;

        private void Start()
        {
            //TownshipManagerZDOID = "";

            //TownshipManagerZDO = ZDOMan.instance.GetZDO(TownshipManagerZDOID);


            ZDOMan.instance.GetAllZDOsWithPrefab(settlemanangerprefabname, SettlementManagerZDOs);

            foreach(ZDO setmanzdo in SettlementManagerZDOs)
            {
                SettlementManager.m_AllSettleMans.Add(new SettlementManager(setmanzdo) );
            }

        }

        public readonly int CS_buildrange = 20;
        public readonly int Extender_buildrange = 20;
        public readonly int connection_range = 30; // range at which a SettlementManager/Expanders can connect

        public readonly int SOI_range = 20; // Sphere of Influence

        public CraftingStation TS_CS;

        private void addPieces()
        {

            ///////////////////////////////// Heart /////////////////////////////////

            // duplicating because I'm too lazy to deal with assets right now.
            //CustomPiece CP = new CustomPiece("piece_HeartSettlement", "stone_pillar", "Hammer");
            CustomPiece CP = new CustomPiece("piece_TS_Heart", "stone_wall_4x2", "Hammer");
            CP.Piece.m_name = "$piece_TS_Heart";
            CP.Piece.m_description = "$piece_TS_Heart_desc";

            CP.Piece.m_craftingStation = PrefabManager.Cache.GetPrefab<CraftingStation>("piece_workbench");

            CP.Piece.m_resources = new Piece.Requirement[]{
                new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Stone"), m_amount = 10 },
                new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Wood"), m_amount = 10 }
            };

            CP.PiecePrefab.AddComponent<SettlementManager>();

            CP.PiecePrefab.AddComponent<CraftingStation>();
            TS_CS = CP.PiecePrefab.GetComponent<CraftingStation>();
            TS_CS.m_name = "$piece_TS_CS"; ;
            TS_CS.m_rangeBuild = CS_buildrange; // 50 or 45 - the range is for the player *not* the piece. Does that matter?

            PieceManager.Instance.AddPiece(CP);

            Jotunn.Logger.LogDebug("Added Heart Totem to pieceTable Hammer");


            ///////////////////////////////// EXPANDERS /////////////////////////////////



            // Just duplicate the ward for now, too lazy to deal with mocks and assents atm
            CP = new CustomPiece("piece_TS_Expander", "stone_pillar", "Hammer");
            CP.Piece.m_name = "$piece_TS_Expander";
            CP.Piece.m_description = "$piece_TS_Expander_desc";
            CP.Piece.m_craftingStation = TS_CS;
            CP.Piece.m_resources = new Piece.Requirement[]{
                new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Stone"), m_amount = 10 },
                new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Wood"), m_amount = 10 },
            };

            CP.PiecePrefab.AddComponent<ExpanderBody>();

            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_TS_CS";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = CS_buildrange; // 50 or 45 - the range is for the player *not* the piece.

            PieceManager.Instance.AddPiece(CP);
            Jotunn.Logger.LogDebug("Added Expander Totem to pieceTable Hammer");



            ///////////////////////////////// EXTENDERS /////////////////////////////////


            // todo; for the asset; something like the banner and a "banner of the forge" to attach to a Extender/Heart that'd be a pillar object thing
            CP = new CustomPiece("piece_TS_Extender_Workbench", "piece_banner01", "Hammer");
            CP.Piece.m_name = "$piece_TS_Extender_Workbench";
            CP.Piece.m_description = "$piece_TS_Extender_Workbench_desc";
            CP.Piece.m_craftingStation = TS_CS;

            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Workbench";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_workbench";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = Extender_buildrange;
            //CP.PiecePrefab.GetComponent<CraftingStation>().m_icon = ;
            PieceManager.Instance.AddPiece(CP);

            
            CP = new CustomPiece("piece_TS_Extender_Forge", "piece_banner02", "Hammer");
            CP.Piece.m_name = "$piece_TS_Extender_Forge";
            CP.Piece.m_description = "$piece_TS_Extender_Forge_desc";
            CP.Piece.m_craftingStation = TS_CS;
            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Forge";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$forge";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = Extender_buildrange;
            PieceManager.Instance.AddPiece(CP);


            CP = new CustomPiece("piece_TS_Extender_Stonecutter", "piece_banner03", "Hammer");
            CP.Piece.m_name = "$piece_TS_Extender_Stonecutter";
            CP.Piece.m_description = "$piece_TS_Extender_Stonecutter_desc";
            CP.Piece.m_craftingStation = TS_CS;
            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Stonecutter";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_stonecutter";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = Extender_buildrange;
            PieceManager.Instance.AddPiece(CP);


            CP = new CustomPiece("piece_TS_Extender_Artisanstation", "piece_banner04", "Hammer");
            CP.Piece.m_name = "$piece_TS_Extender_Artisanstation";
            CP.Piece.m_description = "$piece_TS_Extender_Artisanstation_desc";
            CP.Piece.m_craftingStation = TS_CS;
            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Artisan's Station";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_artisanstation";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = Extender_buildrange;
            PieceManager.Instance.AddPiece(CP);
            

            Jotunn.Logger.LogDebug("Added Extender Totems to pieceTable Hammer");

            ItemManager.OnVanillaItemsAvailable -= addPieces;
        }

        public void loadLocilizations() {
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations =
                    {
                        { "piece_TS_Heart", "Heart of the Settlement" },
                        { "piece_TS_Heart_desc", "Gotta tell somethin descritive here at some point" },

                        { "piece_TS_Expander", "Expander of the Settlement" },
                        { "piece_TS_Expander_desc", "Gotta tell somethin descritive here at some point" },

                        { "piece_TS_Extender_Workbench", "Banner of the Workbench" },
                        { "piece_TS_Extender_Workbench_desc", "Fulfills the need of a Workbench" },

                        { "piece_TS_Extender_Forge", "Banner of the Forge" },
                        { "piece_TS_Extender_Forge_desc", "Fulfills the need of a forge" },

                        { "piece_TS_Extender_Stonecutter", "Banner of the Stonemason" },
                        { "piece_TS_Extender_Stonecutter_desc", "Fulfills the need of a workstation" },

                        { "piece_TS_Extender_Artisanstation", "Banner of the Artisan" },
                        { "piece_TS_Extender_Artisanstation_desc", "Fulfills the need of a workstation" },
                }
            });
        }

    /*
     *  This function is called both when a new SettlementManager is created when a new Heart is placed
     *      AND when a world is loaded
     */
    public void registerSMAI( SettlementManager newSMAI )
        {
            Jotunn.Logger.LogInfo("Registering new SettlementManager " + newSMAI.settlementName );
            SettlementManager.m_AllSettleMans.Add(newSMAI);
        }

        /*
         *  This function is called when a Heart totem is destroyed
         */
        public void unregisterSMAI( SettlementManager oldSMAI )
        {
            Jotunn.Logger.LogInfo("Unregistering new SettlementManager " + oldSMAI.settlementName);
            // ping all totems of this SettlementManager that thair parentSMAI is long longer there :'(
            SettlementManager.m_AllSettleMans.Remove(oldSMAI);
        }

        public SettlementManager PosInWhichSettlement(Vector3 pos)
        {
            foreach(SettlementManager settlement in SettlementManager.m_AllSettleMans)
            {
                if( settlement.isPosInThisSettlement(pos) )
                {
                    return settlement;
                }
            }


            return null; // this is valid, means Pos isn't in a settlement
        }
    }
}

