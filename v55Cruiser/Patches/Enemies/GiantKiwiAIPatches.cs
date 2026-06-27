using GameNetcodeStuff;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(GiantKiwiAI))]
public static class GiantKiwiAIPatches
{
    [HarmonyPatch(nameof(GiantKiwiAI.IsEggInsideClosedTruck))]
    [HarmonyPrefix]
    static bool IsEggInsideClosedTruck_Prefix(GiantKiwiAI __instance, KiwiBabyItem egg, bool closedTruck, ref bool __result, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        if (egg.parentObject == controller.physicsRegion.parentNetworkObject.transform)
        {
            __result = !controller.liftGateOpen;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(GiantKiwiAI.AnimationEventB))]
    [HarmonyPrefix]
    static void AnimationEventB_Prefix(GiantKiwiAI __instance)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return;

        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB == null ||
            !playerControllerB.isPlayerControlled ||
            playerControllerB.isPlayerDead)
            return;


        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, truckController: controller, velocityCheck: true, velocityMagnitude: 13f))
            {
                __instance.timeSinceHittingPlayer = 0f;
            }
            return;
        }

        bool enemyInVan = VehicleUtils.IsEnemyInTruck(enemyScript: __instance, truckController: controller);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(truckController: controller);
        bool backDoorsOpen = controller.liftGateOpen;
        if (VehicleUtils.IsPlayerInTruckBounds(truckController: controller))
        {
            if (playerInStorage && !backDoorsOpen && !enemyInVan || !playerInStorage && enemyInVan)
            {
                __instance.timeSinceHittingPlayer = 0.4f;
                return;
            }
            if (VehicleUtils.IsPlayerProtectedByTruck(playerController: playerControllerB, truckController: controller, velocityCheck: true, velocityMagnitude: 13f))
            {
                __instance.timeSinceHittingPlayer = 0.4f;
                return;
            }
            return;
        }
        if (enemyInVan)
        {
            __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }
    }
}