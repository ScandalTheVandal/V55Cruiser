using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(MouthDogAI))]
public static class MouthDogAIPatches
{
    [HarmonyPatch(nameof(MouthDogAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(MouthDogAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation, false);
        if (playerControllerB == null)
            return true;

        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, truckController: controller, velocityCheck: false, velocityMagnitude: 0f))
            {
                return false;
            }
            return true;
        }
        if (VehicleUtils.IsPlayerInTruckBounds(truckController: controller))
        {
            if (VehicleUtils.IsPlayerProtectedByTruck(playerController: playerControllerB, truckController: controller, velocityCheck: false, velocityMagnitude: 0f))
            {
                return false;
            }
            return true;
        }
        return true;
    }
}
