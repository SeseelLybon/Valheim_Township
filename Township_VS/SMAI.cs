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
            //m_sm_guiManager = SM_GUI.Instance;
            GetComponent<WearNTear>().m_onDestroyed += OnDestroyed;
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
                makeActive(m_nview.GetZDO().GetBool("isActive", false), checkconnections:true );
                isActive = m_nview.GetZDO().GetBool("isActive", false);
                m_nview.GetZDO().Set("isActive", isActive);
            }
        }



        /*
         */
        public void think()
        {
            m_nview.GetZDO().Set("TestNumber", m_nview.GetZDO().GetInt("TestNumber") +1);
            Jotunn.Logger.LogDebug(settlementName + " thinks, therefore it is.");
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
                return true;
            }
            if (!hold)
            {
                if (isActive == true)
                {
                    makeActive(toactive:false, checkconnections:true);
                    // m_smguiManager.close_gui();
                    return true;
                }
                else
                {
                    makeActive(toactive:true, checkconnections: true);
                    // m_smguiManager.close_gui();
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
                "\n TestNumber: " + m_nview.GetZDO().GetInt("TestNumber").ToString() +
                "\n Expanders: " + expanderList.Count();

            //"\nVillagers: " + m_nview.GetZDO().GetInt("Happiness").ToString() +
            // list of Difiner types
            //"\nDefiners: " + m_nview.GetZDO().GetInt("Happiness").ToString();
        }

        public void makeActive(bool toactive, bool checkconnections)
        {
            if (toactive)// && !isActive) // if making active and is not active, activate
            {
                // test if there's another settlement nearby
                SMAI SMAIasdf = m_tsManager.PosInWhichSettlement(m_piece.GetCenter());
                if (SMAIasdf == null ) // null means it isn't in a settlement
                {
                    m_nview.GetZDO().Set("isActive", true);
                    isActive = true;
                    InvokeRepeating("think", 5f, 5f);
                    m_tsManager.registerSMAI(this);
                    GetComponent<CraftingStation>().m_rangeBuild = m_tsManager.CS_buildrange;

                    if (checkconnections)
                    {
                        checkConnectionstoHeart();
                    }
                }
                else
                {
                    Jotunn.Logger.LogMessage("Heart of " + settlementName + "is too close to another Heart");
                }
            }
            else if (!toactive)// && isActive) // if making unactive and isactive, deactivate
            {
                m_nview.GetZDO().Set("isActive", false);
                isActive = false;
                CancelInvoke("think");
                m_tsManager.unregisterSMAI(this);
                GetComponent<CraftingStation>().m_rangeBuild = 0;
                if (checkconnections)
                {
                    checkConnectionstoHeart();
                }
            }
        }

        
        public void OnDestroyed()
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

        /*  checkConnectionsDistancesfromHeart()
         * This has to be called on register/unregister (activation/deactivation/destruction) but shouldn't need to be called more than that.
         *  It also should be rather cheap to call, unless someone makes a stupidly huge settlement at which point this pathfinding algoritm
         *  won't be their main concern.
         */

        private readonly object checkconnection_lock = new object();
        public void checkConnectionstoHeart()
        {
            lock(checkconnection_lock) // this is a major operation and nobody should access this file (or expanderList tbf) at this time.
            {
                int canidate_range = 2000;

                Jotunn.Logger.LogDebug("checking connections for " + settlementName);

                Jotunn.Logger.LogDebug("Heart center: " + m_piece.GetCenter());

                // list with all the expanders in range that are active
                List<Expander> new_expanderList = new List<Expander>();

                // list of all expanders in a 2000 something range
                List<Expander> localexpanderList = new List<Expander>();

                // no point in testing which are conneccted if there is no active heart to connect to
                if(isActive)
                {


                    // Don't really need to check for Expanders that would be an unreasonable distance way (and not active).
                    // 10 bucks someone files a bug report about this.
                    foreach (Expander n_expander in Expander.m_AllExpanders)
                    {
                        if (Vector3.Distance(n_expander.m_piece.GetCenter(), m_piece.GetCenter()) <= canidate_range && n_expander.isActive)
                        {
                            localexpanderList.Add(n_expander);
                        }
                    }

                    Jotunn.Logger.LogDebug("Found nearby expanders: " + localexpanderList.Count());

                    foreach (Expander n_expander in localexpanderList)
                    {
                        if (Vector3.Distance(n_expander.m_piece.GetCenter(), m_piece.GetCenter()) <= m_tsManager.connection_range && n_expander.isActive)
                        {
                            if (new_expanderList.Contains(n_expander))
                            {
                                Jotunn.Logger.LogDebug("Expander was already in list");
                                continue;
                            }
                            else
                            {
                                Jotunn.Logger.LogDebug("Adding expander and going deeper");
                                new_expanderList.Add(n_expander);
                                checkConnectionstoHeart_R(n_expander, in localexpanderList, ref new_expanderList);
                            }
                        }
                    }
                }


                // deactivate all the Expanders that aren't in the new list
                foreach(Expander old_expander in expanderList)
                {
                    old_expander.changeConnection(toConnect: false, checkconnections: false);
                }

                Jotunn.Logger.LogDebug("Old: " + expanderList.Count() + "| New: " + new_expanderList.Count());
                
                expanderList.Clear();
                expanderList.AddRange( new_expanderList );

                foreach (Expander new_expander in new_expanderList)
                {
                    new_expander.changeConnection(toConnect: true, checkconnections: false);
                }

                Jotunn.Logger.LogDebug("current: " + expanderList.Count() + "| New: " + new_expanderList.Count());

            }
        }

        private void checkConnectionstoHeart_R(Expander c_expander, in List<Expander> localexpanderList, ref List<Expander> new_expanderList)
        {
            foreach (Expander n_expander in localexpanderList)
            {
                if (Vector3.Distance(n_expander.m_piece.GetCenter(), c_expander.m_piece.GetCenter()) <= m_tsManager.connection_range && n_expander.isActive) // this time test against c_expander *not* heart/m_piece.center
                {
                    if (new_expanderList.Contains(n_expander))
                    {
                        continue;
                    } else
                    {
                        new_expanderList.Add(n_expander);
                        checkConnectionstoHeart_R(n_expander, in localexpanderList, ref new_expanderList);
                    }
                }
            }
        }






        /*
         * is it even possible to keep a list of objects that extend from Definer?
         * something something slicing from C++?
         * no need to check connections here, as they don't influence the Expander network
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
