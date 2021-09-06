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


    class SettlementManager //SettleMan
    {

        public static List<SettlementManager> AllSettleMans = new List<SettlementManager>();

        private readonly TownshipManager m_tsManager;
        private readonly ZNetView m_nview;

        public ZDO myZDO;
        public ZDOID myZDOID;

        public GameObject gameObject;

        public List<Expander> expanderList = new List<Expander>(); // list of Expanders connected to this SettlementManager

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

        public Guid myGuid
        {
            get { return Guid.ParseExact( myZDO.GetString("myGuid", Guid.NewGuid().ToString("D")), "D"); }
            set { myZDO.Set("myGuid", value.ToString("D") ); }
        }

        public Vector3 centerofSOI;
        public Expander centerExpander;


        /// <summary>
        /// called by Townshipmanager on load
        /// </summary>
        /// <param name="settleManZDO"></param>
        public SettlementManager( ZDO settleManZDO )
        {
            m_tsManager = TownshipManager.Instance;

            Jotunn.Logger.LogDebug("Constructing SettlementManager on load");
            gameObject = UnityEngine.Object.Instantiate(new GameObject(m_tsManager.settlemanangerprefabname, new Type[] { typeof(ZNetView) }));
            myZDO = gameObject.GetComponent<ZNetView>().GetZDO();

            myZDOID = myZDO.m_uid;


            Jotunn.Logger.LogDebug("my ExpanderSouls, Assemble");
            foreach (Expander expandersoul in Expander.AllExpanders)
            {
                if(expandersoul.isActive)
                    if(expandersoul.isConnected)
                        if (expandersoul.parentSettleManZDOID == myZDOID)
                        {
                            //expanderSoulList.Add(expandersoul);
                            expandersoul.connectSettleMan(this, register:true);
                        }
            }
            Jotunn.Logger.LogDebug(settlementName + " has loaded " + expanderList.Count() + " Souls");


            if( expanderList.Count() == 0 )
            {
                Jotunn.Logger.LogWarning( settlementName + " has loaded no Souls. This SettleMan can't exist. Removing." );
                myZDO.m_persistent = false;
                return;
            } else
            {
                myZDO.m_persistent = true;
            }


            //InvokeRepeating("think", 5f, 5f);

            calcCenterExpander(); // also sets the centerofSOI

            AllSettleMans.Add(this);
        }


        /// <summary>
        /// called when creating a new Settlemanager
        /// </summary>
        /// <param name="expanderSoul"></param>
        public SettlementManager( Expander expanderSoul )
        {
            m_tsManager = TownshipManager.Instance;
            Jotunn.Logger.LogDebug("Constructing new SettlementManager");


            Jotunn.Logger.LogDebug("\t creating new ZDO for SettlementManager");
            gameObject = UnityEngine.Object.Instantiate(new GameObject(m_tsManager.settlemanangerprefabname, new Type[] { typeof(ZNetView) }));
            myZDO = gameObject.GetComponent<ZNetView>().GetZDO();
            myZDO.m_persistent = true;


            Jotunn.Logger.LogDebug("\t populating new ZDO & stuff");
            myZDOID = myZDO.m_uid;
            myGuid = Guid.NewGuid();
            settlementName = "No-Name";
            happiness = 0f;
            amount_villagers = 0;

            calcCenterExpander(); // also sets the centerofSOI

            Jotunn.Logger.LogDebug("Starting to think");
            //InvokeRepeating("think", 5f, 5f);

            AllSettleMans.Add(this);

            Jotunn.Logger.LogDebug("Done creating new SettleMan");
        }


        // called when invalid or no ExpanderBodies/Souls to sustain it
        public void onDestroy()
        {
            myZDO.m_persistent = false;
            //SettlementManager.AllSettleMans.Remove(this);
        }



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
            foreach( Expander expander in expanderList)
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
                if (expanderList.Count() == 0)
                {
                    Jotunn.Logger.LogWarning("No point in checking connections if there's no Expanders left");
                    return;
                }

                int canidate_range = 2000;

                Jotunn.Logger.LogDebug("checking connections for " + settlementName);

                calcCenterExpander();

                // list of all the expanders that will be connected
                List<Expander> new_expanderList = new List<Expander>();

                // list of all expanders in a 2000 something range
                List<Expander> canidateexpanderList = new List<Expander>();


                // Don't really need to check for Expanders that would be an unreasonable distance way (and not active).
                // 10 bucks someone files a bug report about this.
                foreach (Expander c_expander in Expander.AllExpanders)
                {
                    if (Vector3.Distance(c_expander.position, centerofSOI) <= canidate_range && c_expander.isActive)
                    {
                        canidateexpanderList.Add(c_expander);
                    }
                }

                Jotunn.Logger.LogDebug("Found nearby expanders: " + canidateexpanderList.Count());


                // the meat of the function
                foreach (Expander n_expander in canidateexpanderList)
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

                List<Expander> old_expanderSouls = new List<Expander>();


                Jotunn.Logger.LogDebug("Gathering old Expanders");
                foreach (Expander expander in expanderList)
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
                foreach (Expander old_expander in old_expanderSouls)
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

        private void checkConnectionsWeb_R(Expander c_expander, in List<Expander> localexpanderList, ref List<Expander> new_expanderList)
        {
            foreach (Expander n_expander in localexpanderList)
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

            foreach (Expander expander in expanderList)
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
            foreach (Expander expander in expanderList)
            {
                temp += expander.position;
            }

            return temp /= expanderList.Count();
        }

        public void RegisterExpanderSoul( Expander newsoul )
        {
            if( !expanderList.Contains(newsoul) )
                expanderList.Add(newsoul);
        }

        public void unRegisterExpanderSoul(Expander oldsoul)
        {
            expanderList.Remove(oldsoul);
            if( expanderList.Count() == 0)
            {
                onDestroy();
            }
        }

        public static SettlementManager registerNewSettlement( Expander firstExpanderSoul)
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
            foreach (Expander soul in expanderList)
            {
                soul.settlementName = newname;
                return;
            }
        }

        public static void printAllSettlements()
        {
            foreach (SettlementManager setman in AllSettleMans)
            {
                Console.instance.Print(setman.settlementName + "\n");
            }
            List<ZDO> SettlementManagerZDOs = new List<ZDO>();
            ZDOMan.instance.GetAllZDOsWithPrefab(TownshipManager.Instance.settlemanangerprefabname, SettlementManagerZDOs);
            Console.instance.Print("Loading " + SettlementManagerZDOs.Count() + " SettleManager ZDO's from ZDOMan");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setmanname"></param>
        /// <returns></returns>
        public static SettlementManager GetSetManByName(string setmanname)
        {
            foreach(SettlementManager setman in AllSettleMans)
            {
                if (setman.settlementName == setmanname)
                    return setman;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setmanZDOID"></param>
        /// <returns></returns>
        public static SettlementManager GetSetManByZDOID(ZDOID setmanZDOID)
        {
            foreach (SettlementManager setman in AllSettleMans)
            {
                if (setman.myZDOID == setmanZDOID)
                    return setman;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setmanZDOID"></param>
        /// <returns></returns>
        public static SettlementManager GetSetManByGuid( Guid setmanguid )
        {
            foreach (SettlementManager setman in AllSettleMans)
            {
                if (setman.myGuid == setmanguid)
                    return setman;
            }
            return null;
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
