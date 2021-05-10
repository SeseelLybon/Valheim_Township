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
        public const string PluginVersion = "0.1.0.1";

        Texture2D testTex;
        Sprite testSprite;

        private void Awake()
        {


            


            // Do all your init stuff here
            // Acceptable value ranges can be defined to allow configuration via a slider in the BepInEx ConfigurationManager: https://github.com/BepInEx/BepInEx.ConfigurationManager
            Config.Bind<int>("Main Section", "Example configuration integer", 1, new ConfigDescription("This is an example config, using a range limitation for ConfigurationManager", new AcceptableValueRange<int>(0, 100)));

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogMessage($"I think, therefore I am");


            ItemManager.OnVanillaItemsAvailable += addHeart;
        }

        private void addHeart()
        {


            //CustomItem CI = new CustomItem("HeartSettlement", "guard_stone");
            //
            //ItemManager.Instance.AddItem(CI);
            //
            // Replace vanilla properties of the custom item
            //var itemDrop = CI.ItemDrop;
            //
            //itemDrop.m_itemData.m_shared.m_name = "$item_HeartSettlement";
            //itemDrop.m_itemData.m_shared.m_description = "$item_HeartSettlement_desc";

            // Add translations for the custom piece in addEmptyItems
            LocalizationManager.Instance.AddLocalization(new LocalizationConfig("English")
            {
                Translations =
                {
                        { "HeartSettlement", "Heart of the Settlement" }
                 }
            });

            CustomPiece CP = new CustomPiece("$HeartSettlement", "Hammer");
            var piece = CP.Piece;

            testTex = AssetUtils.LoadTexture("Assets/Texture2D/sactx-2048x2048-Uncompressed-IconAtlas-61238c20.png");
            testSprite = Sprite.Create(testTex, new Rect(0f, 0f, testTex.width, testTex.height), Vector2.zero);

            piece.m_icon = testSprite;

            var prefab = CP.PiecePrefab;
            prefab.GetComponent<MeshRenderer>().material.mainTexture = testTex;

            PieceManager.Instance.AddPiece(CP);

            // You want that to run only once, Jotunn has the item cached for the game session
            ItemManager.OnVanillaItemsAvailable -= addHeart;

        }




#if DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            { // Set a breakpoint here to break on F6 key press
            }
        }
#endif
    }
}