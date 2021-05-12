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

    class Expander : MonoBehaviour//, Hoverable, Interactable
    {

        public string m_name = "Heart";

        public string settlementName;

        public bool isActive = false;

        private Piece m_piece;

        private bool isPlaced = false; // if the Piece is currently placed in the world

        public SMAI childofSMAI; // the SMAI that is connected to this expander
        

        private ZNetView m_nview;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
        }

        private void Start()
        {

            if (m_piece.IsPlacedByPlayer())
            {
                
                // if ZDO.getbool wasPlaced == true
                // Skip over specific initializer stuff
                // else
                // ZDO.set wasPlaced == true;
                // do specific initializer stuffs


                Jotunn.Logger.LogDebug("Doing stuff to expander that was placed by a player");


                m_nview.SetPersistent(true);


                m_nview.GetZDO().Set("isActive", m_nview.GetZDO().GetBool("isActive", false));

            }
        }


        // When placed, an Expander totem is inert and doesn't do anything
        // When activated/interacted with, the totem will check if there's any other totems in it's range
        //  if it finds one that's active, ask what it's SMAI is and link to that.
        //  if it doesn't find one, create it's own SMAI and become a new settlement.










        public string GetHoverName()
        {
            // showing the name of the object
            return "Expander of " + childofSMAI.m_name;
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\nActive: " + m_nview.GetZDO().GetBool("isActive");
        }

        public bool Interact(Humanoid user, bool hold)
        {

            if (!hold)
            {
                if (m_nview.GetZDO().GetBool("isActive") == true)
                {
                    m_nview.GetZDO().Set("isActive", false);
                }
                else
                {
                    m_nview.GetZDO().Set("isActive", true);
                }

            }
            else if (hold)
            {
                // blank
            }

            return false;
        }


    }
}
