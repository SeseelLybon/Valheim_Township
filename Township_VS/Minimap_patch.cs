﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using UnityEngine;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using Logger = Jotunn.Logger;


namespace Township.patch
{
    public static class Minimap_patch
    {

        public static bool firstInit = true;
        public static void init()
        {
            if (firstInit)
            {
                On.Minimap.UpdateDynamicPins += Minimap_UpdateDynamicPins;
                On.Minimap.OnDestroy += Minimap_OnDestroy;
                On.Minimap.Awake += Minimap_Awake;
            }
        }

        public const float AreaScale = 2.1f;


        // a pin+area for each expander
        public static Dictionary<ZDOID, Minimap.PinData> ExpanderPins = new Dictionary<ZDOID, Minimap.PinData>();
        // a pin+name for each settlement using the averaged center
        public static Dictionary<ZDOID, Minimap.PinData> SettlementPins = new Dictionary<ZDOID, Minimap.PinData>();

        private static void Minimap_Awake(On.Minimap.orig_Awake orig, Minimap self)
        {
            orig(self);

            // m_icons.Add(new Minimap.SpriteData { m_name = EpicLoot.TreasureMapPinType, m_icon = EpicLoot.Assets.MapIconTreasureMap });

        }

        private static void Minimap_OnDestroy(On.Minimap.orig_OnDestroy orig, Minimap self)
        {
            orig(self);
            ExpanderPins.Clear();
            SettlementPins.Clear();
        }

        static int updateinterval = 1; //seconds
        static float updatenextTime = 0;
        private static void Minimap_UpdateDynamicPins(On.Minimap.orig_UpdateDynamicPins orig, Minimap self, float dt)
        {
            orig(self, dt);



            if (Time.time >= updatenextTime)
            {



                /// Code for the expanders area's
                List<ZDO> allExpanderZDOs = new List<ZDO>();
                ZDOMan.instance.GetAllZDOsWithPrefab(TownshipManager.instance.expanderprefabname, allExpanderZDOs);

                foreach (ZDO expanderzdo in allExpanderZDOs)
                {
                    if (expanderzdo.GetBool(Expander.showOnMinimap))
                    {
                        // only add if not already shown
                        if (!ExpanderPins.ContainsKey(expanderzdo.m_uid))
                        {
                            Jotunn.Logger.LogInfo("Showing pin " + expanderzdo.m_uid);
                            var position = expanderzdo.GetVec3(Expander.position, Vector3.zero);
                            // start showing
                            var pin = self.AddPin(position, Minimap.PinType.EventArea, string.Empty, false, false); // adds red circle
                            pin.m_worldSize = 50; // some random number, should match up with the extender's SoI
                            ExpanderPins.Add(expanderzdo.m_uid, pin);
                        }
                    }
                    else
                    {
                        if (ExpanderPins.ContainsKey(expanderzdo.m_uid))
                        {
                            Jotunn.Logger.LogInfo("Removing pin " + expanderzdo.m_uid);
                            // stop showing
                            self.RemovePin(ExpanderPins[expanderzdo.m_uid]);
                            ExpanderPins.Remove(expanderzdo.m_uid);
                        }
                    }
                }


                /// Code for the settlements labels
                List<ZDO> allSettlementZDOs = new List<ZDO>();
                ZDOMan.instance.GetAllZDOsWithPrefab(TownshipManager.instance.settlemanangerprefabname, allSettlementZDOs);

                foreach (ZDO settlemanZDO in allSettlementZDOs)
                {
                    if (settlemanZDO.GetBool(Expander.showOnMinimap))
                    {
                        // only add if not already shown
                        if (!SettlementPins.ContainsKey(settlemanZDO.m_uid))
                        {
                            Jotunn.Logger.LogInfo("Showing pin " + settlemanZDO.m_uid);
                            var position = settlemanZDO.GetVec3("centerofSOI", Vector3.zero);
                            // start showing
                            var pin = self.AddPin(position, Minimap.PinType.Icon1, settlemanZDO.GetString("settlementName"), false, false); // adds red circle
                            //pin.m_worldSize = 50; // some random number, should match up with the extender's SoI
                            SettlementPins.Add(settlemanZDO.m_uid, pin);
                        }
                    }
                    else
                    {
                        if (SettlementPins.ContainsKey(settlemanZDO.m_uid))
                        {
                            Jotunn.Logger.LogInfo("Removing pin " + settlemanZDO.m_uid);
                            // stop showing
                            self.RemovePin(SettlementPins[settlemanZDO.m_uid]);
                            SettlementPins.Remove(settlemanZDO.m_uid);
                        }
                    }
                }
                updatenextTime += updateinterval;
            }
        }

        public static void ShowAllExpanders()
        {
            List<ZDO> allExpanderZDOs = new List<ZDO>();
            ZDOMan.instance.GetAllZDOsWithPrefab(TownshipManager.instance.expanderprefabname, allExpanderZDOs);

            foreach (ZDO expanderzdo in allExpanderZDOs)
            {
                expanderzdo.Set(Expander.showOnMinimap, true);
            }
        }

        public static void HideAllExpanders()
        {
            List<ZDO> allExpanderZDOs = new List<ZDO>();
            ZDOMan.instance.GetAllZDOsWithPrefab(TownshipManager.instance.expanderprefabname, allExpanderZDOs);

            foreach (ZDO expanderzdo in allExpanderZDOs)
            {
                expanderzdo.Set(Expander.showOnMinimap, false);
            }
        }



        ///
    }
}