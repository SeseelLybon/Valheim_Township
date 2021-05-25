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
        public static List<ExpanderSoul> m_AllExpanderSouls = new List<ExpanderSoul>();

        public SettlementManager parentSettleMan; // the SettlementManager that is connected to this expander
        public TownshipManager m_tsManager;

        public ZDO myBodyZDO;
        public ExpanderBody myBody;

        public Vector3 position
        {
            get { return myBodyZDO.GetVec3("position", Vector3.zero );      }
            set { myBodyZDO.Set("position", myBody.m_piece.GetCenter() );   } // if this causes a NRE, then the call itself was bad
        }

        public ExpanderSoul( ZDO mybodyZDO)
        {
            myBodyZDO = mybodyZDO;
        }


        // called by myBody when it gets destroyed.
        // Soul also gets destroyed
        public void OnDestroyed()
        {
            return;
        }
    }
}
