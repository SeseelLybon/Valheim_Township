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
        public const string PluginVersion = "0.1.4.0";
        // Phase    - getting basic totems working
        // Major    - getting expanders working
        

        // Singleton stuff - boy, I hope my teachers don't see this
        // sourced from https://csharpindepth.com/articles/singleton#lock
        private TownshipManager() { }
        //private static readonly object managerLock = new object(); 
        private static Lazy<TownshipManager> instance = new Lazy<TownshipManager>(() => new TownshipManager());
        public static TownshipManager Instance { get { return instance.Value; } }


        public List<SMAI> SMAIList = new List<SMAI>(); // list of SMAI that were created

        private void Awake()
        {


            // Do all your init stuff here
            // Acceptable value ranges can be defined to allow configuration via a slider in the BepInEx ConfigurationManager: https://github.com/BepInEx/BepInEx.ConfigurationManager
            //Config.Bind<int>("Main Section", "Example configuration integer", 1, new ConfigDescription("This is an example config, using a range limitation for ConfigurationManager", new AcceptableValueRange<int>(0, 100)));

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogWarning($"Hello World, from the Township plugin");


            ItemManager.OnVanillaItemsAvailable += addHeart;
            //ItemManager.OnVanillaItemsAvailable += addExpander;
            ItemManager.OnVanillaItemsAvailable += addExtender;

            //ItemManager.OnVanillaItemsAvailable += addDefinerX1;
            //ItemManager.OnVanillaItemsAvailable += addDefinerX2;
            //ItemManager.OnVanillaItemsAvailable += addSubdefinerY1;



            loadLocilizations();
        }


        private void addHeart()
        {
            // duplicating because I'm too lazy to deal with assets right now.
            CustomPiece CP = new CustomPiece("piece_HeartSettlement", "stone_wall_1x1", "Hammer");
            CP.Piece.m_name = "$piece_HeartSettlement";
            CP.Piece.m_description = "$piece_HeartSettlement_desc";

            // Downside of duplicating the ward is that I got to rip out the PrivateArea script and put in the SMAI script.
            // Seems to work without downside. While a rather expensive action, I only have to do it once (per unique piece).
            //Destroy(CP.PiecePrefab.GetComponent<PrivateArea>());
            CP.PiecePrefab.AddComponent<SMAI>();

            //CP.PiecePrefab.AddComponent<CraftingStation>();
            //CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_ExpanderSettlement";
            //CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = 50;

            PieceManager.Instance.AddPiece(CP);
            ItemManager.OnVanillaItemsAvailable -= addHeart;

            Jotunn.Logger.LogDebug("Added Heart Totem to pieceTable Hammer");
        }


        private void addExpander()
        {
            // Just duplicate the ward for now, too lazy to deal with mocks and assents atm
            CustomPiece CP = new CustomPiece("piece_ExpanderSettlement", "stone_wall_1x1", "Hammer");
            CP.Piece.m_name = "$piece_ExpanderSettlement";
            CP.Piece.m_description = "$piece_ExpanderSettlement_desc";
            //Destroy(CP.PiecePrefab.GetComponent<PrivateArea>());
            CP.PiecePrefab.AddComponent<Expander>();

            //CP.PiecePrefab.AddComponent<CraftingStation>();
            //CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_ExpanderSettlement";
            //CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = 50;

            PieceManager.Instance.AddPiece(CP);
            ItemManager.OnVanillaItemsAvailable -= addExpander;
            Jotunn.Logger.LogDebug("Added Expander Totem to pieceTable Hammer");
        }


        private void addExtender()
        {
            // todo; for the asset; something like the banner and a "banner of the forge" to attach to a Extender/Heart that'd be a pillar object thing
            CustomPiece CP = new CustomPiece("piece_ExtenderSettlement", "stone_wall_1x1", "Hammer");
            CP.Piece.m_name = "$piece_ExtenderWorkstation";
            CP.Piece.m_description = "$piece_ExtenderWorkstation_desc";
            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Workstation";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_workbench";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = 50;
            CP.PiecePrefab.GetComponent<CraftingStation>().m_useDistance = 0;
            //CP.PiecePrefab.GetComponent<CraftingStation>().m_icon = ;
            PieceManager.Instance.AddPiece(CP);

            
            CP = new CustomPiece("$piece_ExtenderForge", "stone_wall_1x1", "Hammer");
            CP.Piece.m_name = "$piece_ExtenderForge";
            CP.Piece.m_description = "$piece_ExtenderSettlement_desc";
            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Forge";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$forge";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = 50;
            CP.PiecePrefab.GetComponent<CraftingStation>().m_useDistance = 10;
            PieceManager.Instance.AddPiece(CP);


            CP = new CustomPiece("piece_ExtenderStonecutter", "stone_wall_1x1", "Hammer");
            CP.Piece.m_name = "$piece_ExtenderStonecutter";
            CP.Piece.m_description = "$piece_ExtenderSettlement_desc";
            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Stonecutter";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_stonecutter";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = 50;
            CP.PiecePrefab.GetComponent<CraftingStation>().m_useDistance = 10;
            PieceManager.Instance.AddPiece(CP);


            CP = new CustomPiece("piece_ExtenderArtisanstation", "stone_wall_1x1", "Hammer");
            CP.Piece.m_name = "$piece_ExtenderArtisanstation";
            CP.Piece.m_description = "$piece_ExtenderSettlement_desc";
            CP.PiecePrefab.AddComponent<Extender>();
            CP.PiecePrefab.GetComponent<Extender>().m_extender_type = "Artisan's Station";
            CP.PiecePrefab.AddComponent<CraftingStation>();
            CP.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_artisanstation";
            CP.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = 50;
            CP.PiecePrefab.GetComponent<CraftingStation>().m_useDistance = 10;
            PieceManager.Instance.AddPiece(CP);
            



            ItemManager.OnVanillaItemsAvailable -= addExtender;
            Jotunn.Logger.LogDebug("Added Extender Totems to pieceTable Hammer");
        }


        public void loadLocilizations() {
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations =
                    {
                        { "piece_HeartSettlement", "Heart of the Settlement" },
                        { "piece_HeartSettlement_desc", "Gotta tell somethin descritive here at some point" },
                        { "piece_ExpanderSettlement", "Expander of the Settlement" },
                        { "piece_ExpanderSettlement_desc", "Gotta tell somethin descritive here at some point" }
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

