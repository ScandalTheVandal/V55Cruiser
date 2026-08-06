using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(Terminal))]
public static class TerminalPatches
{
    [HarmonyPatch(nameof(Terminal.Awake))]
    [HarmonyPrefix]
    private static void Terminal_Pre_Awake(Terminal __instance)
    {
        TerminalNode result = __instance.terminalNodes.allKeywords[0].compatibleNouns[40].result;
        result.itemCost = 400;
        result.terminalOptions[0].result.itemCost = 400;
        __instance.buyableVehicles[0].creditsWorth = 400;

        __instance.buyableVehicles[0].vehiclePrefab = References.companyCruiserPrefab;
        __instance.buyableVehicles[0].secondaryPrefab = References.companyCruiserManualPrefab;
    }
}