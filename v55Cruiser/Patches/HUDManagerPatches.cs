using GameNetcodeStuff;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(HUDManager))]
public static class HUDManagerPatches
{
    [HarmonyPatch(nameof(HUDManager.HelmetCondensationDrops))]
    [HarmonyPostfix]
    private static void HUDManager_Post_HelmetCondensationDrops(HUDManager __instance)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            __instance.increaseHelmetCondensation = false;
    }
}
