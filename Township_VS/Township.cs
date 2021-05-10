// JotunnModStub
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
        public const string PluginVersion = "0.1.0.2";

        private void Awake()
        {


            // Do all your init stuff here
            // Acceptable value ranges can be defined to allow configuration via a slider in the BepInEx ConfigurationManager: https://github.com/BepInEx/BepInEx.ConfigurationManager
            Config.Bind<int>("Main Section", "Example configuration integer", 1, new ConfigDescription("This is an example config, using a range limitation for ConfigurationManager", new AcceptableValueRange<int>(0, 100)));

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogWarning($"Hello World, from the Township plugin");


            ItemManager.OnVanillaItemsAvailable += addHeart;
        }

        private void addHeart()
        {
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations =
                {
                        { "piece_HeartSettlement", "Heart of the Settlement" },
                        { "piece_HeartSettlement_desc", "Currently doesn't do anything" }

                 }
            });

            Jotunn.Logger.LogWarning($"1");
            CustomPiece CP = new CustomPiece("piece_heartSettlement", "guard_stone", "Hammer");


            Jotunn.Logger.LogWarning($"2");
            CP.Piece.m_name = "piece_HeartSettlement";
            CP.Piece.m_description = "piece_HeartSettlement_desc";

            Destroy(CP.PiecePrefab.GetComponent<PrivateArea>());

            CP.PiecePrefab.AddComponent<SMAI>();


            Jotunn.Logger.LogWarning($"3");
            PieceManager.Instance.AddPiece(CP);

            Jotunn.Logger.LogWarning($"4");
            // You want that to run only once, Jotunn has the item cached for the game session
            ItemManager.OnVanillaItemsAvailable -= addHeart;
            Jotunn.Logger.LogWarning($"5");

        }
    }
}