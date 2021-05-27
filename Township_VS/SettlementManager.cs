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


        private TownshipManager m_tsManager;

        public ZDOID myZDOID;
        public ZDO myZDO;
        public List<ExpanderSoul> expanderSoulList = new List<ExpanderSoul>(); // list of expander totems connected to this SettlementManager

        public string settlementName
        {
            get { return myZDO.GetString("settlementName", "Elktown"); }
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
        public ExpanderBody centerExpander;


        // called by Townshipmanager on load
        public SettlementManager(ZDO settleManZDO)
        {
            myZDO = settleManZDO;
            myZDOID = myZDO.m_uid;


            List<ZDO> extenderSoulZDOs = new List<ZDO>();
            ZDOMan.instance.GetAllZDOsWithPrefab(m_tsManager.expandersoulprefabname, extenderSoulZDOs);
            foreach (ZDO extendersoulzdo in extenderSoulZDOs)
            {
                if( extendersoulzdo.GetZDOID("settlementZDOID") == myZDOID)
                {
                    expanderSoulList.Add(new ExpanderSoul(extendersoulzdo) );
                }
            }


            InvokeRepeating("think", 5f, 5f);
            centerofSOI = calcCenterofSOI();
        }

        // called when creating a new Settlemanager
        public SettlementManager()
        {
            myZDO = ZDOMan.instance.CreateNewZDO(Vector3.zero);
            myZDO.m_persistent = true;
            myZDO.SetPrefab( "Anima".GetStableHashCode() );



            InvokeRepeating("think", 5f, 5f);
            centerofSOI = calcCenterofSOI();
        }



        /*
         */
        public void think()
        {
            //  only the server should run this
            if (Jotunn.ZNetExtension.IsLocalInstance(ZNet.instance) || Jotunn.ZNetExtension.IsServerInstance(ZNet.instance))
            {
                myZDO.Set("TestNumber", myZDO.GetInt("TestNumber") + 1);
                Jotunn.Logger.LogDebug(myZDO.GetString("settlementName") + " thinks, therefore it is.");
            }
        }



        public bool isPosInThisSettlement( Vector3 pos )
        {
            foreach( ExpanderBody totem in expanderList )
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
        public void checkConnectionsWeb()
        {
            lock(checkconnection_lock) // this is a major operation and nobody should access this file (or expanderList tbf) at this time.
            {
                int canidate_range = 2000;

                Jotunn.Logger.LogDebug("checking connections for " + settlementName);

                calcCenterExpander();

                // list with all the expanders in range that are active
                List<ExpanderSoul> new_expanderList = new List<ExpanderSoul>();

                // list of all expanders in a 2000 something range
                List<ExpanderSoul> canidateexpanderList = new List<ExpanderSoul>();


                // Don't really need to check for Expanders that would be an unreasonable distance way (and not active).
                // 10 bucks someone files a bug report about this.
                foreach (ExpanderSoul c_expander in ExpanderSoul.m_AllExpanders)
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
                    if (Vector3.Distance(n_expander.m_piece.GetCenter(), centerExpander.m_piece.GetCenter() ) <= m_tsManager.connection_range && n_expander.isActive)
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


                // deactivate all the Expanders that aren't in the new list
                foreach(ExpanderSoul old_expander in expanderSoulList)
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

        private void checkConnectionsWeb_R(ExpanderBody c_expander, in List<ExpanderBody> localexpanderList, ref List<ExpanderBody> new_expanderList)
        {
            foreach (ExpanderBody n_expander in localexpanderList)
            {
                if (Vector3.Distance(n_expander.m_piece.GetCenter(), c_expander.m_piece.GetCenter()) <= m_tsManager.connection_range && n_expander.isActive) // this time test against c_expander *not* heart/m_piece.center
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

            foreach (ExpanderBody expander in expanderSoulList)
            {
                float distancefromCOSOI = Vector3.Distance(centerofSOI, expander.m_piece.GetCenter());
                if (distancefromCOSOI <= shortestDistancefromCOSOI)
                {
                    centerExpander = expander;
                }
            }
        }

        private Vector3 calcCenterofSOI()
        {
            Vector3 temp = Vector3.zero;
            foreach (ExpanderBody expander in expanderSoulList)
            {
                temp += expander.m_piece.GetCenter();
            }

            return temp /= expanderSoulList.Count();
        }

        public void RegisterExpanderSoul( ExpanderSoul newsoul, ZDO newsoulZDO )
        {
            expanderSoulList.Add(newsoul);
        }

        public void unRegisterExpanderSoul(ExpanderSoul oldsoul)
        {
            expanderSoulList.Remove(oldsoul);
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
