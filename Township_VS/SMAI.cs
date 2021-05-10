﻿using System;
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

        private Piece m_piece;

        private ZNetView m_nview;

        private void Awake()
        {
            //Jotunn.Logger.LogWarning($"Awakeing SMAI");

            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
            m_nview.m_persistent = true;



        }

        private void Start()
        {
            if (m_piece.IsPlacedByPlayer())
            {
                m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness", 95));
            }
        }


        public void think()
        {
            m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness") -1);
            Jotunn.Logger.LogInfo("thinking...");
        }


        public string GetHoverName()
        {
            // showing the name of the object
            return "Heart of " + m_name_settlement;
        }

        public string GetHoverText()
        {
            // for the ward it's things like is_active and stuff.
            return m_name +
                "\nActive: " + isActive.ToString() +
                "\nHappiness:" + m_nview.GetZDO().GetInt("Happiness").ToString();
        }

        public bool Interact(Humanoid user, bool hold)
        {
            // Show SMAI gui on press
            // Let remane on hold?

            if ( !hold )
            {

                if (isActive)
                {
                    isActive = false;
                    CancelInvoke("think");
                }
                else
                {
                    isActive = true;
                    InvokeRepeating("think", 5f, 10f);
                }

            }else if( hold )
            {
                m_nview.GetZDO().Set("Happiness", m_nview.GetZDO().GetInt("Happiness") + 1);
            }

            return false;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}