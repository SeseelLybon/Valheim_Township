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
    class SMAI : MonoBehaviour, Hoverable, Interactable
    {

        public string m_name = "Heart of the settlement";
        public string m_name_settlement = "Boar";

        public bool isActive = false;

        public int Happiness = 100;
        public int Expanders = 0;
        public int Population = 0;

        private Piece m_piece;



        private void Awake()
        {
            //Jotunn.Logger.LogWarning($"Awakeing SMAI");
        }

        private void Start()
        {
            //Jotunn.Logger.LogWarning($"Starting SMAI");
        }

        



        private void think()
        {
            Happiness -= 1;
            Jotunn.Logger.LogMessage("Thinking...");
        }



        public string GetHoverName()
        {
            // showing the name of the object
            return "Heart of " + m_name_settlement;
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return m_name + " " + isActive.ToString() + " " + Happiness.ToString();
        }

        public bool Interact(Humanoid user, bool hold)
        {
            // Show SMAI gui on press
            // Let remane on hold?

            if ( hold )
            {

                if (isActive)
                {
                    isActive = true;
                    InvokeRepeating("think", 5f, 10f);
                }else
                {
                    isActive = false;
                    CancelInvoke("think");
                }

            }
            if( !hold )
            {
                Happiness += 1;
            }
            return false;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
