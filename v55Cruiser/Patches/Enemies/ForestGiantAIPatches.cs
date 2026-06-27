using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(ForestGiantAI))]
public static class ForestGiantAIPatches
{
    [HarmonyPatch(nameof(ForestGiantAI.AnimationEventA))]
    [HarmonyPrefix]
    static bool AnimationEventA_Prefix(ForestGiantAI __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB == null)
            return false;

        // do not allow fall death in the trucks storage compartment
        if (VehicleUtils.IsPlayerInTruckStorage(truckController: controller))
            return false;

        // not in our truck, run vanilla logic
        return true;
    }

    [HarmonyPatch(nameof(ForestGiantAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(ForestGiantAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inEatingPlayerAnimation, false);
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