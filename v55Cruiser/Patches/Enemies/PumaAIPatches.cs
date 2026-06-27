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
    static bool OnCollideWithPlayer_Prefix(PumaAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerControllerB == null)
            return true;

        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, truckController: controller, velocityCheck: true, velocityMagnitude: 10f))
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
            if (VehicleUtils.IsPlayerProtectedByTruck(playerController: playerControllerB, truckController: controller, velocityCheck: true, velocityMagnitude: 10f))
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