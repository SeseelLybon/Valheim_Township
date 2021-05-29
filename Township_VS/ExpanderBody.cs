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

    class ExpanderBody : MonoBehaviour, Hoverable, Interactable
    {

        public Piece m_piece;
        public TownshipManager m_tsManager;
        private ZNetView m_nview;

        public static List<ExpanderBody> AllExpanderBodies = new List<ExpanderBody>();




        // This variable is only available to the Server!!!!!!
        public ExpanderSoul mySoul;


        public bool isActive
        {   get { return mySoulZDO.GetBool("isActive"); }
            set { mySoulZDO.Set("isActive", value); } }

        public bool isConnected
        {   get { return mySoulZDO.GetBool("isConnected"); }
            set { mySoulZDO.Set("isConnected", value); } }
        public bool hasSoul
        {   get { return m_nview.GetZDO().GetBool("hasSoul"); }
            set { m_nview.GetZDO().Set("hasSoul", value); } }

        public ZDO myZDO;
        public ZDOID myZDOID
        {   get { return myZDO.GetZDOID("myZDOID"); }
            set { myZDO.Set("myZDOID", value); } }

        public ZDO mySoulZDO;
        public ZDOID mySoulZDOID
        {   get { return myZDO.GetZDOID("mySoulZDOID"); }
            set { myZDO.Set("mySoulZDOID", value); } }




        public void Start()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
            m_tsManager = TownshipManager.Instance;


            if (m_piece.IsPlacedByPlayer()) {
                if( m_nview.IsOwner() )
                {
                    Jotunn.Logger.LogDebug("Doing stuff to expander that was placed by a player");

                    GetComponent<WearNTear>().m_onDestroyed += OnDestroyed;

                    AllExpanderBodies.Add(this);
                    m_nview.GetZDO().m_persistent = true;
                    myZDO = m_nview.GetZDO();
                    myZDOID = myZDO.m_uid;

                    m_nview.Register<bool>("changeActive", RPC_changeActive);
                    m_nview.Register<bool, bool>("changeConnection", RPC_changeConnection);
                    m_nview.Register("createNewSoul", RPC_createNewSoul);
                    m_nview.Register("connecttoSoul", RPC_connecttoSoul);
                    m_nview.Register("onDestroy", RPC_OnDestroy);
                    m_nview.Register("onDestroyed", RPC_OnDestroyed);

                    if (!hasSoul)
                    {   // no soul in ZDO, then a new expander;

                        Jotunn.Logger.LogDebug("Placed Expander had no soul, creating new one");

                        // this operation can only be done by the server!
                        m_nview.InvokeRPC(ZNetView.Everybody, "createNewSoul");
                        // mySoul.connectBody(this); // createNewSoul also connects the body

                        hasSoul = true;
                    } else
                    {
                        Jotunn.Logger.LogDebug("Placed Expander has a soul, finding and connecting to that one");

                        // find the soul


                        m_nview.InvokeRPC(ZNetView.Everybody, "connecttoSoul");
                    }

                    m_nview.InvokeRPC( ZNetView.Everybody, "changeActive", isActive );
                    m_nview.GetZDO().Set("mySoulZDOID", mySoulZDOID);

                    mySoulZDOID = mySoulZDO.m_uid;

                    Jotunn.Logger.LogDebug("ExpanderBody ZDOID: " + myZDOID);
                    Jotunn.Logger.LogDebug("ExpanderSoul ZDOID: " + mySoulZDOID);


                    // how to register RPC
                    //m_nview.Register<bool>("changeActive", RPC_changeActive);
                    // how to invoke RPC
                    // m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", isActive);

                    Jotunn.Logger.LogDebug("Done doing stuff to ExpanderBody");
                }
            }
        }

        public void RPC_createNewSoul(long sender)
        {
            if( m_tsManager.IsServerorLocal() )
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_createNewSoul; making new soul");
                mySoul = new ExpanderSoul(this);
                mySoulZDO = mySoul.myZDO;
            }
        }

        private void RPC_connecttoSoul(long obj)
        {
            if (m_tsManager.IsServerorLocal())
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_connecttoSoul; connecting to soul");
                Jotunn.Logger.LogDebug("Picking through " + ExpanderSoul.AllExpanderSouls.Count() + " ExpanderSouls.");
                foreach (ExpanderSoul soul in ExpanderSoul.AllExpanderSouls)
                {
                    Jotunn.Logger.LogDebug(myZDOID + " vs " + soul.myBodyZDOID);
                    if (soul.myBodyZDOID == myZDOID)
                    {
                        Jotunn.Logger.LogDebug("Found the soul");
                        mySoul = soul;
                        mySoulZDO = mySoul.myZDO;
                        mySoul.connectBody(this);
                    }
                }
                if (mySoul is null)
                {
                    Jotunn.Logger.LogFatal("No soul found!");
                    Jotunn.Logger.LogFatal("NRE incoming!");
                    hasSoul = false;
                    myZDO.m_persistent = false;
                    myZDO.Reset();
                    //Jotunn.Logger.LogDebug("Emergency soul creation");
                    //m_nview.InvokeRPC(ZNetView.Everybody, "createNewSoul");
                }
            }
        }

        public void RPC_changeConnection(long sender, bool toConnect, bool checkconnections = true )
        {

            if (m_tsManager.IsServerorLocal())
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_changeConnection; changing connections : " + toConnect + " " + checkconnections);
                mySoul.changeConnection(toConnect:toConnect, checkconnections:checkconnections);
            }
        }

        public void RPC_changeActive(long sender, bool toactive)
        {
            if ( m_nview.IsOwner() )
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_changeActive; changing Expander to " + toactive);
                // change whether the Expander can connect and relay connections
                if (toactive)// && !isActive) // if true and false
                {
                    isActive = true;
                    //changeConnection(toConnect: true, checkconnections: true); // if I can connect, it'd be nice if I was
                    m_nview.InvokeRPC(ZNetView.Everybody, "changeConnection", true, true);
                    GetComponent<CraftingStation>().m_rangeBuild = 10;
                }
                else if (!toactive)// && isActive) // if false and true
                {
                    isActive = false;
                    //changeConnection(toConnect: false, checkconnections: true); // relinquesh connection when deactivatin
                    m_nview.InvokeRPC(ZNetView.Everybody, "changeConnection", false, true);
                    GetComponent<CraftingStation>().m_rangeBuild = 0;
                }
            } else if (m_tsManager.IsServerorLocal() )
            {
                // what if this runs on a server?!
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
                if (m_nview.IsOwner())
                {
                    m_nview.InvokeRPC(ZNetView.Everybody, "onDestroyed" );
                }
            }
            AllExpanderBodies.Remove(this);
        }

        public void RPC_OnDestroyed(long user)
        {
            if (m_tsManager.IsServerorLocal())
            {
                mySoul.onDestroyed();
            }
        }

        // Called when the object is destroyed, either by unloading or anything
        public void OnDestroy()
        {
            /*
            if( true )//m_piece.IsPlacedByPlayer() ) // can't call these objects because they're already gone.
                if ( true) //m_nview.IsOwner() )
                    if(mySoul != null)
                        m_nview.InvokeRPC(ZNetView.Everybody, "onDestroy");

            AllExpanderBodies.Remove(this);
            */
        }

        public void RPC_OnDestroy( long user )
        {
            if (m_tsManager.IsServerorLocal())
            {
                mySoul.disconnectBody();
                mySoul = null;
            }
        }

        /* Function is now only for making active or not.
          *  Will later be used to call the GUI
          */
        public bool Interact(Humanoid user, bool hold)
        {
            if (!user.IsOwner())
            {
                return true;
            }
            if (hold)
            {
                return false;
            }
            if (!hold)
            {
                if (mySoulZDO.GetBool("isActive"))
                {
                    m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", false);
                    return true;
                }
                else
                {
                    m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", true);
                    return true;
                }
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
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\n Active: " + isActive +
                "\n Connected: " + isConnected +
                "\n ExpanderBodies: " + AllExpanderBodies.Count() +
                "\n ExpanderSouls: " + ExpanderSoul.AllExpanderSouls.Count();
        }
    }
}
