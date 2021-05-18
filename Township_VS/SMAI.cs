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

    // Funfact; the SMAI and the Heart are intrisicly connected because I'm lazy. So in the code they're generally one and the same.
    // The SMAI referencing to the manager and Heart referencing to the piece

    class SMAI : MonoBehaviour, Hoverable, Interactable
    {

        public string settlementName;

        private Piece m_piece;
        public bool isPlaced = false;
        public bool isActive;

        private ZNetView m_nview;
        private TownshipManager m_tsManager;

        private List<Expander> expanderList = new List<Expander>(); // list of expander totems connected to this SMAI

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();         
            m_tsManager = TownshipManager.Instance;
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

                Jotunn.Logger.LogDebug("Doing stuff to object that was placed by a player");

                m_nview.SetPersistent(true);


                // Rather ugly code, gotta do something about it later
                // fetch var, if var not there use default, then set fetched var or default.
                m_nview.GetZDO().Set("TestNumber", m_nview.GetZDO().GetInt("TestNumber", 0));
                m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness", 100));
                m_nview.GetZDO().Set("Villagers", m_nview.GetZDO().GetInt("Villagers", 0));

                m_nview.GetZDO().Set("settlementName", m_nview.GetZDO().GetString("settlementName", "Elktown"));
                settlementName = m_nview.GetZDO().GetString("settlementName");

                // Got to do this twice because of how makeActive is a toggle.
                makeActive(m_nview.GetZDO().GetBool("isActive", false) );
                isActive = m_nview.GetZDO().GetBool("isActive", false);
                m_nview.GetZDO().Set("isActive", isActive);

            }
        }




        /*
         */
        public void think()
        {
            m_nview.GetZDO().Set("TestNumber", m_nview.GetZDO().GetInt("TestNumber") +1);
            Jotunn.Logger.LogDebug(settlementName + " is thinking of...");
            //Jotunn.Logger.LogDebug("Elk");
        }


        /* Function is now only for making active or not.
         *  Will later be used to call the GUI
         */
        public bool Interact(Humanoid user, bool hold)
        {
            if( !user.IsOwner() )
            {
                return true;
            }
            if (hold)
            {
                return false;
            }
            if (!hold)
            {
                if (isActive == true)
                {
                    makeActive(false);
                    return true;
                }
                else
                {
                    makeActive(true);
                    return true;
                }
            }

            // if !hold if active call gui, else throw soft warning
            // if hold toggle active

            return false;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
            // invoke the move thingy here with... an item... somehow?
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
                "\n Active: " + m_nview.GetZDO().GetBool("isActive").ToString() +
                "\n TestNumber: " + m_nview.GetZDO().GetInt("TestNumber").ToString();

            //"\nVillagers: " + m_nview.GetZDO().GetInt("Happiness").ToString() +
            //"\nExpander Totems: " + m_nview.GetZDO().GetInt("Happiness").ToString();
            // list of Difiner types
            //"\nDefiners: " + m_nview.GetZDO().GetInt("Happiness").ToString();
        }

        public void makeActive(bool toactive)
        {
            if (toactive && !isActive) // if true and false, activate
            {
                SMAI SMAIalksf = m_tsManager.PosInWhichSettlement(m_piece.GetCenter());
                if (SMAIalksf == null ||  !SMAIalksf.isActive) // null means it isn't in a settlement
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

        /*
         */
        public void registerExpanderTotem( Expander totem_to_be_added )
        {
            Jotunn.Logger.LogInfo("Registering a new totem to SMAI of " + settlementName);

            // add totem to list
            expanderList.Add(totem_to_be_added);

            // Tell other totems a new totem was registered and recheck their connections to the Heart.
            checkConnectionsDistancesfromHeart();
        }

        /*
         */
        public void unregisterExpanderTotem( Expander totem_to_be_removed )
        {
            Jotunn.Logger.LogInfo("Unregistering a totem from the SMAI of " + settlementName);

            // Can I assume the totem has been deactivated on it's side?
            expanderList.Remove(totem_to_be_removed);
            
            // Tell other totems one of their own got unregistered and recheck their connections to the Heart.
            checkConnectionsDistancesfromHeart();
        }

        public void OnDestroy()
        {
            if( m_piece.IsPlacedByPlayer() )
            {
                m_tsManager.unregisterSMAI(this);
            }
        }


        public bool isPosInThisSettlement( Vector3 pos )
        {
            if( Vector3.Distance( m_piece.GetCenter(), pos ) <= 30 )
            {
                return true;
            }
            foreach( Expander totem in expanderList )
            {
                if ( Vector3.Distance(totem.m_piece.GetCenter(), pos) <= 30 )
                {
                    return true;
                }
            }
            return false;
        }

        // public void OnGUI() {}

        /*  checkConnectionsDistancesfromHeart()
         * This has to be called on register/unregister (activation/deactivation/destruction) but shouldn't need to be called more than that.
         *  It also should be rather cheap to call, unless someone makes a stupidly huge settlement at which point this pathfinding algoritm
         *  won't be their main concern.
         */
        public void checkConnectionsDistancesfromHeart()
        {
            // run some pathfinding algoritm to test what totems are considered to be still connected.
            //  probably something recursive. Dijkstra?
            //      Find all totems that're touching this and enter the first one, giving it a distance of 1
            //          if it can't find any, return
            //          Find all totems touching this one, ask what the closest one is and if this one's distence is shorter, pass it on.
            //              GO DEEPER
            //              if it can't find any, return

            // Go through the expanderList again and throw out (and unregister) any Expanders that haven't been assigned a distance.
            Jotunn.Logger.LogMessage("SMAI.checkConnectionsDistancesfromHeart() hasn't been filled out yet");
        }



        /*
         * //is it even possible to keep a list of objects that extend from Definer?
         * 
        public void registerWorkshopDefinerTotem()
        public void registerWarehouseDefinerTotem()
        public void registerHouseDefinerTotem()
        public void registerDockDefinerTotem()
        public void registerIndustrialDefinerTotem()
        public void registerInnDefinerTotem()
        public void registerFarmlandDefinerTotem()
        public void registerWallDefinerTotem()
        public void registerGateDefinerTotem()
        public void registerRoadDefinerTotem()
        public void registerMineDefinerTotem()
        public void registerDefinerTotem()

        {
            // add totem to the list of totems
        }
        */

    }
}
