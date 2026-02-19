using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(ItemDropship))]
public static class ItemDropshipPatches
{
    [HarmonyPatch(nameof(ItemDropship.Start))]
    [HarmonyPrefix]
    private static void Start_Postfix(ItemDropship __instance)
    {
        if (__instance == null) 
            return;

        References.itemShip = __instance;
    }
}
