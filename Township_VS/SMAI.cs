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
                Jotunn.Logger.LogDebug("Doing stuff to object that was placed by a player");

                m_nview.SetPersistent(true);


                // Rather ugly code, gotta do something about it later
                // fetch var, if var not there use default, then set fetched var or default.
                m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness", 95));


                settlementName = m_nview.GetZDO().GetString("settlementName", "Elktown");
                m_nview.GetZDO().Set("settlementName", settlementName);


                isActive = m_nview.GetZDO().GetBool("isActive", false);
                m_nview.GetZDO().Set("isActive", isActive);


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

            if ( !hold )
            {
                if( isActive == true)
                {
                    makeActive(false);
                } else
                {
                    makeActive(true);
                }

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
                m_nview.GetZDO().Set("isActive", true);
                isActive = true;
                InvokeRepeating("think", 5f, 10f);
            } else if (!toactive && isActive) // if false and true
            {
                m_nview.GetZDO().Set("isActive", false);
                isActive = false;
                CancelInvoke("think");
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

    }
}
