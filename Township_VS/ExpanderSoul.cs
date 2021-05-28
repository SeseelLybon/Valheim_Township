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
    class ExpanderSoul
    {
        public static List<ExpanderSoul> AllExpanderSouls = new List<ExpanderSoul>();

        public SettlementManager parentSettleMan; // the SettlementManager that is connected to this expander

        public TownshipManager m_tsManager;

        public ZDO myZDO;
        public ZDOID myZDOID;

        public ZDOID myBodyZDOID;
        public ExpanderBody myBody;

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
        public Vector3 position
        {
            get { return myZDO.GetVec3("position", Vector3.zero );      }
            set { myZDO.Set("position", value );   } // if this causes a NRE, then the call itself was bad
        }
        public ZDOID parentSettleManZDOID
        {
            get { return myZDO.GetZDOID("parentSettleManZDOID"); }
            set { myZDO.Set("parentSettleManZDOID", value); } // if this causes a NRE, then the call itself was bad
        }


        // ////////////////////////////// CONSTRUCTORS

        // Constructor called when loading save
        public ExpanderSoul( ZDO mysoulZDO )
        {
            m_tsManager = TownshipManager.Instance;
            Jotunn.Logger.LogDebug("Constructing ExpanderSoul from save.");
            myZDO = mysoulZDO;

            AllExpanderSouls.Add(this);
        }

        // Constructor called when a new ExpanderBody is made
        public ExpanderSoul( ExpanderBody mybody)
        {
            m_tsManager = TownshipManager.Instance;
            Jotunn.Logger.LogDebug("Constructing ExpanderSoul from ExpanderBody.");
            myBody = mybody;
            myBodyZDOID = myBody.myZDOID;

            Jotunn.Logger.LogDebug("\t creating new ZDO for ExpanderSoul");
            myZDO = ZDOMan.instance.CreateNewZDO(Vector3.zero);
            myZDO.SetPrefab(m_tsManager.expanderprefabname.GetStableHashCode());
            myZDO.m_persistent = true;

            Jotunn.Logger.LogDebug("\t populating new ZDO");
            myZDO.Set("isActive", myZDO.GetBool("isActive", false));
            myZDO.Set("isConnected", myZDO.GetBool("isConnected", false));
            myZDO.Set("position", myBody.m_piece.m_center);

            AllExpanderSouls.Add(this);
        }

        // ////////////////////////////// METHODES

        // called by myBody when it gets destroyed.
        // Soul also gets destroyed
        public void OnDestroyed()
        {
            Jotunn.Logger.LogDebug("An Expader Soul is being destroyed because its body is.");
            if ( parentSettleMan != null )
            {
                parentSettleMan.unRegisterExpanderSoul(this);
            }
            AllExpanderSouls.Remove(this);
            // Destroy self?
        }

        public void OnDestroy()
        {
            Jotunn.Logger.LogDebug("An Expader Soul is being destroyed.");
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



        public void changeConnection(bool toConnect, bool checkconnections = true)
        {
            Jotunn.Logger.LogDebug("Changing connections of an Expander; " + toConnect + " " + checkconnections);

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

                parentSettleMan = null;
                isConnected = false;
                return;
            }

            //Change whether the Expander can connect in this moment
            Vector3 curpos = myZDO.GetVec3("position", Vector3.zero);

            if ( curpos == Vector3.zero)
            {
                Jotunn.Logger.LogFatal("changeConnection: curpos returned Vector3.zero");
                throw new NullReferenceException(); // if myBodyZDO returns a Vector3.zero, the position was not properly set
            }

            SettlementManager tempSetMan = m_tsManager.PosInWhichSettlement( curpos );

            if ( toConnect && isActive && tempSetMan != null)
            { // if wanting to connect && expander is active && I'm near enough to a settlement
                Jotunn.Logger.LogDebug("Connecting Expander");
                tempSetMan.RegisterExpanderSoul( this );
                parentSettleMan = tempSetMan;
                isConnected = true;
            }
            else if (tempSetMan == null)
            {   // if not in a village
                // this isn't the place to start a new village
                Jotunn.Logger.LogDebug("No nearby or active Settlement found!");
                parentSettleMan = null;
                isConnected = false;
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
