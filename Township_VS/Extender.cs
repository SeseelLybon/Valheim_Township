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
    // This class is for use on the Extender totem pieces - Expander totem pieces.
    // Their main purpose is to expand CraftingStation requirements without adding ugly tables everywhere.
    // Allowing for a bigger area and no crafting.

    class Extender : MonoBehaviour, Hoverable, Interactable
    {

        public string m_name = "Extender";
        public string m_extender_type = "PLACEHOLDER";

        public bool isActive = false;

        public Piece m_piece;

        private bool isPlaced = false; // if the Piece was placed before Awake/Start placed in the world (aka loaded from save data rather than placed by the player

        public SMAI parentSMAI; // the SMAI that is connected to this extender
        public bool isOwned; // whether the 


        private ZNetView m_nview;


        // -2 = not connected to a Heart
        // -1 = may be connected to a Heart, but no distance found yet
        //  0 = not supposed to happen?
        // greater than 0 ; distance to the Heart
        public int distanceToHeart = -2;



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

                // test if sitting on top of an active Expander/Heart else self-destruct?


                Jotunn.Logger.LogDebug("Doing stuff to extender that was placed by a player");

            }
        }


        /*
         * find the nearest SMAI and if it is close enough use that as parentSMAI
        public SMAI getNearbySMAI () {
            // ask totems if they're close enough
            // if totem is nearby, ask for it's parentSMAI
        }
        */


        public void couldntfindHeart()
        {
            //makeActive(false);
            Jotunn.Logger.LogError("Couldn't find a Heart totem near this Expander totem.");
            parentSMAI = null;
            distanceToHeart = -2;
        }

        /*
        private void OnDestroy()
        {
            // if this Piece is destroyed, remove from the SMAI's totem list
            parentSMAI.unregisterExpanderTotem( this );
        }
        */


        public string GetHoverName()
        {
            // showing the name of the object
            return "Extender";
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\n" + m_extender_type;
        }

        /*
         * Unlike Hearts and Expanders; extenders get activated/deactivated along with their parent
        public void makeActive(bool toactive)
        {
            if (toactive && !isActive) // if true and false, activate
            {
                SMAI SMAIalksf = m_tsManager.PosInWhichSettlement(m_piece.GetCenter());
                if (SMAIalksf == null || !SMAIalksf.isActive) // null means it isn't in a settlement
                {
                    m_nview.GetZDO().Set("isActive", true);
                    isActive = true;
                    InvokeRepeating("think", 5f, 5f);
                    m_tsManager.registerSMAI(this);
                }
                else
                {
                    Jotunn.Logger.LogMessage("Heart of " + settlementName + "is too close to another Heart");
                }
            }
            else if (!toactive && isActive) // if false and true, deactivate
            {
                m_nview.GetZDO().Set("isActive", false);
                isActive = false;
                CancelInvoke("think");
                m_tsManager.unregisterSMAI(this);
            }
        }
        */

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            throw new NotImplementedException();
        }

        public bool Interact(Humanoid user, bool hold)
        {
            throw new NotImplementedException();
        }
    }
}
