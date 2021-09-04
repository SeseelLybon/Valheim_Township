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
            get { return myZDO.GetVec3("position", Vector3.zero); }
            set { myZDO.Set("position", value); }
        }
        public ZDOID parentSettleManZDOID
        {
            get { return myZDO.GetZDOID("parentSettleManZDOID"); }
            set { myZDO.Set("parentSettleManZDOID", value); }
        }
        public ZDOID myZDOID
        {
            get { return myZDO.GetZDOID("myZDOID"); }
            set { myZDO.Set("myZDOID", value); }
        }

        bool isPlaced = false;



        public void Start()
        {
            Jotunn.Logger.LogDebug("ExpanderBody.Start()");

            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
            m_tsManager = TownshipManager.Instance;


            if (m_piece.IsPlacedByPlayer())
            {
                isPlaced = true;
                Jotunn.Logger.LogDebug("ExpanderBody is placed by player");
                if (TownshipManager.IsServerorLocal())
                {
                    Jotunn.Logger.LogDebug("Doing stuff to expander that was placed by a player");

                    GetComponent<WearNTear>().m_onDestroyed += OnDestroyed;

                    AllExpanders.Add(this);
                    myZDO = m_nview.GetZDO();
                    myZDOID = myZDO.m_uid;

                    m_nview.Register<bool>("changeActive", RPC_changeActive);
                    m_nview.Register<bool, bool>("changeConnection", RPC_changeConnection);
                    m_nview.Register("onDestroy", RPC_OnDestroy);
                    m_nview.Register("onDestroyed", RPC_OnDestroyed);

                    Jotunn.Logger.LogDebug("Expander ZDOID: " + myZDOID);


                    position = m_piece.GetCenter();

                    // how to register RPC
                    //m_nview.Register<bool>("changeActive", RPC_changeActive);
                    // how to invoke RPC
                    // m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", isActive);

                    myZDO.m_persistent = true;// Doing this at the end; if a NRE shows up before this, the ZDO will be cleaned when the world closes
                    Jotunn.Logger.LogDebug("Done doing stuff to ExpanderBody\n");
                }
            }
        }

        public void RPC_changeConnection(long sender, bool toConnect, bool checkconnections = true)
        {
            if (TownshipManager.IsServerorLocal())
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_changeConnection; changing connections : " + toConnect + " " + checkconnections);
                changeConnection(toConnect: toConnect, checkconnections: checkconnections);
            }
        }

        public void RPC_changeActive(long sender, bool toactive)
        {
            // send RPC to the soul
            if (TownshipManager.IsServerorLocal())
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_changeActive; changing Expander to " + toactive);
                changeActive(toactive: toactive);
            }

            // Do some stuff to the body
            if (m_nview.IsOwner())
            {
                if (toactive)
                {
                    GetComponent<CraftingStation>().m_rangeBuild = 10;
                }
                else
                {
                    GetComponent<CraftingStation>().m_rangeBuild = 0;
                }
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            // if wacked with the "Stick of creating settlements", create a new settlement.
            return false;
        }

        // Called when the piece is destroyed ingame
        public void OnDestroyed()
        {
            if (m_piece.IsPlacedByPlayer())
            {
                Jotunn.Logger.LogDebug("ExpanderBody.OnDestroyed()");
                if (m_nview.IsOwner())
                {
                    m_nview.InvokeRPC(ZNetView.Everybody, "onDestroyed");
                }
                AllExpanders.Remove(this);
            }
        }

        public void RPC_OnDestroyed(long user)
        {
        }

        // Called when the object is destroyed, either by unloading or anything
        public void OnDestroy()
        {
            Jotunn.Logger.LogDebug("ExpanderBody.OnDestroy()");
            /*
            if( true )//m_piece.IsPlacedByPlayer() ) // can't call these objects because they're already gone.
                if ( true) //m_nview.IsOwner() )
                    if(mySoul != null)
                        m_nview.InvokeRPC(ZNetView.Everybody, "onDestroy");
            */
            if (isPlaced)
            {
                AllExpanders.Remove(this);
            }
        }

        public void RPC_OnDestroy(long user)
        {

        }

        /* Function is now only for making active or not.
          *  Will later be used to call the GUI
          */
        public bool Interact(Humanoid user, bool hold)
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
                m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", !isActive);
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
                "\n Active: " + isActive +
                "\n Connected: " + isConnected +
                "\n ExpanderBodies: " + AllExpanders.Count() +
                "\n ExpanderSouls: " + Expander.AllExpanders.Count() +
                "\n SettlementManagers: " + SettlementManager.AllSettleMans.Count() +
                "\n Connected Settlement: " + settlementName);
            return sb.ToString();
        }
        public void onDestroyed()
        {
            Jotunn.Logger.LogDebug("An Expader Soul is being destroyed because its body no longer availabe");
            if (parentSettleMan != null)
            {
                parentSettleMan.unRegisterExpanderSoul(this);
            }
            AllExpanders.Remove(this);
            myZDO.m_persistent = false;
            disConnectSettleMan(unregister: true);

            //myZDO.Reset();
            //Destroy(this);
        }
        public void connectSettleMan(SettlementManager setman, bool register = true, int reason = 0)
        {
            Jotunn.Logger.LogDebug(setman.settlementName + " connecting to ExpanderSoul");
            if (register)
                setman.RegisterExpanderSoul(this);
            parentSettleMan = setman;
            parentSettleManZDO = parentSettleMan.myZDO;
            parentSettleManZDOID = parentSettleMan.myZDOID;
            settlementName = parentSettleMan.settlementName;
            isConnected = true;
        }

        public void disConnectSettleMan(bool unregister = true, int reason = 0)
        {
            Jotunn.Logger.LogDebug("SettleMan disconnecting from ExpanderSoul (if it wasn't already)");
            if (!(parentSettleMan is null))
            {
                Jotunn.Logger.LogDebug("(Was connected) Disconnecting from" + parentSettleMan.settlementName);
                if (unregister)
                    parentSettleMan.unRegisterExpanderSoul(this);
                parentSettleMan = null;
                parentSettleManZDO = null;
                // parentSettleManZDOID = null; // can't set ZDOIDD to null
                settlementName = "None";
                isConnected = false;
            }
            else
            {
                Jotunn.Logger.LogDebug("(Wasn't connected)");
            }
        }


        public void changeActive(bool toactive)
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
                disConnectSettleMan(unregister: true);

                if (checkconnections && isActive && (parentSettleMan is null))
                    parentSettleMan.checkConnectionsWeb();

                return;
            }

            //Change whether the Expander can connect in this moment
            Vector3 curpos = position;
            Jotunn.Logger.LogDebug(curpos.ToString());

            if (curpos == Vector3.zero)
            {
                Jotunn.Logger.LogFatal("changeConnection: curpos returned Vector3.zero");
                throw new NullReferenceException(); // if myBodyZDO returns a Vector3.zero, the position was not properly set
            }

            SettlementManager tempSetMan = SettlementManager.PosInWhichSettlement(curpos);

            if (toConnect && isActive && !(tempSetMan is null))
            { // if wanting to connect && expander is active && I'm near enough to a settlement
                Jotunn.Logger.LogDebug("Connecting Expander");
                tempSetMan.RegisterExpanderSoul(this);
                connectSettleMan(tempSetMan);
                isConnected = true;
            }
            else if (tempSetMan is null)
            {   // if not in a village
                Jotunn.Logger.LogWarning("No nearby or active Settlement found!");
                Jotunn.Logger.LogWarning("Creating new SettlementManager and connecting to it.");
                connectSettleMan(SettlementManager.registerNewSettlement(this));

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
    }
}
