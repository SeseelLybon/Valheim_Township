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
    class SMAI : MonoBehaviour, Hoverable, Interactable
    {

        public string m_name = "Heart";

        public string settlementName;

        public bool isActive = false;

        private Piece m_piece;
        private bool isPlaced = false;

        private ZNetView m_nview;

        private GUIManager m_guiM;

        /*
        GameObject panel_main;
        GameObject checkbox_Active;
        GameObject text_SettlementName;
        GameObject button_RenameSettlement;
        GameObject button_ExitGUI;
        */


        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
        }

        private void Start()
        {
            // A tricky system to deal with the fact that I have to save/network variables,
            //  but also deal with active development and updates
            // Either I keep overwriting values, throw errors in the log or just make useless actions
            // the isPlaced is a hack because the game doesn't track this itself appearantly.
            // Gotta poke Jotunn to use something similar and also a "isPlaced" call like Start but for when a Piece is placed and not a ghost anymore.

            if ( m_piece.IsPlacedByPlayer() )
            {
                isPlaced = true;
            }

            if (isPlaced)
            {
                Jotunn.Logger.LogError("Doing stuff to object that was placed by a player");

                m_nview.SetPersistent(true);


                // Rather ugly code, gotta do something about it later
                // fetch var, if var not there, return default, then set fetched var or default.
                m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness", 95));

                settlementName = m_nview.GetZDO().GetString("settlementName", "no name");
                m_nview.GetZDO().Set("settlementName", settlementName);

                /*
                m_guiM = GUIManager.Instance;

                panel_main = m_guiM.CreateWoodpanel(
                    GUIManager.PixelFix.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 0f), 800f, 600f);

                panel_main.SetActive(false);

                button_ExitGUI = m_guiM.CreateButton(
                    "X",
                    panel_main.transform,
                    new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(50, 50), 50, 50
                    );

                checkbox_Active = m_guiM.CreateToggle(
                    panel_main.transform,
                    new Vector2(0f, 0f),
                    350f, 350f);

                text_SettlementName = m_guiM.CreateText(
                    m_nview.GetZDO().GetString("settlementName"),
                    panel_main.transform,
                    new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(150f, 150f),
                    GUIManager.Instance.AveriaSerifBold, 18, GUIManager.Instance.ValheimOrange, true, Color.black,
                    50, 50,
                    false);

                button_RenameSettlement = m_guiM.CreateButton(
                    "Rename",
                    panel_main.transform,
                    new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(250, 250), 50, 50
                    );
                */
            }
        }

        public void think()
        {
            m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness") -1);
            Jotunn.Logger.LogInfo("thinking...");
        }


        public string GetHoverName()
        {
            // showing the name of the object
            return "Heart of " + settlementName;
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\nActive: " + isActive.ToString() +
                "\nHappiness: " + m_nview.GetZDO().GetInt("Happiness").ToString();
        }

        public bool Interact(Humanoid user, bool hold)
        {
            // Show SMAI gui on press
            // Let remane on hold?

            //test if user == owner of piece

            if ( !hold )
            {
                if( isActive == true)
                {
                    makeActive(false);
                } else
                {
                    makeActive(true);
                }
                //ToggleSMAI_GUI(user);

            }
            else if( hold )
            {
                // blank
            }

            return false;
        }

        public void makeActive(bool toactive)
        {
            if ( toactive && !isActive) // if true and false
            {
                isActive = true;
                InvokeRepeating("think", 5f, 10f);
            } else if (!toactive && isActive) // if false and true
            {
                isActive = false;
                CancelInvoke("think");
            }
            isActive = toactive;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }


        // GUI Stuff //////////////////////////////////////////////////////////////

        /*
        private void ToggleSMAI_GUI(Humanoid user)
        {
            if ( panel_main.activeSelf )
            {
                panel_main.SetActive(false);
                CancelInvoke("test_user_close");
                Jotunn.Logger.LogInfo("Closing GUI");

            } else
            {
                panel_main.SetActive(true);
                last_GUI_user = user;
                InvokeRepeating("test_user_close", 0f, 1f);
                Jotunn.Logger.LogInfo("Opening GUI");
            }
        }

        Humanoid last_GUI_user;
        private void test_user_close() //  Humanoid user 
        {
            if( Vector3.Distance(last_GUI_user.GetCenterPoint(), m_piece.GetCenter()) >= 3f )
            {
                ToggleSMAI_GUI(last_GUI_user);
                CancelInvoke("test_user_close");
                last_GUI_user = null; // since this is a temp var, if this crashes somewhere, then I know the other place shouldn't touch this
                Jotunn.Logger.LogInfo("Closing GUI because user is out of range");
            }
        }
        */

    }
}
