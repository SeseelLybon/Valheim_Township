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

        public string settlementName;


        public Piece m_piece;
        public TownshipManager m_tsManager;

        public static List<ExpanderBody> m_AllExpanderBodies = new List<ExpanderBody>();

        public ExpanderSoul mySoul;

        private ZNetView m_nview;

        public ZDO expanderbodyZDO;

        public bool isActive
        {
            get { return expanderbodyZDO.GetBool("isActive"); }
            set { expanderbodyZDO.Set("isActive", value); }
        }

        public bool isConnected
        {
            get { return expanderbodyZDO.GetBool("isConnected"); }
            set { expanderbodyZDO.Set("isConnected", value); }
        }


        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
            m_tsManager = TownshipManager.Instance;
            GetComponent<WearNTear>().m_onDestroyed += OnDestroyed;
        }

        private void Start()
        {
            if (m_piece.IsPlacedByPlayer())
            {
                
                // if ZDO.getbool wasPlaced == true
                // Skip over specific initializer stuff
                // else
                // ZDO.set wasPlaced == true;
                // do specific initializer stuffs


                Jotunn.Logger.LogDebug("Doing stuff to expander that was placed by a player");


                m_nview.SetPersistent(true);

                m_AllExpanderBodies.m_AllExpanders.Add(this);

                


                expanderbodyZDO = m_nview.GetZDO();

                expanderbodyZDO.Set("isActive", expanderbodyZDO.GetBool("isActive", false));
                expanderbodyZDO.Set("position", expanderbodyZDO.GetVec3("position", m_piece.m_center ));

                m_nview.Register<bool>("changeActive", RPC_changeActive);


                m_nview.InvokeRPC( ZNetView.Everybody, "changeActive", isActive);
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
                if ( expanderbodyZDO.GetBool("isActive") )
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
                    changeConnection(toConnect: true, checkconnections: true); // if I can connect, it'd be nice if I was
                    GetComponent<CraftingStation>().m_rangeBuild = 10;
                }
                else if (!toactive)// && isActive) // if false and true
                {
                    isActive = false;
                    changeConnection(toConnect: false, checkconnections: true); // relinquesh connection when deactivatin
                    GetComponent<CraftingStation>().m_rangeBuild = 0;
                }
            }
        }

        public void changeConnection(bool toConnect, bool checkconnections = true)
        {
            //Change whether the Expander can connect in this moment

            SettlementManager tempSetMan = m_tsManager.PosInWhichSettlement(expanderbodyZDO.GetVec3("position", Vector3.zero));

            if (expanderbodyZDO.GetBool("isActive") && toConnect && tempSetMan != null)
            {
                parentSettleMan = tempSetMan;
                expanderbodyZDO.Set("isConnected", true);
                Jotunn.Logger.LogInfo("Connecting Expander");
            }
            else if (!toConnect)
            {
                parentSettleMan = null;
                expanderbodyZDO.Set("isConnected", false);
                Jotunn.Logger.LogInfo("Disconnecting Expander");
            }
            else if (tempSetMan == null)
            {
                parentSettleMan = null;
                expanderbodyZDO.Set("isConnected", false);
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

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
            // invoke the move thingy here with... an item... somehow?
        }

        public void OnDestroyed()
        {
            mySoul.onDestroyed();
        }

        public string GetHoverName()
        {
            // showing the name of the object
            if (parentSettleMan != null)
            {
                return "Expander of " + parentSettleMan.settlementName;
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
                "\n Expanders: " + m_AllExpanders.Count();
        }


    }
}
