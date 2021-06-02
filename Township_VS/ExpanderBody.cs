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
        public ZNetView m_nview;

        public static List<ExpanderBody> AllExpanderBodies = new List<ExpanderBody>();


        // Body needs it's own ZDO to store some sort of ID for connecting with the soul.
        // And save some persistent variables like hasSoul and isPlaced
        public ZDO myZDO;

        public ZDO mySoulZDO;


        // This variable is only available to the Server!!!!!!
        public ExpanderSoul mySoul;

        public bool hasSoul
        {   get { return m_nview.GetZDO().GetBool("hasSoul"); }
            set { m_nview.GetZDO().Set("hasSoul", value); }
        }
        public bool isPlaced;

        public ZDOID myZDOID
        {   get { return myZDO.GetZDOID("myZDOID"); }
            set { myZDO.Set("myZDOID", value); } }
        public ZDOID mySoulZDOID
        {   get { return myZDO.GetZDOID("mySoulZDOID"); }
            set { myZDO.Set("mySoulZDOID", value); }
        }


        // the body isn't allowed to set these variables of the soul!!!
        public bool isActive
        {   get { return mySoulZDO.GetBool("isActive"); }
        }
        public bool isConnected
        {
            get { return mySoulZDO.GetBool("isConnected"); }
        }
        public string settlementName
        {
            get { return mySoulZDO.GetString("settlementName"); }
        }




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

                    AllExpanderBodies.Add(this);
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
                        mySoulZDO = mySoul.myZDO;
                    } else
                    {
                        Jotunn.Logger.LogDebug("Placed Expander has a soul, finding and connecting to that one");

                        // find the soul

                        m_nview.InvokeRPC(ZNetView.Everybody, "connecttoSoul");
                        mySoulZDO = mySoul.myZDO;
                    }


                    // m_nview.InvokeRPC( ZNetView.Everybody, "changeActive", isActive );
                    // isActive is stored in the Soul and set there, Body just fetches this value
                    m_nview.GetZDO().Set("mySoulZDOID", mySoulZDOID);

                    mySoulZDOID = mySoulZDO.m_uid;

                    Jotunn.Logger.LogDebug("ExpanderBody ZDOID: " + myZDOID);
                    Jotunn.Logger.LogDebug("ExpanderSoul ZDOID: " + mySoulZDOID);


                    // how to register RPC
                    //m_nview.Register<bool>("changeActive", RPC_changeActive);
                    // how to invoke RPC
                    // m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", isActive);

                    myZDO.m_persistent = true;// Doing this at the end; if a NRE shows up before this, the ZDO will be cleaned when the world closes
                    Jotunn.Logger.LogDebug("Done doing stuff to ExpanderBody\n");
                }
            }
        }

        public void RPC_createNewSoul(long sender)
        {
            if( TownshipManager.IsServerorLocal() )
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_createNewSoul; making new soul");
                mySoul = new ExpanderSoul(this);
                mySoulZDO = mySoul.myZDO;
            }
        }

        private void RPC_connecttoSoul(long obj)
        {
            if (TownshipManager.IsServerorLocal())
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
                    Jotunn.Logger.LogFatal("Emergency soul creation");
                    m_nview.InvokeRPC(ZNetView.Everybody, "createNewSoul");
                    m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", false);
                }
            }
        }

        public void RPC_changeConnection(long sender, bool toConnect, bool checkconnections = true )
        {
            if (TownshipManager.IsServerorLocal())
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_changeConnection; changing connections : " + toConnect + " " + checkconnections);
                mySoul.changeConnection(toConnect:toConnect, checkconnections:checkconnections);
            }
        }

        public void RPC_changeActive(long sender, bool toactive)
        {
            // send RPC to the soul
            if ( TownshipManager.IsServerorLocal() )
            {
                Jotunn.Logger.LogDebug("RPC reciever RPC_changeActive; changing Expander to " + toactive);
                mySoul.changeActive( toactive: toactive);
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
                    m_nview.InvokeRPC(ZNetView.Everybody, "onDestroyed" );
                }
                AllExpanderBodies.Remove(this);
            }
        }

        public void RPC_OnDestroyed(long user)
        {
            if (TownshipManager.IsServerorLocal())
            {
                Jotunn.Logger.LogDebug("ExpanderBody.RPC_OnDestroyed()");
                mySoul.onDestroyed();
            }
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
            if( isPlaced)
            {
                if(!(mySoul is null))
                    mySoul.disconnectBody();
                AllExpanderBodies.Remove(this);
            }
        }

        public void RPC_OnDestroy( long user )
        {
            if (TownshipManager.IsServerorLocal())
            {
                Jotunn.Logger.LogDebug("ExpanderBody.RPC_OnDestroy()");
                mySoul.disconnectBody();
                mySoul = null;
            }
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
            StringBuilder sb = new StringBuilder(capacity:50);

            sb.Append( GetHoverName() );

            if (mySoul is null)
            {
                sb.Append(
                   "\n This Body has no soul," +
                   "\n please destroy and place it again"
                );
            }
            else
            {
                sb.Append(
                    "\n Active: " + isActive +
                    "\n Connected: " + isConnected +
                    "\n ExpanderBodies: " + AllExpanderBodies.Count() +
                    "\n ExpanderSouls: " + ExpanderSoul.AllExpanderSouls.Count() +
                    "\n SettlementManagers: " + SettlementManager.AllSettleMans.Count() +
                    "\n Connected Settlement: " + settlementName);
            }
            return sb.ToString();
        }
    }
}
