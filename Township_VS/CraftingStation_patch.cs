using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Township.patch
{
    static class CraftingStation_patch
    {
        public static bool firstInit = true;
        public static void init()
        {
            if (firstInit)
            {
                On.CraftingStation.Start += CraftingStation_Start;
                On.Piece.DropResources += Piece_DropResources;
                firstInit = false;
            }
        }

        private static void CraftingStation_Start(On.CraftingStation.orig_Start orig, CraftingStation self)
        {
            orig(self);
            if (self.m_nview != null && self.m_nview.GetZDO() != null)
            {
                var isinsettlement = SettlementManager.PosInWhichSettlement(self.m_nview.GetZDO().m_position);
                if (isinsettlement != null)
                {
                    isinsettlement.enableCraftinStationProxy(self.m_name, self.m_nview.GetZDO().m_uid);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private static void Piece_DropResources(On.Piece.orig_DropResources orig, Piece self)
        //private void Piece_OnDestroy(On.Piece.orig_OnDestroy orig, Piece self)
        //private void CraftingStation_OnDestroy(On.CraftingStation.orig_OnDestroy orig, CraftingStation self)
        {
            orig(self);

            CraftingStation tempCS;
            self.TryGetComponent<CraftingStation>(out tempCS);
            if (tempCS != null)
            {
                // only destroy if the object is destroyed, not when unloaded
                var isinsettlement = SettlementManager.PosInWhichSettlement(self.m_nview.GetZDO().m_position);
                if (isinsettlement != null)
                {
                    Jotunn.Logger.LogDebug("Removing Craftingstation (hopefully)");
                    isinsettlement.disableCraftinStationProxy(self.m_name, self.GetComponent<CraftingStation>().m_nview.GetZDO().m_uid);
                }
            }
        }
    }
}
