using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(HUDManager))]
public static class HUDManagerPatches
{
    [HarmonyPatch(nameof(HUDManager.HelmetCondensationDrops))]
    [HarmonyPostfix]
    private static void HelmetCondensationDrops_Postfix(HUDManager __instance)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return;

        if (VehicleUtils.IsPlayerInTruckStorage(controller))
        {
            __instance.increaseHelmetCondensation = false;
        }
    }
}
