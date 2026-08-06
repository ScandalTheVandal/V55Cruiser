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
    static bool MouthDogAI_Pre_OnCollideWithPlayer(MouthDogAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = 
            __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation, false);
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
