using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(PumaAI))]
public static class PumaAIPatches
{
    [HarmonyPatch(nameof(PumaAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool PumaAI_Pre_OnCollideWithPlayer(PumaAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = 
            __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerController == null)
            return true;

        BoxCollider truckNavMeshBounds = truckController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInTruck = VehicleUtils.IsEnemyInTruck(__instance, truckNavMeshBounds);
        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(playerController, truckController);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(playerController, truckController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController);
        bool storageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtectedByTruck(playerController, truckController, velocityCheck: true, velocityMagnitude: 10f);

        if (!playerOnTruck)
            return !enemyInTruck;

        bool protectedByStorage =
            (playerInStorage && storageEnclosed && !enemyInTruck) ||
            (!playerInStorage && enemyInTruck);

        if (protectedByStorage)
            return false;

        if (enemyInTruck && playerInStorage)
            return true;

        return !VehicleUtils.IsPlayerProtectedByTruck(playerController, truckController, velocityCheck: true, velocityMagnitude: 10f);
    }
}