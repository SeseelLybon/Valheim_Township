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

        public string m_name = "Expander";

        public string settlementName;

        public bool isActive = false;

        public Piece m_piece;

        private bool isPlaced = false; // if the Piece was placed before Awake/Start placed in the world (aka loaded from save data rather than placed by the player

        public SMAI parentSMAI; // the SMAI that is connected to this expander
        public bool isOwned; // whether the 
        

        private ZNetView m_nview;


        // -2 = not connected to a Heart
        // -1 = may be connected to a Heart, but no distance found yet
        //  0 = not supposed to happen?
        // greater than 0 ; distance to the Heart
        public int distanceToHeart = -2; // in lines, a direct connection is 1



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
                

                /*
                 * // if this object was loaded from save, and it was active
                // i
                 */

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
            makeActive(false);
            Jotunn.Logger.LogError("Couldn't find a Heart totem near this Expander totem.");
            parentSMAI = null;
            distanceToHeart = -2;
        }

        /* Function is now only for making active or not.
          *  Will later be used to call the GUI
          */
        public bool Interact(Humanoid user, bool hold)
        {
            if (!hold)
            {
                if (isActive == true)
                {
                    makeActive(false);
                }
                else
                {
                    makeActive(true);
                }

            }

            // if !hold if active call gui, else throw soft warning
            // if hold toggle active

            return false;
        }

        public void makeActive(bool toactive)
        {
            if (toactive && !isActive) // if true and false
            {
                m_nview.GetZDO().Set("isActive", true);
                isActive = true;
                InvokeRepeating("think", 0f, 10f);
            }
            else if (!toactive && isActive) // if false and true
            {
                m_nview.GetZDO().Set("isActive", false);
                isActive = false;
                CancelInvoke("think");
            }
        }

        /*
        private void OnDestroy()
        {
            // if this Piece is destroyed, remove from the SMAI's totem list
            parentSMAI.unregisterExpanderTotem( this );
        }
        */


        /*
         * function to allow a heart to be moved after being placed without destroying it.
         * Have the player place an expander totem, some kind of interaction and then swap the two's location.
        private void moveHeart(SMAI hearttomove, Expander expandertomoveto )
        {
            temp Vector3 hearttomove.m_piece.getPosition();
            hearttomove.m_piece.setPosition(expandertomoveto.m_piece.getPosition() ); 
            expandertomoveto.m_piece.setPosition(temp.m_piece.getPosition() );
        }
        */


        public string GetHoverName()
        {
            // showing the name of the object
            return "Expander of " + parentSMAI.settlementName;
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\nActive: " + m_nview.GetZDO().GetBool("isActive");
        }


    }
}
