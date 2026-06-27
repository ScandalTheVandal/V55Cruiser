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
    static void OnCollideWithPlayer_Prefix(RedLocustBees __instance, Collider other)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerControllerB == null)
            return;

        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            return;
        }

        bool enemyInVan = VehicleUtils.IsEnemyInTruck(enemyScript: __instance, truckController: controller);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(truckController: controller);
        bool backDoorsOpen = controller.liftGateOpen;
        if (VehicleUtils.IsPlayerInTruckBounds(truckController: controller))
        {
            if (playerInStorage && !backDoorsOpen && !enemyInVan || !playerInStorage && enemyInVan)
            {
                __instance.timeSinceHittingPlayer = 0f;
                return;
            }
            if (VehicleUtils.IsPlayerProtectedByTruck(playerController: playerControllerB, truckController: controller, velocityCheck: true, velocityMagnitude: 5f))
            {
                __instance.timeSinceHittingPlayer = 0f;
                return;
            }
            return;
        }
        if (enemyInVan)
        {
            __instance.timeSinceHittingPlayer = 0f;
            return;
        }
    }
}