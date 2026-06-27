using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(RadMechAI))]
public static class RadMechAIPatches
{
    [HarmonyPatch(nameof(RadMechAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(RadMechAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerControllerB == null)
            return true;

        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            return true;
        }
        if (VehicleUtils.IsPlayerInTruckBounds(truckController: controller))
        {
            if (VehicleUtils.IsPlayerProtectedByTruck(playerController: playerControllerB, truckController: controller))
            {
                return false;
            }
            return true;
        }
        return true;
    }
}