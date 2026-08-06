using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(GiantKiwiAI))]
public static class GiantKiwiAIPatches
{
    [HarmonyPatch(nameof(GiantKiwiAI.IsEggInsideClosedTruck))]
    [HarmonyPrefix]
    static bool GiantKiwiAI_Pre_IsEggInsideClosedTruck(GiantKiwiAI __instance, KiwiBabyItem egg, bool closedTruck, ref bool __result, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        if (egg.parentObject != truckController.physicsRegion.parentNetworkObject.transform)
            return true;

        __result = VehicleUtils.IsTruckStorageEnclosed(truckController);
        return false;
    }

    [HarmonyPatch(nameof(GiantKiwiAI.AnimationEventB))]
    [HarmonyPrefix]
    static void GiantKiwiAI_Pre_AnimationEventB(GiantKiwiAI __instance)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (playerController == null ||
            !playerController.isPlayerControlled ||
            playerController.isPlayerDead)
            return;

        BoxCollider truckNavMeshBounds = truckController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInTruck = VehicleUtils.IsEnemyInTruck(__instance, truckNavMeshBounds);
        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(playerController, truckController);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(playerController, truckController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController);
        bool storageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);

        if (playerSeated)
        {
            if (VehicleUtils.IsSeatedPlayerProtectedByTruck(playerController, truckController, velocityCheck: true, velocityMagnitude: 13f))
                __instance.timeSinceHittingPlayer = 0f;
            return;
        }

        if (!playerOnTruck)
        {
            if (enemyInTruck)
                __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }

        bool protectedByStorage =
            (playerInStorage && storageEnclosed && !enemyInTruck) ||
            (!playerInStorage && enemyInTruck);

        if (protectedByStorage)
        {
            __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }

        if (enemyInTruck && playerInStorage)
            return;

        if (VehicleUtils.IsPlayerProtectedByTruck(playerController, truckController, velocityCheck: true, velocityMagnitude: 13f))
            __instance.timeSinceHittingPlayer = 0.4f;
    }
}