using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(MaskedPlayerEnemy))]
public static class MaskedPlayerEnemyPatches
{
    [HarmonyPatch(nameof(MaskedPlayerEnemy.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool MaskedPlayerEnemy_Pre_OnCollideWithPlayer(MaskedPlayerEnemy __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = 
            __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation || __instance.startingKillAnimationLocalClient || !__instance.enemyEnabled, false);
        if (playerController == null)
            return true;

        BoxCollider truckNavMeshBounds = truckController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInTruck = VehicleUtils.IsEnemyInTruck(__instance, truckNavMeshBounds);
        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(playerController, truckController);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(playerController, truckController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController);
        bool storageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtectedByTruck(playerController, truckController, velocityCheck: false, velocityMagnitude: 0f);

        if (!playerOnTruck)
            return !enemyInTruck;

        bool protectedInStorage =
            (playerInStorage && storageEnclosed && !enemyInTruck) ||
            (!playerInStorage && enemyInTruck);

        if (protectedInStorage)
            return false;

        if (enemyInTruck && playerInStorage)
            return true;

        return !VehicleUtils.IsPlayerProtectedByTruck(playerController, truckController, velocityCheck: true, velocityMagnitude: 5f);
    }
}