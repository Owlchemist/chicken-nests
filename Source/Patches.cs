using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
 
namespace AnimalBeds
{
    [StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
        public static HashSet<ushort> animalBeds = new HashSet<ushort>();
        static HarmonyPatches()
        {
            new Harmony("owlchemist.chickennests").PatchAll();
            animalBeds = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasModExtension<AnimalBed>()).Select(y => y.index).ToHashSet();
        }
    }
    //Process the shinies on the active map
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn_Bed.AssigningCandidates), MethodType.Getter)]
    public static class Patch_CompAssignableToPawn_Bed
    {
        static IEnumerable<Pawn> Postfix(IEnumerable<Pawn> values, CompAssignableToPawn_Bed __instance)
        {
            var ext = __instance.parent.def.GetModExtension<AnimalBed>();
            //Fast return is irrelevant
            if (ext == null) foreach (var value in values) yield return value;
            //Else begin filtering process
            else
            {
                foreach (var value in values) {
                    if (ext.species?.Contains(value.def) ?? true) yield return value;
                }    
            }
        }
    }

    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanUseBedEver))]
    public static class Patch_CanUseBedEver
    {
        static bool Postfix(bool __result, Pawn p, ThingDef bedDef)
        {
            //This is an animal and they were allowed to use this bed. Double check with our rules
            if (__result && p.def.race.intelligence != Intelligence.Humanlike && HarmonyPatches.animalBeds.Contains(bedDef.index))
            {
                var ext = bedDef.GetModExtension<AnimalBed>();
                //Does this bed have an extension?
                if (ext != null && (!ext.species?.Contains(p.def) ?? false)) __result = false;
            }
            return __result;
        }
    }
}