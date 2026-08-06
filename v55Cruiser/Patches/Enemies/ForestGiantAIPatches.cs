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
    static bool ForestGiantAI_Pre_AnimationEventA(ForestGiantAI __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return false;

        return true;
    }

    [HarmonyPatch(nameof(ForestGiantAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool ForestGiantAI_Pre_OnCollideWithPlayer(ForestGiantAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = 
            __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inEatingPlayerAnimation, false);
        if (playerController == null)
            return true;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController);
        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(playerController, truckController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtectedByTruck(playerController, truckController, velocityCheck: false, velocityMagnitude: 0f);

        if (!playerOnTruck)
            return true;

        return !VehicleUtils.IsPlayerProtectedByTruck(playerController, truckController, velocityCheck: false, velocityMagnitude: 0f);
    }
}