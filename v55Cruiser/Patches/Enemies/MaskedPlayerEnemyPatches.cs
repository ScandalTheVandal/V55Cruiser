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
    static bool OnCollideWithPlayer_Prefix(MaskedPlayerEnemy __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation || __instance.startingKillAnimationLocalClient || !__instance.enemyEnabled, false);
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
        bool enemyInVan = VehicleUtils.IsEnemyInTruck(enemyScript: __instance, truckController: controller);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(truckController: controller);
        bool backDoorsOpen = controller.liftGateOpen;
        if (VehicleUtils.IsPlayerInTruckBounds(truckController: controller))
        {
            if (playerInStorage && !backDoorsOpen && !enemyInVan || !playerInStorage && enemyInVan)
            {
                return false;
            }
            if (VehicleUtils.IsPlayerProtectedByTruck(playerController: playerControllerB, truckController: controller, velocityCheck: true, velocityMagnitude: 5f))
            {
                return false;
            }
            return true;
        }
        if (enemyInVan)
        {
            return false;
        }
        return true;
    }
}