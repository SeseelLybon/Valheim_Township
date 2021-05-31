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
    class ExpanderSoul : MonoBehaviour
    {
        public static List<ExpanderSoul> AllExpanderSouls = new List<ExpanderSoul>();

        public SettlementManager parentSettleMan; // the SettlementManager that is connected to this expander
        public ZDO parentSettleManZDO; // the SettlementManager that is connected to this expander

        public TownshipManager m_tsManager;


        public ZDO myZDO;
        public ZDOID myZDOID;

        public ExpanderBody myBody;
        public ZDO myBodyZDO;


        public bool isActive
        {
            get { return myZDO.GetBool("isActive"); }
            set { myZDO.Set("isActive", value); }
        }
        public bool isConnected
        {
            get { return myZDO.GetBool("isConnected"); }
            set { myZDO.Set("isConnected", value); }
        }
        public bool hasSoul
        {
            get { return myZDO.GetBool("hasSoul"); }
            set { myZDO.Set("hasSoul", value); }
        }
        public string settlementName
        {
            get { return myZDO.GetString("settlementName"); }
            set { myZDO.Set("settlementName", value); }
        }
        public Vector3 position
        {
            get { return myZDO.GetVec3("position", Vector3.zero );      }
            set { myZDO.Set("position", value );   }
        }
        public ZDOID parentSettleManZDOID
        {
            get { return myZDO.GetZDOID("parentSettleManZDOID"); }
            set { myZDO.Set("parentSettleManZDOID", value); }
        }
        public ZDOID myBodyZDOID
        {
            get { return myZDO.GetZDOID("myBodyZDOID"); }
            set { myZDO.Set("myBodyZDOID", value); }
        }


        // ////////////////////////////// CONSTRUCTORS

        // Constructor called when loading save
        public ExpanderSoul( ZDO mysoulZDO )
        {
            m_tsManager = TownshipManager.Instance;

            Jotunn.Logger.LogDebug("Constructing ExpanderSoul from save.");
            myZDO = mysoulZDO;
            myZDO.m_persistent = true;
            myZDOID = myZDO.m_uid;


            // check if my body still exists/has ZDO

            List<ZDO> expanderBodyZDOs = new List<ZDO>();
            ZDOMan.instance.GetAllZDOsWithPrefab("piece_TS_Expander", expanderBodyZDOs);
            Jotunn.Logger.LogDebug("Seeing if ExpanderBody is among " + expanderBodyZDOs.Count() + " ExpanderBodyZDO's");
            bool stillhasbody = false;
            foreach (ZDO expanderbodyZDO in expanderBodyZDOs)
            {
                Jotunn.Logger.LogDebug(myZDOID + " vs " + expanderbodyZDO.GetZDOID("mySoulZDOID"));
                if ( expanderbodyZDO.GetZDOID("mySoulZDOID") == myZDOID)
                {
                    Jotunn.Logger.LogDebug("ExpanderSoul still has a body.");
                    stillhasbody = true;
                    myBodyZDO = expanderbodyZDO;
                    break;
                }
            }
            if (!stillhasbody)
            {
                Jotunn.Logger.LogFatal("ExpanderSoul seems to no longer have a body.");
                onDestroyed();
                return;
            }

            myZDO.m_persistent = true;// Doing this at the end; if a NRE shows up before this, the ZDO will be cleaned when the world closes
            AllExpanderSouls.Add(this);
        }

        // Constructor called when a new ExpanderBody is made
        public ExpanderSoul( ExpanderBody mybody)
        {
            m_tsManager = TownshipManager.Instance;

            Jotunn.Logger.LogDebug("Constructing ExpanderSoul from ExpanderBody.");
            myBody = mybody;


            Jotunn.Logger.LogDebug("\t creating new ZDO for ExpanderSoul");

            myZDO = ZDOMan.instance.CreateNewZDO(new Vector3(0,-10000,0));
            myZDO.SetPrefab( m_tsManager.expanderprefabname.GetStableHashCode() );


            Jotunn.Logger.LogDebug("\t populating new ZDO");
            isActive = false;
            isConnected = false;
            position = myBody.m_piece.GetCenter();
            myBodyZDO = myBody.myZDO;
            myBodyZDOID = myBody.myZDOID;
            myBody.mySoulZDOID = myZDOID; // give myBody my ZDOID

            changeActive(isActive);

            myZDO.m_persistent = true;// Doing this at the end; if a NRE shows up before this, the ZDO will be cleaned when the world closes
            AllExpanderSouls.Add(this);
        }

        // ////////////////////////////// METHODES //////////////////////////////

        // called by myBody when it gets destroyed.
        // Soul also gets destroyed
        public void onDestroyed()
        {
            Jotunn.Logger.LogDebug("An Expader Soul is being destroyed because its body no longer availabe");
            if (parentSettleMan != null)
            {
                parentSettleMan.unRegisterExpanderSoul(this);
            }
            AllExpanderSouls.Remove(this);
            myZDO.m_persistent = false;
            disConnectSettleMan();

            //myZDO.Reset();
            //Destroy(this);
        }

        // called when unloading but not destroying
        public void onDestroy()
        {
            Jotunn.Logger.LogDebug("ExpanderSoul.OnDestroy()");
            AllExpanderSouls.Remove(this);
        }


        // called when the body is unloaded (but not destroyed)
        public void disconnectBody()
        {
            Jotunn.Logger.LogDebug("An Expader Soul was disconnected from its body.");
            myBody = null;
        }

        // called when the body is loaded
        public void connectBody(ExpanderBody mybody )
        {
            Jotunn.Logger.LogDebug("An Expader Soul was connected to its body.");
            myBody = mybody;
        }

        public void connectSettleMan( SettlementManager setman, int reason = 0 )
        {
            Jotunn.Logger.LogDebug(setman.settlementName + " connecting to ExpanderSoul");
            setman.RegisterExpanderSoul( this );
            parentSettleMan = setman;
            parentSettleManZDO = parentSettleMan.myZDO;
            parentSettleManZDOID = parentSettleMan.myZDOID;
            settlementName = parentSettleMan.settlementName;
            isConnected = true;
        }

        public void disConnectSettleMan(int reason = 0)
        {
            Jotunn.Logger.LogDebug("SettleMan disconnecting to ExpanderSoul (if I wasn't already)");
            parentSettleMan.unRegisterExpanderSoul( this);

            parentSettleMan = null;
            parentSettleManZDO = null;
            // parentSettleManZDOID = null; // can't set ZDOIDD to null
            settlementName = "None";
            isConnected = false;
        }


        public void changeActive( bool toactive )
        {
            Jotunn.Logger.LogDebug("ExpanderSoul.changeActive: " + toactive);
            // change whether the Expander can connect and relay connections
            if (toactive)
            {   
                isActive = true;
                changeConnection(toConnect: true, checkconnections: true); // if I can connect, it'd be nice if I was
            }
            else if (!toactive)
            {   
                isActive = false;
                changeConnection(toConnect: false, checkconnections: true); // relinquesh connection when deactivating
            }
        }



        public void changeConnection(bool toConnect, bool checkconnections = true)
        {
            Jotunn.Logger.LogDebug("ExpanderSoul.changeConnection: " + toConnect + " " + checkconnections);

            // TODO: myBody *can* be null

            if (!toConnect)
            {   // if wanting to disconnect
                Jotunn.Logger.LogDebug("Disconnecting Expander (if it wasn't already)");

                // if I have a parentSetMan, unregister
                if( parentSettleMan != null )
                {
                    parentSettleMan.unRegisterExpanderSoul(this);
                    // Do I want my parent to check his connections?
                    if (checkconnections && isActive)
                        parentSettleMan.checkConnectionsWeb();
                }

                disConnectSettleMan();
                return;
            }

            //Change whether the Expander can connect in this moment
            Vector3 curpos = myZDO.GetVec3("position", Vector3.zero);
            Jotunn.Logger.LogDebug( curpos.ToString() );

            if ( curpos == Vector3.zero)
            {
                Jotunn.Logger.LogFatal("changeConnection: curpos returned Vector3.zero");
                throw new NullReferenceException(); // if myBodyZDO returns a Vector3.zero, the position was not properly set
            }

            SettlementManager tempSetMan = TownshipManager.PosInWhichSettlement( curpos );

            if ( toConnect && isActive && tempSetMan != null)
            { // if wanting to connect && expander is active && I'm near enough to a settlement
                Jotunn.Logger.LogDebug("Connecting Expander");
                tempSetMan.RegisterExpanderSoul( this );
                connectSettleMan(tempSetMan);
                isConnected = true;
            }
            else if (tempSetMan == null)
            {   // if not in a village
                Jotunn.Logger.LogWarning("No nearby or active Settlement found!");
                Jotunn.Logger.LogWarning("Creating new SettlementManager and connecting to it.");
                connectSettleMan( SettlementManager.registerNewSettlement(this) );

                // this isn't the place to start a new village
                //parentSettleMan = null;
                //isConnected = false;
            }
            else
            {
                Jotunn.Logger.LogFatal("Expander.changeConnection() had an outcome I didn't expect");
            }

            if (checkconnections && tempSetMan != null)
            {
                tempSetMan.checkConnectionsWeb();
            }
        }
    }
}
