using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using HarmonyLib;
using Unity;
using MelonLoader;
using Il2CppTLD.Gear;

namespace ImprovedFires.Patches
{
    internal class FireUIPatches
    {

        //[HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.FilterItem))]
        //public class testshit
        //{
        //    public static void Postfix(ref bool __result, ref GearItem gi)
        //    {
        //        if (GameManager.GetSkillFireStarting().GetCurrentTierNumber() > 4) return;
        //        if (gi.name.ToLowerInvariant().Contains("hardwood") || gi.name.ToLowerInvariant().Contains("softwood") || gi.name.ToLowerInvariant().Contains("firelog")) __result = false;
        //    }
        //}
    }
}
