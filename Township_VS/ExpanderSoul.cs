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

        public ZDO myBodyZDO;
        public ZDOID myBodyZDOID;
        public ExpanderBody myBody;

        public bool isActive
        {
            get { return myBodyZDO.GetBool("isActive"); }
            set { myBodyZDO.Set("isActive", value); }
        }
        public bool isConnected
        {
            get { return myBodyZDO.GetBool("isConnected"); }
            set { myBodyZDO.Set("isConnected", value); }
        }
        public bool hasSoul
        {
            get { return myBodyZDO.GetBool("hasSoul"); }
            set { myBodyZDO.Set("hasSoul", value); }
        }
        public Vector3 position
        {
            get { return myBodyZDO.GetVec3("position", Vector3.zero );      }
            set { myBodyZDO.Set("position", myBody.m_piece.GetCenter() );   } // if this causes a NRE, then the call itself was bad
        }


        // ////////////////////////////// CONSTRUCTORS

        // Constructor called when loading save
        public ExpanderSoul( ZDO mybodyZDO )
        {
            myBodyZDO = mybodyZDO;
        }

        // Constructor called when a new ExpanderBody is amde
        public ExpanderSoul(ZDO mybodyZDO, ExpanderBody mybody)
        {
            myBodyZDO = mybodyZDO;
            myBody = mybody;
        }

        // ////////////////////////////// METHODES

        // called by myBody when it gets destroyed.
        // Soul also gets destroyed
        public void OnDestroyed()
        {
            if( parentSettleMan != null )
            {
                parentSettleMan.unRegisterExpanderSoul(this);
            }
            // Destroy self?
        }


        // called when the body is unloaded (but not destroyed)
        public void disconnectBody()
        {
            myBody = null;
        }
        // called when the body is loaded
        public void connectBody(ExpanderBody mybody )
        {
            myBody = mybody;
        }



        public void changeConnection(bool toConnect, bool checkconnections = true)
        {

            // TODO: myBody *can* be null

            //Change whether the Expander can connect in this moment
            Vector3 curpos = myBodyZDO.GetVec3("position", Vector3.zero);
            if ( curpos == Vector3.zero)
            {
                throw new NullReferenceException(); // if myBodyZDO returns a Vector3.zero, the position was not properly set
            }
            SettlementManager tempSetMan = m_tsManager.PosInWhichSettlement( curpos );

            if (toConnect && myBody.isActive && tempSetMan != null)
            { // if wanting to connect && expander is active && I'm near enough to a settlement
                tempSetMan.RegisterExpanderSoul(this, myBodyZDO );
                parentSettleMan = tempSetMan;
                isConnected = true;
                Jotunn.Logger.LogInfo("Connecting Expander");
            }
            else if (!toConnect)
            {   // if wanting to disconnect
                tempSetMan.unRegisterExpanderSoul(this);
                parentSettleMan = tempSetMan;
                isConnected = false;
                Jotunn.Logger.LogInfo("Disconnecting Expander");
            }
            else if (tempSetMan == null)
            {   // if not in a village
                // this isn't the place to start a new village
                parentSettleMan = null;
                isConnected = false;
                Jotunn.Logger.LogInfo("No nearby or active Settlement found!");
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
