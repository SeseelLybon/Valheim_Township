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
    // This class is for use on the Expander totems.
    // Their main purpose is to expand the SettlementManager's sphere of unfluence.
    // Most functions are about passing questions on to the SettlementManager.

    class Expander : MonoBehaviour, Hoverable, Interactable
    {

        public Piece m_piece;
        public TownshipManager m_tsManager;
        public ZNetView m_nview;

        public SettlementManager parentSettleMan; // the SettlementManager that is connected to this expander
        public ZDO parentSettleManZDO; // the SettlementManager that is connected to this expander

        public static List<Expander> AllExpanders = new List<Expander>();

        public ZDO myZDO;
        public ZDOID myID;

        public const string isActive = "isActive";
        public const string isConnected = "isConnected";
        public const string settlementName = "settlementName";
        public const string position = "position";
        public const string parentSettleManID = "parentSettleManID";
        public const string showOnMinimap = "showOnMinimap";

        bool isPlaced = false;

        public GameObject workbenchProxy;
        public GameObject stonecutterProxy;
        public GameObject forgeProxy;
        public GameObject artisanProxy;
        //public GameObject wardProxy;


        public void Start()
        {
            Jotunn.Logger.LogDebug("ExpanderBody.Start()");

            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
            m_tsManager = TownshipManager.instance;


            if (m_piece.IsPlacedByPlayer())
            {
                PopulateProxies();
                isPlaced = true;
                Jotunn.Logger.LogDebug("ExpanderBody is placed by player");
                if (TownshipManager.IsServerorLocal())
                {
                    Jotunn.Logger.LogDebug("Doing stuff to expander that was placed by a player");

                    GetComponent<WearNTear>().m_onDestroyed += OnDestroyed;

                    AllExpanders.Add(this);
                    myZDO = m_nview.GetZDO();
                    myID = myZDO.m_uid;

                    //m_nview.Register<bool>("changeActive", RPC_changeActive);
                    //m_nview.Register<bool, bool>("changeConnection", RPC_changeConnection);
                    //m_nview.Register("onDestroy", RPC_OnDestroy);
                    //m_nview.Register("onDestroyed", RPC_OnDestroyed);

                    Jotunn.Logger.LogDebug("Expander ZDOID: " + myID);

                    myZDO.Set("position", m_piece.GetCenter() );

                    if (myZDO.GetBool(isConnected))
                    {
                        SettlementManager temp = SettlementManager.GetSetManByZDOID( myZDO.GetZDOID(parentSettleManID) );
                        if(!(temp == null))
                        {
                            connectSettleMan(temp, true);
                        } else
                        {
                            disConnectSettleMan(false);
                        }
                    }

                    // how to register RPC
                    //m_nview.Register<bool>("changeActive", RPC_changeActive);
                    // how to invoke RPC
                    // m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", isActive);
                    myZDO.m_persistent = true;// Doing this at the end; if a NRE shows up before this, the ZDO will be cleaned when the world closes
                    Jotunn.Logger.LogDebug("Done doing stuff to ExpanderBody\n");
                }
            }
        }

        /// <summary>
        /// This is a *terrible* way of grabbing them all.
        /// </summary>
        private void findNearbyWorkbenches()
        {
            var pos = myZDO.GetVec3(position, Vector3.zero);
            //Jotunn.Logger.LogDebug("Reminder that findNearbyWorkbenches is called multipletimes at once");

            List<ZDO> CS_ZDOs = new List<ZDO>();
            ZDOMan.instance.GetAllZDOsWithPrefab("piece_workbench", CS_ZDOs);
            foreach(ZDO zdo in CS_ZDOs)
            {
                if(zdo.m_distant == false)
                {
                    if( Vector3.Distance(zdo.m_position, pos) <= TownshipManager.instance.Extender_buildrange){
                        parentSettleMan.enableCraftinStationProxy("$piece_workbench", zdo.m_uid);
                    }
                }
            }

            CS_ZDOs.Clear();
            ZDOMan.instance.GetAllZDOsWithPrefab("forge", CS_ZDOs);
            foreach (ZDO zdo in CS_ZDOs)
            {
                if (zdo.m_distant == false)
                {
                    if (Vector3.Distance(zdo.m_position, pos) <= TownshipManager.instance.Extender_buildrange)
                    {
                        parentSettleMan.enableCraftinStationProxy("$piece_forge", zdo.m_uid);
                    }
                }
            }

            CS_ZDOs.Clear();
            ZDOMan.instance.GetAllZDOsWithPrefab("piece_stonecutter", CS_ZDOs);
            foreach (ZDO zdo in CS_ZDOs)
            {
                if (zdo.m_distant == false)
                {
                    if (Vector3.Distance(zdo.m_position, pos) <= TownshipManager.instance.Extender_buildrange)
                    {
                        parentSettleMan.enableCraftinStationProxy("$piece_stonecutter", zdo.m_uid);
                    }
                }
            }

            CS_ZDOs.Clear();
            ZDOMan.instance.GetAllZDOsWithPrefab("piece_artisanstation", CS_ZDOs);
            foreach (ZDO zdo in CS_ZDOs)
            {
                if (zdo.m_distant == false)
                {
                    if (Vector3.Distance(zdo.m_position, pos) <= TownshipManager.instance.Extender_buildrange)
                    {
                        parentSettleMan.enableCraftinStationProxy("$piece_artisanstation", zdo.m_uid);
                    }
                }
            }
        }

        private void PopulateProxies()
        {
            workbenchProxy = new GameObject();
            workbenchProxy.transform.position = m_piece.GetCenter();
            var wbPcs = workbenchProxy.AddComponent<CraftingStation>();
            wbPcs.m_name = "$piece_workbench";
            //wbPcs.m_nview.GetZDO().m_prefab = "proxy_workbench".GetStableHashCode();

            forgeProxy = new GameObject();
            forgeProxy.transform.position = m_piece.GetCenter();
            var fPcs = forgeProxy.AddComponent<CraftingStation>();
            fPcs.m_name = "$piece_forge";
            //fPcs.m_nview.GetZDO().m_prefab = "proxy_workbench".GetStableHashCode();

            stonecutterProxy = new GameObject();
            stonecutterProxy.transform.position = m_piece.GetCenter();
            var stPcs = stonecutterProxy.AddComponent<CraftingStation>();
            stPcs.m_name = "$piece_stonecutter";
            //stPcs.m_nview.GetZDO().m_prefab = "proxy_workbench".GetStableHashCode();

            artisanProxy = new GameObject();
            artisanProxy.transform.position = m_piece.GetCenter();
            var atPcs = artisanProxy.AddComponent<CraftingStation>();
            atPcs.m_name = "$piece_artisanstation";
            //atPcs.m_nview.GetZDO().m_prefab = "proxy_workbench".GetStableHashCode();

            //wardProxy = new GameObject();
            //wardProxy.transform.position = m_piece.GetCenter();
            //var wPcs = wardProxy.AddComponent<PrivateArea>();
            //atPcs.m_name = "$piece_ward";
        }


        //int updateinterval = 1; //seconds
        //float updatenextTime = 0;
        public void Update()
        {

            /*
            if (Time.time >= updatenextTime)
            {
                if (isPlaced && myZDO.GetBool(isActive))
                {
                    if (myZDO.GetBool(isConnected) && parentSettleMan == null)
                    {
                        foreach (SettlementManager settleman in SettlementManager.AllSettleMans)
                        {
                            if (settleman.myZDOID == myZDO.GetZDOID(parentSettleManID))
                            {
                                connectSettleMan(settleman, true);
                            }
                        }
                    }
                }
                updatenextTime += updateinterval;
            }
            */
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        // Called when the piece is destroyed ingame
        public void OnDestroyed()
        {
            if (m_piece.IsPlacedByPlayer())
            {
                Jotunn.Logger.LogDebug("ExpanderBody.OnDestroyed()");
                AllExpanders.Remove(this);
                if (myZDO.GetBool(isConnected))
                {
                    //changeConnection(false, true);
                    var oldParentsettlement = SettlementManager.GetSetManByZDOID(myZDO.GetZDOID(parentSettleManID));
                    oldParentsettlement.loadedExpanders.Remove(this);
                    oldParentsettlement.unRegisterExpander(myID, this);
                }
            }
        }

        /// <summary>
        /// Called when the object is destroyed by unloading
        /// See OnDestroyed for when the object is destroyed by damage/deconstructed
        /// </summary>
        public void OnDestroy()
        {
            Jotunn.Logger.LogDebug("ExpanderBody.OnDestroy()");

            if (isPlaced)
            {
                AllExpanders.Remove(this);
                if (parentSettleMan != null)
                {
                    parentSettleMan.loadedExpanders.Remove(this);
                }
            }
        }

        /* Function is now only for making active or not.
          *  Will later be used to call the GUI
          */
        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            Jotunn.Logger.LogDebug("ExpanderBody.Interact()");
            if (!user.IsOwner())
            {
                Jotunn.Logger.LogDebug("Player isn't the owner");
                return true; // disabled for testing
            }
            if (hold)
            {
                return false;
            }
            if (!hold)
            {   // this works as a toggle
                changeActive( !myZDO.GetBool("isActive") );
                return true;
            }

            // if !hold if active call gui, else throw soft warning
            // if hold toggle active

            return false;
        }

        public string GetHoverName()
        {
            return "Expander";
        }

        public string GetHoverText()
        {
            StringBuilder sb = new StringBuilder(capacity: 50);

            sb.Append(GetHoverName());
            sb.Append(
                "\n Active: " + myZDO.GetBool(isActive, false) +
                "\n Connected: " + myZDO.GetBool(isConnected, false) +
                "\n Expander: " + AllExpanders.Count() +
                "\n Settlements: " + SettlementManager.AllSettleMans.Count());
            if (!(parentSettleMan == null) ){
                sb.Append(
                    "\n Settlement name: " + parentSettleMan.myZDO.GetString(settlementName) +
                    "\n Settlement ID: " + parentSettleMan.myZDOID +
                    "\n Connected Expanders: " + parentSettleMan.GetRegisteredExpanderIDs().Count() +
                    "\n Workbenches: " + parentSettleMan.amountWorkbenches + "-" + parentSettleMan.hasWorkbench +
                    "\n Forges: " + parentSettleMan.amountForges + "-" + parentSettleMan.hasForge +
                    "\n Stonecutters: " + parentSettleMan.amountStonecutters + "-" + parentSettleMan.hasStonecutter +
                    "\n Artisan stations: " + parentSettleMan.amountArtisanStations + "-" + parentSettleMan.hasArtisanStation +
                    "\n Wards: " + parentSettleMan.amountWards + "-" + parentSettleMan.hasWard);
            }
                
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setman"></param>
        /// <param name="register"></param>
        /// <param name="reason"></param>
        public void connectSettleMan(SettlementManager setman, bool register = true, int reason = 0)
        {
            Jotunn.Logger.LogDebug("Connecting" + setman.myZDO.GetString(settlementName) + " to Expander");
            if (register)
                setman.RegisterExpander(this.myID);

            parentSettleMan = setman;
            parentSettleManZDO = parentSettleMan.myZDO;
            myZDO.Set(parentSettleManID, parentSettleMan.myZDOID);
            myZDO.Set(settlementName, parentSettleMan.myZDO.GetString(settlementName));
            myZDO.Set(isConnected, true);
            parentSettleMan.loadedExpanders.Add(this);

            findNearbyWorkbenches(); // this is run for every expander in the settlement?!
            updateProxyWorkbenches();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unregister"></param>
        /// <param name="reason"></param>
        public void disConnectSettleMan(bool unregister = true, int reason = 0)
        {
            Jotunn.Logger.LogDebug("SettleMan disconnecting from ExpanderSoul (if it wasn't already)");
            if(!(parentSettleMan == null))
            {
                Jotunn.Logger.LogDebug("(Was connected) Disconnecting from" + parentSettleMan.myZDO.GetString(settlementName));
                parentSettleMan.loadedExpanders.Remove(this);
                parentSettleMan = null;
                parentSettleManZDO = null;
                if (unregister)
                    parentSettleMan.unRegisterExpander(this.myID, this);
            }
            
            myZDO.Set(parentSettleManID, ZDOID.None);
            myZDO.GetString(settlementName, "None");
            myZDO.Set(isConnected, false);

            updateProxyWorkbenches();
        }


        public void changeActive(bool toactive)
        {
            Jotunn.Logger.LogDebug("ExpanderSoul.changeActive: " + toactive);
            // change whether the Expander can connect and relay connections
            if (toactive)
            {
                myZDO.Set("isActive", true );
                changeConnection(toConnect: true, checkconnections: true); // if I can connect, it'd be nice if I was
            }
            else if (!toactive)
            {
                myZDO.Set("isActive", false);
                changeConnection(toConnect: false, checkconnections: true); // relinquesh connection when deactivating
            }
            updateProxyWorkbenches();
        }



        public void changeConnection(bool toConnect, bool checkconnections = true)
        {
            Jotunn.Logger.LogDebug("ExpanderSoul.changeConnection: " + toConnect + " " + checkconnections);

            if (!toConnect)
            {   // if wanting to disconnect
                Jotunn.Logger.LogDebug("Disconnecting Expander (if it wasn't already)");
                var tempPSM = parentSettleMan;
                disConnectSettleMan(unregister: true);
                // if I have a parentSetMan, unregister
                if (checkconnections && myZDO.GetBool("isActive") && (parentSettleMan is null))
                    tempPSM.checkConnectionsWeb();
                return;
            }

            //Change whether the Expander can connect in this moment
            Vector3 curpos = myZDO.GetVec3(position, Vector3.zero);
            Jotunn.Logger.LogDebug(curpos.ToString());

            if (curpos == Vector3.zero)
            {
                Jotunn.Logger.LogFatal("changeConnection: curpos returned Vector3.zero");
                throw new NullReferenceException("changeConnection: curpos returned Vector3.zero"); // if myBodyZDO returns a Vector3.zero, the position was not properly set
            }

            SettlementManager tempSetMan = SettlementManager.PosInWhichSettlement(curpos);

            if (toConnect && myZDO.GetBool("isActive") && !(tempSetMan is null))
            { // if wanting to connect && expander is active && I'm near enough to a settlement
                Jotunn.Logger.LogDebug("Connecting Expander");
                tempSetMan.RegisterExpander(this.myID, this);
                connectSettleMan(tempSetMan);
                myZDO.Set(isConnected, true);
            }
            else if (tempSetMan is null)
            {   // if not in a village
                Jotunn.Logger.LogWarning("No nearby or active Settlement found!");
                Jotunn.Logger.LogWarning("Creating new SettlementManager and connecting to it.");
                connectSettleMan(SettlementManager.makeNewSettlement(this));

                // this isn't the place to start a new village
                //parentSettleMan = null;
                //isConnected = false;
            }
            else
            {
                Jotunn.Logger.LogFatal("Expander.changeConnection() had an outcome I didn't expect");
            }

            if (checkconnections && !(tempSetMan is null))
            {
                tempSetMan.checkConnectionsWeb();
            }
        }

        public static ZDO GetZDO(ZDOID zdoid)
        {
            return ZDOMan.instance.GetZDO(zdoid);
        }

        /// <summary>
        /// Tests for each workbench if the settlement will provide and set range
        /// else set range for all to 0;
        /// </summary>
        public void updateProxyWorkbenches()
        {
            if (myZDO.GetBool(isConnected))
            {
                // TODO: replace townshipinstance thing with config thing
                if (parentSettleMan.hasWorkbench)
                    workbenchProxy.GetComponent<CraftingStation>().m_rangeBuild = TownshipManager.instance.Extender_buildrange;
                else
                    workbenchProxy.GetComponent<CraftingStation>().m_rangeBuild = 0;

                if (parentSettleMan.hasForge)
                    forgeProxy.GetComponent<CraftingStation>().m_rangeBuild = TownshipManager.instance.Extender_buildrange;
                else
                    forgeProxy.GetComponent<CraftingStation>().m_rangeBuild = 0;

                if (parentSettleMan.hasStonecutter)
                    stonecutterProxy.GetComponent<CraftingStation>().m_rangeBuild = TownshipManager.instance.Extender_buildrange;
                else
                    stonecutterProxy.GetComponent<CraftingStation>().m_rangeBuild = 0;

                if (parentSettleMan.hasArtisanStation)
                    artisanProxy.GetComponent<CraftingStation>().m_rangeBuild = TownshipManager.instance.Extender_buildrange;
                else
                    artisanProxy.GetComponent<CraftingStation>().m_rangeBuild = 0;
            }
        }



    }
    

}
