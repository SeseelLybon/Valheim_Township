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

    class Expander : MonoBehaviour, Hoverable, Interactable
    {

        public string m_name = "Expander";

        public string settlementName;


        public Piece m_piece;
        public TownshipManager m_tsManager;

        private bool isPlaced = false; // if the Piece was placed before Awake/Start placed in the world (aka loaded from save data rather than placed by the player

        public SettlementManager parentSMAI; // the SettlementManager that is connected to this expander

        public bool isConnected =  false;   // where the Expander has a connection
        public bool isActive = false;       // whether the Expander can recieve connections

        public bool isOwned; // whether the 

        public static List<Expander> m_AllExpanders = new List<Expander>();

        private ZNetView m_nview;



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

                m_AllExpanders.Add(this);

                isActive = m_nview.GetZDO().GetBool("isActive", false);
                m_nview.GetZDO().Set("isActive", isActive);

                changeActive(isActive);


                /*
                 * // if this object was loaded from save, and it was active
                // i
                 */

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
                if (isActive == true)
                {
                    changeActive(false);
                    return true;
                }
                else
                {
                    changeActive(true);
                    return true;
                }
            }

            // if !hold if active call gui, else throw soft warning
            // if hold toggle active

            return false;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
            // invoke the move thingy here with... an item... somehow?
        }

        public void changeActive(bool toactive)
        {
            // change whether the Expander can connect and relay connections
            if (toactive)// && !isActive) // if true and false
            {
                m_nview.GetZDO().Set("isActive", true);
                isActive = true;
                changeConnection(toConnect:true, checkconnections:true); // if I can connect, it'd be nice if I was
                GetComponent<CraftingStation>().m_rangeBuild = 10; // doesn't work
            }
            else if (!toactive)// && isActive) // if false and true
            {
                m_nview.GetZDO().Set("isActive", false);
                isActive = false;
                changeConnection(toConnect:false, checkconnections:true); // relinquesh connection when deactivatin
                GetComponent<CraftingStation>().m_rangeBuild = 0; // doesn't work
            }
        }

        public void changeConnection(bool toConnect, bool checkconnections = true)
        {
            //Change whether the Expander can connect in this moment

            SettlementManager SMAIasdf = m_tsManager.PosInWhichSettlement(m_piece.GetCenter());

            if (isActive && toConnect && SMAIasdf != null && SMAIasdf.isActive)
            {
                parentSMAI = SMAIasdf;
                isConnected = true;
                Jotunn.Logger.LogInfo("Connecting Expander");
            }
            else if (!toConnect)
            {
                parentSMAI = null;
                isConnected = false;
                Jotunn.Logger.LogInfo("Disconnecting Expander");
            }
            else if (SMAIasdf == null)
            {
                parentSMAI = null;
                isConnected = false;
                Jotunn.Logger.LogInfo("No nearby or active Settlement found!");
            } else
            {
                Jotunn.Logger.LogFatal("Expander.changeConnection() had an impossible outcome");
                Jotunn.Logger.LogFatal("\t " + isActive);
                Jotunn.Logger.LogFatal("\t " + isConnected);
                Jotunn.Logger.LogFatal("\t " + toConnect);
                Jotunn.Logger.LogFatal("\t " + (parentSMAI?"null":parentSMAI.settlementName));
                Jotunn.Logger.LogFatal("\t " + (SMAIasdf ? "null" : parentSMAI.settlementName));
            }

            if (checkconnections && SMAIasdf != null )
            {
                SMAIasdf.checkConnectionstoHeart();
            }

        }



        public void OnDestroyed()
        {
            if(parentSMAI)
                parentSMAI.checkConnectionstoHeart();
            m_AllExpanders.Remove(this);
        }

        public string GetHoverName()
        {
            // showing the name of the object
            if (parentSMAI != null)
            {
                return "Expander of " + parentSMAI.settlementName;
            } else
            {
                return "Expander of None";
            }
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return GetHoverName() +
                "\n Active: " + m_nview.GetZDO().GetBool("isActive") +
                "\n Connected: " + isConnected +
                "\n Expanders: " + m_AllExpanders.Count();
        }


    }
}
