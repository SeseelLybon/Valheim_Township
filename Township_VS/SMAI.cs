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
    class SMAI : MonoBehaviour
    {

        public string settlementName;

        private Piece m_piece;
        private bool isPlaced = false;

        private ZNetView m_nview;

        // public List<Expanders> expandersList; // list of expander totems connected to this SMAI

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
        }

        private void Start()
        {
            // A tricky system to deal with the fact that I have to save/network variables,
            //  but also deal with active development and updates
            // Either I keep overwriting values, throw errors in the log or just make useless actions
            // the isPlaced is a hack because the game doesn't track this itself appearantly.
            // Gotta poke Jotunn to use something similar and also a "isPlaced" call like Start but for when a Piece is placed and not a ghost anymore.

            if ( m_piece.IsPlacedByPlayer() )
            {
                isPlaced = true;
            }

            if (isPlaced)
            {
                Jotunn.Logger.LogDebug("Doing stuff to object that was placed by a player");

                m_nview.SetPersistent(true);


                // Rather ugly code, gotta do something about it later
                // fetch var, if var not there use default, then set fetched var or default.
                m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness", 100));


                settlementName = m_nview.GetZDO().GetString("settlementName", "Elktown");
                m_nview.GetZDO().Set("settlementName", settlementName);


            }
        }

        public void think()
        {
            m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness") -1);
            Jotunn.Logger.LogInfo("thinking...");
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }



        /*
        public void registerExpanderTotem()
        {
            // add totem to the list of totems
        }
        */

        /*
        public void unregisterExpanderTotem()
        {
            // remove totem from list
            // if last totem, destroy SMAI (and tell Township the world has a settlement less)
        }
        */



        /*
         * //is it even possible to keep a list of objects that extend from Definer?
         * 
        public void registerWorkshopDefinerTotem()
        public void registerWarehouseDefinerTotem()
        public void registerHouseDefinerTotem()
        public void registerDockDefinerTotem()
        public void registerIndustrialDefinerTotem()
        public void registerInnDefinerTotem()
        public void registerFarmlandDefinerTotem()
        public void registerWallDefinerTotem()
        public void registerGateDefinerTotem()
        public void registerRoadDefinerTotem()
        public void registerMineDefinerTotem()
        public void registerDefinerTotem()

        {
            // add totem to the list of totems
        }
        */

    }
}
