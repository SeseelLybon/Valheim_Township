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
        public const string RegisteredExpandersIDs = "RegisteredExpandersIDs";

        public static List<SettlementManager> AllSettleMans = new List<SettlementManager>();
        public List<Expander> loadedExpanders = new List<Expander>();
        public HashSet<ZDOID> craftingstationIDset = new HashSet<ZDOID>();

        private readonly TownshipManager m_tsManager;
        //private readonly ZNetView m_nview;

        public ZDO myZDO;
        public ZDOID myZDOID;

        public GameObject gameObject;

        //public List<Expander> expanderList = new List<Expander>(); // list of connected Expanders that are currently loaded

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
        public Vector3 centerofSOI
        {
            get { return myZDO.GetVec3("centerofSOI", Vector3.zero ); }
            set { myZDO.Set("centerofSOI", value); }
        }

        public bool hasWorkbench
        {
            get { return myZDO.GetBool("hasWorkbench", false); }
            set { myZDO.Set("hasWorkbench", value); }
        }
        public int amountWorkbenches
        {
            get { return myZDO.GetInt("amountWorkbenches"); }
            set { myZDO.Set("amountWorkbenches", value); }
        }
        public bool hasForge
        {
            get { return myZDO.GetBool("hasForge", false); }
            set { myZDO.Set("hasForge", value); }
        }
        public int amountForges
        {
            get { return myZDO.GetInt("amountForges"); }
            set { myZDO.Set("amountForges", value); }
        }
        public bool hasStonecutter
        {
            get { return myZDO.GetBool("hasStonecutter", false); }
            set { myZDO.Set("hasStonecutter", value); }
        }
        public int amountStonecutters
        {
            get { return myZDO.GetInt("amountStonecutters"); }
            set { myZDO.Set("amountStonecutters", value); }
        }
        public bool hasArtisanStation
        {
            get { return myZDO.GetBool("hasArtisanStation", false); }
            set { myZDO.Set("hasArtisanStation", value); }
        }
        public int amountArtisanStations
        {
            get { return myZDO.GetInt("amountArtisanStations"); }
            set { myZDO.Set("amountArtisanStations", value); }
        }

        public ZDOID centerExpanderID;

        /// <summary>
        /// called by Townshipmanager on load
        /// </summary>
        /// <param name="settleManZDO"></param>
        public SettlementManager( ZDO settleManZDO )
        {
            m_tsManager = TownshipManager.Instance;

            Jotunn.Logger.LogDebug("Constructing SettlementManager on load");
            //gameObject = UnityEngine.Object.Instantiate(new GameObject(m_tsManager.settlemanangerprefabname, new Type[] { typeof(ZNetView) }));
            //myZDO = gameObject.GetComponent<ZNetView>().GetZDO();
            myZDO = settleManZDO;

            myZDOID = myZDO.m_uid;

            //InvokeRepeating("think", 5f, 5f);

            calcCenterExpander(); // also sets the centerofSOI

            AllSettleMans.Add(this);
            myZDO.m_persistent = true;
        }


        /// <summary>
        /// called when creating a new Settlemanager
        /// </summary>
        /// <param name="expanderSoul"></param>
        public SettlementManager( Expander expander )
        {
            m_tsManager = TownshipManager.Instance;
            Jotunn.Logger.LogDebug("Constructing new SettlementManager");


            Jotunn.Logger.LogDebug("\t creating new ZDO for SettlementManager");
            gameObject = UnityEngine.Object.Instantiate(new GameObject(m_tsManager.settlemanangerprefabname, new Type[] { typeof(ZNetView) }));
            myZDO = gameObject.GetComponent<ZNetView>().GetZDO();
            myZDO.m_persistent = true;


            Jotunn.Logger.LogDebug("\t populating new ZDO & stuff");
            myZDOID = myZDO.m_uid;
            settlementName = "No-Name";
            happiness = 0f;
            amount_villagers = 0;

            ZDOIDSet temp = new ZDOIDSet();
            temp.Add(expander.myID);
            myZDO.Set(RegisteredExpandersIDs, temp.ToZPackage().GetArray() );

            calcCenterExpander(); // also sets the centerofSOI

            Jotunn.Logger.LogDebug("Starting to think");
            //InvokeRepeating("think", 5f, 5f);

            AllSettleMans.Add(this);

            Jotunn.Logger.LogDebug("Done creating new SettleMan");
        }


        // called when invalid or no ExpanderBodies/Souls to sustain it
        public void onDestroy()
        {
            ZDOMan.instance.DestroyZDO(myZDO);
            AllSettleMans.Remove(this);
            // destroy(this);
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
            ZDOIDSet registeredexpanders = GetRegisteredExpanders();

            foreach (ZDOID expanderID in registeredexpanders)
            {
                if ( Vector3.Distance(GetZDO(expanderID).GetVec3(Expander.position, Vector3.zero), pos) <= 30 )
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
            lock (checkconnection_lock) // this is a major operation and nobody should access this file (or expanderList tbf) at this time.
            {
                ZDOIDSet registeredexpanders = GetRegisteredExpanders();
                if (registeredexpanders.Count() == 0)
                {
                    Jotunn.Logger.LogWarning("No point in checking connections if there's no Expanders left");
                    return;
                }

                int canidate_range = 2000;

                Jotunn.Logger.LogDebug("checking connections for " + settlementName);

                calcCenterExpander();

                // list of all the expanders that will be connected
                List<ZDOID> new_expanderIDList = new List<ZDOID>();

                // list of all expanders in a 2000 something range
                List<ZDOID> canidateexpanderIDList = new List<ZDOID>();


                // Don't really need to check for Expanders that would be an unreasonable distance way (and not active).
                // 10 bucks someone files a bug report about this.
                foreach (ZDOID c_expanderID in registeredexpanders)
                {
                    if (Vector3.Distance(ZDOMan.instance.GetZDO(c_expanderID).GetVec3(Expander.position, Vector3.zero), centerofSOI) <= canidate_range && ZDOMan.instance.GetZDO(c_expanderID).GetBool(Expander.isActive))
                    {
                        canidateexpanderIDList.Add(c_expanderID);
                    }
                }

                Jotunn.Logger.LogDebug("Found nearby expanders: " + canidateexpanderIDList.Count());


                // the meat of the function
                foreach (ZDOID n_expanderID in canidateexpanderIDList)
                {
                    if (Vector3.Distance(ZDOMan.instance.GetZDO(n_expanderID).GetVec3(Expander.position, Vector3.zero), ZDOMan.instance.GetZDO(centerExpanderID).GetVec3(Expander.position, Vector3.zero)) <= m_tsManager.connection_range && ZDOMan.instance.GetZDO(n_expanderID).GetBool(Expander.isActive))
                    {
                        if (new_expanderIDList.Contains(n_expanderID))
                        {
                            Jotunn.Logger.LogDebug("Expander was already in list");
                            continue;
                        }
                        else
                        {
                            Jotunn.Logger.LogDebug("Adding expander and going deeper");
                            new_expanderIDList.Add(n_expanderID);
                            checkConnectionsWeb_R(n_expanderID, in canidateexpanderIDList, ref new_expanderIDList);
                        }
                    }
                }

                List<ZDOID> old_expanderIDs = new List<ZDOID>();


                Jotunn.Logger.LogDebug("Gathering old Expanders");
                foreach (ZDOID expanderID in registeredexpanders)
                {
                    if( new_expanderIDList.Contains(expanderID) )
                    {
                        continue;
                    } else
                    {
                        old_expanderIDs.Add(expanderID);
                    }
                }

                Jotunn.Logger.LogDebug("Disconnecting old Expanders");
                // deactivate all the Expanders that aren't in the new list
                foreach (ZDOID old_expander in old_expanderIDs)
                {
                    ZDOMan.instance.GetZDO(old_expander).Set(Expander.isConnected, false);
                    ZDOMan.instance.GetZDO(old_expander).Set(Expander.parentSettleManID, ZDOID.None);
                }


                Jotunn.Logger.LogDebug("Old: " + registeredexpanders.Count() + "| New: " + new_expanderIDList.Count());

                registeredexpanders.Clear();
                registeredexpanders.UnionWith( new_expanderIDList );

                foreach (ZDOID new_expanderID in new_expanderIDList)
                {
                    ZDOMan.instance.GetZDO(new_expanderID).Set(Expander.isConnected, true);
                    ZDOMan.instance.GetZDO(new_expanderID).Set(Expander.parentSettleManID, myZDOID);
                }

                myZDO.Set(RegisteredExpandersIDs, registeredexpanders.ToZPackage().GetArray());
                Jotunn.Logger.LogDebug("current: " + registeredexpanders.Count() + "| New: " + new_expanderIDList.Count());
            }
        }

        private void checkConnectionsWeb_R(ZDOID c_expander, in List<ZDOID> localexpanderList, ref List<ZDOID> new_expanderList)
        {
            foreach (ZDOID n_expanderID in localexpanderList)
            {
                if (Vector3.Distance(ZDOMan.instance.GetZDO(n_expanderID).GetVec3(Expander.position, Vector3.zero), ZDOMan.instance.GetZDO(n_expanderID).GetVec3(Expander.position, Vector3.zero)) <= m_tsManager.connection_range &&
                    ZDOMan.instance.GetZDO(n_expanderID).GetBool(Expander.isActive) ) // this time test against c_expander *not* heart/m_piece.center
                {
                    if (new_expanderList.Contains(n_expanderID))
                    {
                        continue;
                    } else
                    {
                        new_expanderList.Add(n_expanderID);
                        checkConnectionsWeb_R(n_expanderID, in localexpanderList, ref new_expanderList);
                    }
                }
            }
        }

        public void calcCenterExpander()
        {
            //bugged
            ZDOIDSet registeredexpanders = GetRegisteredExpanders();
            centerExpanderID = registeredexpanders.First<ZDOID>();
            /*
            float shortestDistancefromCOSOI = float.MaxValue;
            calcCenterofSOI();

            ZDOIDSet registeredexpanders = GetRegisteredExpanders();

            foreach (ZDOID expander in registeredexpanders)
            {
                float distancefromCOSOI = Vector3.Distance(centerofSOI, ZDOMan.instance.GetZDO(expander).GetVec3(Expander.position, Vector3.zero));
                if (distancefromCOSOI <= shortestDistancefromCOSOI)
                {
                    centerExpanderID = expander;
                }
            }
            */
        }

        private Vector3 calcCenterofSOI()
        {
            //bugged
            ZDOIDSet registeredexpanders = GetRegisteredExpanders();
            return GetZDO(registeredexpanders.First<ZDOID>()).GetVec3(Expander.position, Vector3.zero);
            /*
            ZDOIDSet registeredexpanders = GetRegisteredExpanders();

            Vector3 temp = Vector3.zero;
            foreach (ZDOID expanderID in registeredexpanders)
            {
                temp += GetZDO(expanderID).GetVec3(Expander.position, Vector3.zero);
                // note Vector3 shouldn't be called. It'd mean that it wasn't set and isn't the issue here.
            }

            return temp /= registeredexpanders.Count();
            */
        }

        public void RegisterExpanderSoul( ZDOID newsoulID )
        {
            ZDOIDSet registeredexpanders = GetRegisteredExpanders();

            if ( !registeredexpanders.Contains(newsoulID) )
                registeredexpanders.Add(newsoulID);

            myZDO.Set(RegisteredExpandersIDs, registeredexpanders.ToZPackage().GetArray());
        }

        public void unRegisterExpanderSoul(ZDOID oldsoulID)
        {
            ZDOIDSet registeredexpanders = GetRegisteredExpanders();

            registeredexpanders.Remove(oldsoulID);

            if(registeredexpanders.Count() == 0)
            {
                onDestroy();
            }

            myZDO.Set(RegisteredExpandersIDs, registeredexpanders.ToZPackage().GetArray());
        }

        public static SettlementManager registerNewSettlement( Expander firstExpander)
        {
            return new SettlementManager(firstExpander);
        }


        /// <summary>
        /// returns a ZDOIDset with all ZDOID's of registered expanders
        /// </summary>
        /// <returns></returns>
        public ZDOIDSet GetRegisteredExpanders()
        {
            byte[] data = myZDO.GetByteArray(RegisteredExpandersIDs);
            if(data == null)
            {
                return null;
            }
            return ZDOIDSet.From(new ZPackage(data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expander"></param>
        public void RemoveExpanderFromRegisteredExpanders(Expander expander)
        {
            ZDOID parentSettleManZDOID = expander.myZDO.GetZDOID(Expander.parentSettleManID);
            if(parentSettleManZDOID != myZDOID)
            {
                // this settleman isn't connected to expander
                return;
            }

            ZDOIDSet registeredexpanders = GetRegisteredExpanders();
            registeredexpanders?.Remove(expander.myID);
            if(registeredexpanders == null || registeredexpanders.Count() == 0)
            {
                //remove settlement as there's no expanders left to sustain it
            } else
            {
                myZDO.Set(RegisteredExpandersIDs, registeredexpanders.ToZPackage().GetArray() );
            }
        }

        /// 
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="newname"></param>
        /// <returns></returns>
        public static bool renameLocalSettlement(Vector3 pos, string newname)
        {
            // TODO: make this an RPC call, and only led the admin rename

            SettlementManager local_setman = SettlementManager.PosInWhichSettlement(pos);

            if ( !(local_setman is null) )
            {
                Console.instance.Print("renamend local settlement " + local_setman.settlementName + " to " + newname);
                local_setman.rename(newname);
                return true;
            }

            Console.instance.Print("You're currently not in the vacinity a settlement");
            Jotunn.Logger.LogDebug("You're currently not in the vacinity a settlement");
            return false;
        }

        public void rename(string newname)
        {
            Jotunn.Logger.LogDebug("renamend local settlement " + settlementName + " to " + newname);
            settlementName = newname;
            // This is dumb, but have to do it for now
            ZDOIDSet registeredexpanders = GetRegisteredExpanders();

            foreach (ZDOID expanderID in registeredexpanders)
            {
                ZDO expanderZDO = ZDOMan.instance.GetZDO(expanderID);
                expanderZDO.Set(Expander.settlementName, newname);
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

        public static ZDO GetZDO(ZDOID zdoid)
        {
            return ZDOMan.instance.GetZDO(zdoid);
        }

        public void enableCraftinStationProxy(string craftingstation, ZDOID CSID)
        {
            Jotunn.Logger.LogDebug(settlementName + ": enabling Workbenchproxy " + craftingstation);

            if (!craftingstationIDset.Add(CSID))
                return;
            

            if (craftingstation == "$piece_workbench")
            {
                amountWorkbenches += 1;
                hasWorkbench = true;
            }
            else if (craftingstation == "$piece_forge")
            {
                amountForges += 1;
                hasForge = true;
            }
            else if (craftingstation == "$piece_stonecutter")
            {
                amountStonecutters += 1;
                hasStonecutter = true;
            }
            else if (craftingstation == "$piece_artisanstation")
            {
                amountArtisanStations += 1;
                hasArtisanStation = true;
            }

            foreach(Expander expander in loadedExpanders)
            {
                expander.changeProxyWorkbenches();
            }
        }

        public void disableCraftinStationProxy(string craftingstation, ZDOID CSID)
        {
            Jotunn.Logger.LogDebug(settlementName + ": disabling Workbenchproxy " + craftingstation);

            if (!craftingstationIDset.Remove(CSID))
                return;

            if (craftingstation == "$piece_workbench")
            {
                if (amountWorkbenches >= 1)
                    amountWorkbenches -= 1;

                if (amountWorkbenches <= 0)
                    hasWorkbench = false;
            }
            else if (craftingstation == "$piece_forge")
            {
                if (amountForges >= 1)
                    amountForges -= 1;

                if (amountForges <= 0)
                    hasForge = false;
            }
            else if (craftingstation == "$piece_stonecutter")
            {
                if (amountStonecutters >= 1)
                    amountStonecutters -= 1;

                if (amountStonecutters <= 0)
                    hasStonecutter = false;
            }
            else if (craftingstation == "$piece_artisanstation")
            {
                if ( amountArtisanStations >= 1)
                    amountArtisanStations -= 1;

                if (amountArtisanStations <= 0)
                    hasArtisanStation = false;
            }

            foreach (Expander expander in loadedExpanders)
            {
                expander.changeProxyWorkbenches();
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
