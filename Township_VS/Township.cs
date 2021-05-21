﻿// JotunnModStub
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
        public const string PluginVersion = "0.1.7.0";
        // Phase    - getting basic totems working
        // Major    - getting a gui for SMAI working
        

        // Singleton stuff - boy, I hope my teachers don't see this
        // sourced from https://csharpindepth.com/articles/singleton#lock
        private TownshipManager() { }
        //private static readonly object managerLock = new object(); 
        private static Lazy<TownshipManager> instance = new Lazy<TownshipManager>(() => new TownshipManager());
        public static TownshipManager Instance { get { return instance.Value; } }


        public List<SMAI> SMAIList = new List<SMAI>(); // list of SMAI that were created

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

        public readonly int CS_buildrange = 20;
        public readonly int Extender_buildrange = 20;
        public readonly int connection_range = 30; // range at which a SMAI/Expanders can connect

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

            CP.PiecePrefab.AddComponent<SMAI>();

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

            CP.PiecePrefab.AddComponent<Expander>();

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
     *  This function is called both when a new SMAI is created when a new Heart is placed
     *      AND when a world is loaded
     */
    public void registerSMAI( SMAI newSMAI )
        {
            Jotunn.Logger.LogInfo("Registering new SMAI " + newSMAI.settlementName);
            SMAIList.Add(newSMAI);
        }

        /*
         *  This function is called when a Heart totem is destroyed
         */
        public void unregisterSMAI( SMAI oldSMAI )
        {
            Jotunn.Logger.LogInfo("Unregistering new SMAI " + oldSMAI.settlementName);
            // ping all totems of this SMAI that thair parentSMAI is long longer there :'(
            SMAIList.Remove(oldSMAI);
        }

        public SMAI PosInWhichSettlement(Vector3 pos)
        {
            foreach(SMAI settlement in SMAIList)
            {
                if( settlement.isPosInThisSettlement(pos) && settlement.isActive )
                {
                    return settlement;
                }
            }


            return null; // this is valid, means Pos isn't in a settlement
        }
    }
}

