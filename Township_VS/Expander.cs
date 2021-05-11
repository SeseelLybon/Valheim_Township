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

    // This class is for use on the Expander totems.
    // Their main purpose is to expand the SMAI's sphere of unfluence.
    // Most functions are about passing questions on to the SMAI.

    class Expander : MonoBehaviour, Hoverable, Interactable
    {

        public bool isActive = false;

        private Piece m_piece;
        private bool isPlaced = false;

        private ZNetView m_nview;

        public string m_name = "Expander";

        private void Start()
        {
            // A tricky system to deal with the fact that I have to save/network variables,
            //  but also deal with active development and updates
            // Either I keep overwriting values, throw errors in the log or just make useless actions
            // the isPlaced is a hack because the game doesn't track this itself appearantly.
            // Gotta poke Jotunn to use something similar and also a "isPlaced" call like Start but for when a Piece is placed and not a ghost anymore.

            if (m_piece.IsPlacedByPlayer())
            {
                isPlaced = true;

                m_nview = GetComponent<ZNetView>();
                m_piece = GetComponent<Piece>();
                m_nview.m_persistent = true;

                // Rather ugly code, gotta do something about it later
                // fetch var, if var not there, return default, then set fetched var or default.
                //m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness", 95));

                //settlementName = m_nview.GetZDO().GetString("settlementName", "no name");
                //m_nview.GetZDO().Set("settlementName", settlementName);
            }
        }

        public void think()
        {
            Jotunn.Logger.LogError("Expanders don't think, they're just messagers.");
        }

        public string GetHoverName()
        {
            // showing the name of the object
            return "Expander of " + "PLACEHOLDER";//settlementName;
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\nActive: " + isActive.ToString() +
                "\nHappiness:" + m_nview.GetZDO().GetInt("Happiness").ToString();
        }

        public bool Interact(Humanoid user, bool hold)
        {
            // Show SMAI gui on press
            // Let remane on hold?

            //test if user == owner of piece

            if (!hold)
            {

                if (isActive)
                {
                    isActive = false;
                }
                else
                {
                    isActive = true;
                }

            }
            else if (hold)
            {
            }

            return false;
        }

        public void RCP_renameSettlement()
        {
            //tldr test if 
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

    }
}
