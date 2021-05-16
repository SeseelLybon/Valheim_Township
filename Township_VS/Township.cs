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

        // Fun      - F is for FFS, U is of Uggh, N is for NullReferenceException...
        // Phase    - Liquid, Gas, Solid, Plasma, Goth
        // Major    - Milestone within a phase
        // Minor    - Patches or changes or just tweaks.
        public const string PluginVersion = "0.1.0.21";

        // Singleton stuff - boy I hope my teachers don't see this
        private TownshipManager() { }
        static TownshipManager() { }
        //private static readonly object managerLock = new object(); 
        private static TownshipManager instance = new TownshipManager();
        public static TownshipManager Instance {  get { return instance; } }


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

            //ItemManager.OnVanillaItemsAvailable += addDefinerX1;
            //ItemManager.OnVanillaItemsAvailable += addDefinerX2;
            //ItemManager.OnVanillaItemsAvailable += addSubdefinerY1;
        }


        private void addHeart()
        {
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations =
                {
                        { "piece_HeartSettlement", "Heart of the Settlement" },
                        { "piece_HeartSettlement_desc", "Gotta tell somethin descritive here at some point" }

                 }
            });
            // Just duplicate the ward for now, too lazy to deal with mocks and assents atm
            CustomPiece CP = new CustomPiece("piece_HeartSettlement", "guard_stone", "Hammer");
            CP.Piece.m_name = "$piece_HeartSettlement";
            CP.Piece.m_description = "$piece_HeartSettlement_desc";
            // Downside of duplicating the ward is that I got to rip out the PrivateArea script and put in the SMAI script.
            // Seems to work without downside. While a rather expensive action, I only have to do it once (per unique piece).
            Destroy(CP.PiecePrefab.GetComponent<PrivateArea>());
            CP.PiecePrefab.AddComponent<SMAI>();
            PieceManager.Instance.AddPiece(CP);
            ItemManager.OnVanillaItemsAvailable -= addHeart;

            Jotunn.Logger.LogDebug("Added Heart Totem to pieceTable Hammer");
        }


        private void addExpander()
        {
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations =
                {
                        { "piece_ExpanderSettlement", "Expander of the Settlement" },
                        { "piece_ExpanderSettlement_desc", "Gotta tell somethin descritive here at some point" }

                 }
            });
            // Just duplicate the ward for now, too lazy to deal with mocks and assents atm
            CustomPiece CP = new CustomPiece("piece_ExpanderSettlement", "guard_stone", "Hammer");
            CP.Piece.m_name = "$piece_ExpanderSettlement";
            CP.Piece.m_description = "$piece_ExpanderSettlement_desc";
            Destroy(CP.PiecePrefab.GetComponent<PrivateArea>());

            PieceManager.Instance.AddPiece(CP);
            ItemManager.OnVanillaItemsAvailable -= addExpander;
            Jotunn.Logger.LogDebug("Added Expander Totem to pieceTable Hammer");
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