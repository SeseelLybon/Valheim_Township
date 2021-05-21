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
    class SM_GUI : MonoBehaviour
    {
        // Singleton stuff - boy, I hope my teachers don't see this
        // sourced from https://csharpindepth.com/articles/singleton#lock
        private static Lazy<SM_GUI> instance = new Lazy<SM_GUI>(() => new SM_GUI());
        public static SM_GUI Instance { get { return instance.Value; } }

        // https://github.com/Barril/ValheimMods/blob/master/NameTamedAnimals/Patches/Tameable.cs
        // Trying to learn from Barril's code how to get a rename UI thingy going.


        // TAB Main
        private GameObject panel_main;
        private GameObject panel_secondary;
        private GameObject button_toMainTab;
        private GameObject text_settlementName;
        private GameObject checkbox_settlementIsActive;


        // TAB Info
        private GameObject button_toInfoTab;

        // TAB Buildings
        // TAB Villagers
        // TAB Jobs

        private SM_GUI() {
            createGUIElements();
         }

        private void createGUIElements()
        {
            var guim = GUIManager.Instance; // calling this one a lot, so lets cache it

            panel_main = guim.CreateWoodpanel(
                GUIManager.PixelFix.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), 850, 600
                );

            panel_secondary = guim.CreateWoodpanel(
                panel_main.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), 850, 600
                );

            text_settlementName = guim.CreateText(
                "Unset",
                GUIManager.PixelFix.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f),
                GUIManager.Instance.AveriaSerifBold, 18, GUIManager.Instance.ValheimOrange, true, Color.black,
                400f, 30f, false);

            checkbox_settlementIsActive = guim.CreateToggle(
                GUIManager.PixelFix.transform,
                new Vector2(0f, 0f),
                40f, 40f);
        }

        public bool isGUIshown;
        public void OnGUI()
        {
            if (isGUIshown)
            {

            }
        }

        public void showGUI(SMAI localSMAI)
        {
            text_settlementName.name = localSMAI.settlementName;
            checkbox_settlementIsActive.SetActive( localSMAI.isActive );
        }
    }
}
