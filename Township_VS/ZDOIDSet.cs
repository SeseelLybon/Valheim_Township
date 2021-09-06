using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Township
{

    // Code taken from MacroPogo's Planbuild

    // blueprintZDO.Set(PlanPiece.zdoBlueprintPiece, planPieces.ToZPackage().GetArray());


    class ZDOIDSet : HashSet<ZDOID>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="package">Gained from ZDO.GetByteArray() </param>
        /// <returns></returns>
        public static ZDOIDSet From(ZPackage package)
        {
            ZDOIDSet result = new ZDOIDSet();
            int size = package.ReadInt();
            for (int i = 0; i < size; i++)
            {
                result.Add(package.ReadZDOID());
            }
            return result;
        }

        /// <summary>
        /// Turns everything in this set into a ZPackage that can be set with ZDO.Set();
        /// </summary>
        /// <returns></returns>
        public ZPackage ToZPackage()
        {
            var package = new ZPackage();
            package.Write(this.Count());
            foreach(ZDOID zdoid in this)
            {
                package.Write(zdoid);
            }
            return package;
        }
    }
}
