using System.Linq;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(Terminal))]
public static class TerminalPatches
{
    [HarmonyPatch(nameof(Terminal.Awake))]
    [HarmonyPrefix]
    private static void Awake_Prefix(Terminal __instance)
    {
        // make the Cruiser 400 credits, like in version-55
        TerminalNode result = __instance.terminalNodes.allKeywords[0].compatibleNouns[40].result;
        result.itemCost = 400;
        result.terminalOptions[0].result.itemCost = 400;
        __instance.buyableVehicles[0].creditsWorth = 400;

        __instance.buyableVehicles[0].vehiclePrefab = Plugin.CompanyCruiserPrefab;
        __instance.buyableVehicles[0].secondaryPrefab = Plugin.CompanyCruiserManualPrefab;
    }
}
