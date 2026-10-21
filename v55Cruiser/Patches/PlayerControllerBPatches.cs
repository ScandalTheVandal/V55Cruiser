using GameNetcodeStuff;
using HarmonyLib;
using v55Cruiser.Utils;


namespace v55Cruiser.Patches;

[HarmonyPatch]
public static class PlayerControllerBPatches
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SetItemInElevator))]
    [HarmonyPrefix]
    static bool PlayerControllerB_Pre_SetItemInElevator(PlayerControllerB __instance, bool droppedInShipRoom, bool droppedInElevator, GrabbableObject gObject)
    {
        v55VehicleController vehicle = VehicleUtils.truckController;
        if (vehicle == null)
            return true;

        if (gObject.transform.parent == vehicle.transform)
            return false;
        return true;
    }
}