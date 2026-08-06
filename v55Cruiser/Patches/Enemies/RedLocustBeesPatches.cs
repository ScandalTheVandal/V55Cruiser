using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(RedLocustBees))]
public static class RedLocustBeesPatches
{
    [HarmonyPatch(nameof(RedLocustBees.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static void RedLocustBees_Pre_OnCollideWithPlayer(RedLocustBees __instance, Collider other)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerController == null)
            return;

        BoxCollider truckNavMeshBounds = truckController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInTruck = VehicleUtils.IsEnemyInTruck(__instance, truckNavMeshBounds);
        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(playerController, truckController);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(playerController, truckController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController);
        bool storageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);

        if (playerSeated)
            return;

        if (!playerOnTruck)
        {
            if (enemyInTruck)
                __instance.timeSinceHittingPlayer = 0f;
            return;
        }

        bool protectedByStorage =
            (playerInStorage && storageEnclosed && !enemyInTruck) ||
            (!playerInStorage && enemyInTruck);

        if (protectedByStorage)
        {
            __instance.timeSinceHittingPlayer = 0f;
            return;
        }

        if (enemyInTruck && playerInStorage)
            return;

        if (VehicleUtils.IsPlayerProtectedByTruck(playerController, truckController, velocityCheck: true, velocityMagnitude: 5f))
            __instance.timeSinceHittingPlayer = 0f;
    }
}