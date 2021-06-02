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

    // Funfact; the SettlementManager and the Heart are intrisicly connected because I'm lazy. So in the code they're generally one and the same.
    // The SettlementManager referencing to the manager and Heart referencing to the piece

    class SettlementManager : MonoBehaviour //SettleMan
    {

        public static List<SettlementManager> AllSettleMans = new List<SettlementManager>();


        private readonly TownshipManager m_tsManager;

        public ZDO myZDO;
        public ZDOID myZDOID;

        public List<ExpanderSoul> expanderSoulList = new List<ExpanderSoul>(); // list of Expanders connected to this SettlementManager

        public string settlementName
        {
            get { return myZDO.GetString("settlementName", "No-Name"); }
            set { myZDO.Set("settlementName", value); }
        }
        public float happiness
        {
            get { return myZDO.GetFloat("happiness"); }
            set { myZDO.Set("happiness", value); }
        }
        public int amount_villagers
        {
            get { return myZDO.GetInt("amount_villagers"); }
            set { myZDO.Set("amount_villagers", value); }
        }

        public Vector3 centerofSOI;
        public ExpanderSoul centerExpander;


        // called by Townshipmanager on load
        public SettlementManager( ZDO settleManZDO )
        {
            m_tsManager = TownshipManager.Instance;

            Jotunn.Logger.LogDebug("Constructing SettlementManager on load");
            myZDO = settleManZDO;
            myZDOID = myZDO.m_uid;


            Jotunn.Logger.LogDebug("my ExpanderSouls, Assemble");
            foreach (ExpanderSoul expandersoul in ExpanderSoul.AllExpanderSouls)
            {
                if(expandersoul.isActive)
                    if(expandersoul.isConnected)
                        if (expandersoul.parentSettleManZDOID == myZDOID)
                        {
                            //expanderSoulList.Add(expandersoul);
                            expandersoul.connectSettleMan(this, register:true);
                        }
            }
            Jotunn.Logger.LogDebug(settlementName + " has loaded " + expanderSoulList.Count() + " Souls");


            if( expanderSoulList.Count() == 0 )
            {
                Jotunn.Logger.LogWarning( settlementName + " has loaded no Souls. This SettleMan can't exist. Removing." );
                onDestroy();
            }


            //InvokeRepeating("think", 5f, 5f);

            calcCenterExpander(); // also sets the centerofSOI

            myZDO.m_persistent = true;// Doing this at the end; if a NRE shows up before this, the ZDO will be cleaned when the world closes
            AllSettleMans.Add(this);
        }


        // called when creating a new Settlemanager
        public SettlementManager( ExpanderSoul expanderSoul )
        {
            m_tsManager = TownshipManager.Instance;
            Jotunn.Logger.LogDebug("Constructing new SettlementManager");


            Jotunn.Logger.LogDebug("\t creating new ZDO for SettlementManager");
            myZDO = ZDOMan.instance.CreateNewZDO(new Vector3(0, -10000, 0));
            myZDO.m_persistent = true;
            myZDO.SetPrefab(m_tsManager.settlemanangerprefabname.GetStableHashCode() );


            Jotunn.Logger.LogDebug("\t populating new ZDO & stuff");
            myZDOID = myZDO.m_uid;

            settlementName = "No-Name";
            happiness = 0f;
            amount_villagers = 0;

            calcCenterExpander(); // also sets the centerofSOI

            Jotunn.Logger.LogDebug("Starting to think");
            //InvokeRepeating("think", 5f, 5f);

            AllSettleMans.Add(this);

            myZDO.m_persistent = true;// Doing this at the end; if a NRE shows up before this, the ZDO will be cleaned when the world closes
            Jotunn.Logger.LogDebug("Done creating new SettleMan");
        }


        // called when invalid or no ExpanderBodies/Souls to sustain it
        public void onDestroy()
        {
            myZDO.m_persistent = false;
            AllSettleMans.Remove(this);
        }



        /*
         */
        public void think()
        {
            //  only the server should run this
            if ( TownshipManager.IsServerorLocal() )
            {
                happiness += 1;
                Jotunn.Logger.LogDebug(settlementName + " thinks, therefore it is.");
            }
        }



        public bool isPosInThisSettlement( Vector3 pos )
        {
            foreach( ExpanderSoul expander in expanderSoulList)
            {
                if ( Vector3.Distance(expander.position, pos) <= 30 )
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
        public void checkConnectionsWeb()
        {
            lock(checkconnection_lock) // this is a major operation and nobody should access this file (or expanderList tbf) at this time.
            {
                if (expanderSoulList.Count() == 0)
                {
                    Jotunn.Logger.LogWarning("No point in checking connections if there's no Expanders left");
                    return;
                }

                int canidate_range = 2000;

                Jotunn.Logger.LogDebug("checking connections for " + settlementName);

                calcCenterExpander();

                // list of all the expanders that will be connected
                List<ExpanderSoul> new_expanderList = new List<ExpanderSoul>();

                // list of all expanders in a 2000 something range
                List<ExpanderSoul> canidateexpanderList = new List<ExpanderSoul>();


                // Don't really need to check for Expanders that would be an unreasonable distance way (and not active).
                // 10 bucks someone files a bug report about this.
                foreach (ExpanderSoul c_expander in ExpanderSoul.AllExpanderSouls)
                {
                    if (Vector3.Distance(c_expander.position, centerofSOI) <= canidate_range && c_expander.isActive)
                    {
                        canidateexpanderList.Add(c_expander);
                    }
                }

                Jotunn.Logger.LogDebug("Found nearby expanders: " + canidateexpanderList.Count());


                // the meat of the function
                foreach (ExpanderSoul n_expander in canidateexpanderList)
                {
                    if (Vector3.Distance(n_expander.position, centerExpander.position ) <= m_tsManager.connection_range && n_expander.isActive)
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
                            checkConnectionsWeb_R(n_expander, in canidateexpanderList, ref new_expanderList);
                        }
                    }
                }

                List<ExpanderSoul> old_expanderSouls = new List<ExpanderSoul>();


                Jotunn.Logger.LogDebug("Gathering old Expanders");
                foreach (ExpanderSoul expander in expanderSoulList)
                {
                    if( new_expanderList.Contains(expander) )
                    {
                        continue;
                    } else
                    {
                        old_expanderSouls.Add(expander);
                    }
                }

                Jotunn.Logger.LogDebug("Disconnecting old Expanders");
                // deactivate all the Expanders that aren't in the new list
                foreach (ExpanderSoul old_expander in old_expanderSouls)
                {
                    old_expander.changeConnection(toConnect: false, checkconnections: false);
                }


                Jotunn.Logger.LogDebug("Old: " + expanderSoulList.Count() + "| New: " + new_expanderList.Count());

                expanderSoulList.Clear();
                expanderSoulList.AddRange( new_expanderList );

                foreach (ExpanderSoul new_expander in new_expanderList)
                {
                    new_expander.changeConnection(toConnect: true, checkconnections: false);
                }

                Jotunn.Logger.LogDebug("current: " + expanderSoulList.Count() + "| New: " + new_expanderList.Count());

            }
        }

        private void checkConnectionsWeb_R(ExpanderSoul c_expander, in List<ExpanderSoul> localexpanderList, ref List<ExpanderSoul> new_expanderList)
        {
            foreach (ExpanderSoul n_expander in localexpanderList)
            {
                if (Vector3.Distance(n_expander.position, c_expander.position) <= m_tsManager.connection_range && n_expander.isActive) // this time test against c_expander *not* heart/m_piece.center
                {
                    if (new_expanderList.Contains(n_expander))
                    {
                        continue;
                    } else
                    {
                        new_expanderList.Add(n_expander);
                        checkConnectionsWeb_R(n_expander, in localexpanderList, ref new_expanderList);
                    }
                }
            }
        }

        public void calcCenterExpander()
        {
            float shortestDistancefromCOSOI = float.MaxValue;
            calcCenterofSOI();

            foreach (ExpanderSoul expander in expanderSoulList)
            {
                float distancefromCOSOI = Vector3.Distance(centerofSOI, expander.position);
                if (distancefromCOSOI <= shortestDistancefromCOSOI)
                {
                    centerExpander = expander;
                }
            }
        }

        private Vector3 calcCenterofSOI()
        {
            Vector3 temp = Vector3.zero;
            foreach (ExpanderSoul expander in expanderSoulList)
            {
                temp += expander.position;
            }

            return temp /= expanderSoulList.Count();
        }

        public void RegisterExpanderSoul( ExpanderSoul newsoul )
        {
            if( !expanderSoulList.Contains(newsoul) )
                expanderSoulList.Add(newsoul);
        }

        public void unRegisterExpanderSoul(ExpanderSoul oldsoul)
        {
            expanderSoulList.Remove(oldsoul);
            if( expanderSoulList.Count() == 0)
            {
                onDestroy();
            }
        }

        public static SettlementManager registerNewSettlement( ExpanderSoul firstExpanderSoul)
        {
            return new SettlementManager(firstExpanderSoul);
        }

        // 
        public static bool renameNamedSettlement( string oldname, string newname )
        {
            // TODO: make this an RPC call, and only led the admin rename

            foreach(SettlementManager setman in AllSettleMans)
            {
                if( setman.settlementName == oldname)
                {
                    setman.rename(newname);
                    Console.instance.Print("renamend settlement " + oldname + " to " + newname);
                    return true;
                }
            }

            Jotunn.Logger.LogDebug("Couldn't find a settlement with the name " + oldname );
            Console.instance.Print("Couldn't find a settlement with the name " + oldname);
            return false;
        }

        // 
        public static bool renameLocalSettlement(Vector3 pos, string newname)
        {
            // TODO: make this an RPC call, and only led the admin rename

            SettlementManager local_setman = SettlementManager.PosInWhichSettlement(pos);

            if ( !(local_setman is null) )
            {
                local_setman.rename(newname);
                Console.instance.Print("renamend local settlement " + local_setman.settlementName + " to " + newname);
                return true;
            }

            Jotunn.Logger.LogDebug("You're currently not in the vacinity a settlement");
            Console.instance.Print("You're currently not in the vacinity a settlement");
            return false;
        }

        public void rename(string newname)
        {
            Jotunn.Logger.LogDebug("renamend local settlement " + settlementName + " to " + newname);
            settlementName = newname;
            // This is dumb, but have to do it for now
            foreach (ExpanderSoul soul in expanderSoulList)
            {
                soul.settlementName = newname;
                return;
            }
        }


        public static SettlementManager PosInWhichSettlement(Vector3 pos)
        {
            foreach (SettlementManager settlement in AllSettleMans)
            {
                if (settlement.isPosInThisSettlement(pos))
                {
                    Jotunn.Logger.LogDebug("Found a settlement at this pos");
                    return settlement;
                }
            }
            Jotunn.Logger.LogDebug("Did not find a settlement at this pos");
            return null; // this is valid, means Pos isn't in a settlement
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
