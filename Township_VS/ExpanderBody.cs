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
    // This class is for use on the Expander totems.
    // Their main purpose is to expand the SettlementManager's sphere of unfluence.
    // Most functions are about passing questions on to the SettlementManager.

    class ExpanderBody : MonoBehaviour, Hoverable, Interactable
    {

        public Piece m_piece;
        public TownshipManager m_tsManager;
        private ZNetView m_nview;

        public static List<ExpanderBody> AllExpanderBodies = new List<ExpanderBody>();

        public ZDO myZDO;
        public ZDOID myZDOID;
        public ExpanderSoul mySoul;


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

        private void Start()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
            m_tsManager = TownshipManager.Instance;

            if (m_piece.IsPlacedByPlayer() & m_nview.IsOwner() )
            {
                GetComponent<WearNTear>().m_onDestroyed += OnDestroyed;
                myZDO = m_nview.GetZDO();

                m_nview.SetPersistent(true);
                AllExpanderBodies.Add(this);

                m_nview.Register<bool>("changeActive", RPC_changeActive);
                m_nview.Register<bool, bool>("createNewSoul", RPC_changeConnection);

                if (!hasSoul)
                {
                    // no soul in ZDO, then a new expander;

                    // Populate the fresh ZDO that was just created with all the fun fields.
                    myZDO.Set("isActive", false);
                    myZDO.Set("isConnected", false);
                    myZDO.Set("position", m_piece.m_center);


                    // this operation can only be done by the server!
                    //mySoul = new ExpanderSoul(myZDO);
                    //ZRoutedRPC.instance.invoke(ZRoutedRPC.instance.GetServerPeerId, "CALLNAME", package);


                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "createNewSoul", myZDO);


                    hasSoul = true;
                } else
                {
                    // find the soul
                    foreach (SettlementManager setman in SettlementManager.AllSettleMans)
                    {
                        foreach(ExpanderSoul soul in setman.expanderSoulList)
                        {
                            if ( soul.myBodyZDOID == myZDOID )
                            {
                                mySoul = soul;
                                break;
                            }
                        }
                        if ( mySoul != null ) break;
                    }
                }
                Jotunn.Logger.LogDebug("Doing stuff to expander that was placed by a player");





                // how to register RPC
                //m_nview.Register<bool>("changeActive", RPC_changeActive);

                // how to invoke RPC
                // m_nview.InvokeRPC(ZNetView.Everybody, "changeActive", isActive);

                // note to self; this line sends an RPC call only to the server instance
                // ZRoutedRPC.instance.invoke(ZRoutedRPC.instance.GetServerPeerId, "CALLNAME", package)
            }
        }

        public void RPC_createNewSoul(long sender, ZDO myzdo)
        {
            if (Jotunn.ZNetExtension.IsServerInstance(ZNet.instance) || Jotunn.ZNetExtension.IsServerInstance(ZNet.instance))
            {
                mySoul = new ExpanderSoul(myZDO);
            }
        }

        public void RPC_changeConnection(long sender, bool toConnect, bool checkconnections = true)
        {

            if (Jotunn.ZNetExtension.IsServerInstance(ZNet.instance) || Jotunn.ZNetExtension.IsServerInstance(ZNet.instance))
            {
                mySoul.changeConnection(toConnect:toConnect, checkconnections:checkconnections);
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
                if ( myZDO.GetBool("isActive") )
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

        public void RPC_changeActive(long sender, bool toactive)
        {
            if (m_nview.IsOwner())
            {
                // change whether the Expander can connect and relay connections
                if (toactive)// && !isActive) // if true and false
                {
                    isActive = true;
                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "changeConnection", true, true);
                    //changeConnection(toConnect: true, checkconnections: true); // if I can connect, it'd be nice if I was
                    GetComponent<CraftingStation>().m_rangeBuild = 10;
                }
                else if (!toactive)// && isActive) // if false and true
                {
                    isActive = false;
                    //changeConnection(toConnect: false, checkconnections: true); // relinquesh connection when deactivatin
                    GetComponent<CraftingStation>().m_rangeBuild = 0;
                }
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            // if wacked with the "Stick of creating settlements", create a new settlement.
            return false;
        }

        public void OnDestroyed()
        {
            if( m_nview.IsOwner())
                if (Jotunn.ZNetExtension.IsServerInstance(ZNet.instance) || Jotunn.ZNetExtension.IsServerInstance(ZNet.instance))
                    mySoul.OnDestroyed();
        }

        public string GetHoverName()
        {
            // showing the name of the object
            if (mySoul.parentSettleMan != null)
            {
                return "Expander of " + myZDO.GetString("settlementName");
            } else
            {
                return "Expander of None";
            }
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\n Active: " + isActive +
                "\n Connected: " + isConnected +
                "\n Expanders: " + AllExpanderBodies.Count();
        }
    }
}
