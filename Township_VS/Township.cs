﻿// JotunnModStub
// a Valheim mod skeleton using Jötunn
// 
// File:    JotunnModStub.cs
// Project: JotunnModStub

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

    internal class Main : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.Township";
        public const string PluginName = "Township";
        public const string PluginVersion = "0.1.0.8";

        // public list<SMAI> settlementList;

        private void Awake()
        {


            // Do all your init stuff here
            // Acceptable value ranges can be defined to allow configuration via a slider in the BepInEx ConfigurationManager: https://github.com/BepInEx/BepInEx.ConfigurationManager
            //Config.Bind<int>("Main Section", "Example configuration integer", 1, new ConfigDescription("This is an example config, using a range limitation for ConfigurationManager", new AcceptableValueRange<int>(0, 100)));

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogWarning($"Hello World, from the Township plugin");


            ItemManager.OnVanillaItemsAvailable += addExpander;
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
            CustomPiece CP = new CustomPiece("piece_heartSettlement", "guard_stone", "Hammer");

            CP.Piece.m_name = "$piece_ExpanderSettlement";
            CP.Piece.m_description = "$piece_ExpanderSettlement_desc";


            // Downside of duplicating the ward is that I got to rip out the PrivateArea script and put in the SMAI script.
            // Seems to work without downside. While a rather expensive action, I only have to do it once (per unique piece).
            Destroy(CP.PiecePrefab.GetComponent<PrivateArea>());
            CP.PiecePrefab.AddComponent<SMAI>();


            PieceManager.Instance.AddPiece(CP);


            ItemManager.OnVanillaItemsAvailable -= addExpander;
        }


        // int unique_settlement_ID = 0;
        // claim unique_settlement_ID();
        // ZDO.mod(unique_settlement_ID += 1)
        // return unique_settlement_ID;



    }
}