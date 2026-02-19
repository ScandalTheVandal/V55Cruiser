using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(GiantKiwiAI))]
internal static class GiantKiwiAIPatches
{
    [HarmonyPatch(nameof(GiantKiwiAI.NavigateTowardsTargetPlayer))]
    [HarmonyPrefix]
    static bool NavigateTowardsTargetPlayer_Prefix(GiantKiwiAI __instance)
    {
        if (References.truckController == null)
            return true;
        v55VehicleController controller = References.truckController;

        // this is super hacky
        if (__instance.setDestinationToPlayerInterval <= 0f)
        {
            if (Vector3.Distance(__instance.targetPlayer.transform.position,
                controller.transform.position) < 10f)
            {
                __instance.setDestinationToPlayerInterval = 0.25f;

                bool isOccupant = controller.currentDriver == __instance.targetPlayer ||
                                  controller.currentPassenger == __instance.targetPlayer;

                bool inTruckBounds = controller.vehicleBounds.ClosestPoint(
                    __instance.targetPlayer.transform.position) ==
                    __instance.targetPlayer.transform.position;
                bool onTopOfTruck = controller.ontopOfTruckCollider.ClosestPoint(
                    __instance.targetPlayer.transform.position) ==
                    __instance.targetPlayer.transform.position;
                bool inTruckStorage = PlayerUtils.isPlayerInStorage;

                int areaMask = -1;
                if (isOccupant ||
                    inTruckBounds ||
                    inTruckStorage ||
                    onTopOfTruck)
                {
                    __instance.targetPlayerIsInTruck = true;
                    areaMask = -33;
                }
                __instance.destination = RoundManager.Instance.GetNavMeshPosition(__instance.targetPlayer.transform.position,
                    RoundManager.Instance.navHit, 5.5f,
                    areaMask);
                return false;
            }
            return true;
        }
        return true;
    }

    [HarmonyPatch(nameof(GiantKiwiAI.IsEggInsideClosedTruck))]
    [HarmonyPrefix]
    static bool IsEggInsideClosedTruck_Prefix(GiantKiwiAI __instance, KiwiBabyItem egg, bool closedTruck, ref bool __result)
    {
        if (References.truckController == null)
            return true;
        v55VehicleController controller = References.truckController;

        if (egg.parentObject == controller.physicsRegion.parentNetworkObject.transform)
        {
            __result = (!closedTruck || 
                !controller.liftGateOpen);
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(GiantKiwiAI.AnimationEventB))]
    [HarmonyPrefix]
    static void AnimationEventB_Prefix(GiantKiwiAI __instance)
    {
        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;

        if (playerControllerB == null ||
            !playerControllerB.isPlayerControlled ||
            playerControllerB.isPlayerDead)
            return;

        if (References.truckController == null)
            return;
        v55VehicleController controller = References.truckController;

        bool enemyInTruck = VehicleUtils.IsEnemyInVehicle(__instance, controller);
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            // enemy is not in the back with the player
            if (PlayerUtils.isPlayerInStorage && !enemyInTruck)
                __instance.timeSinceHittingPlayer = 0.4f;
        }
        else
        {
            // reset the timer, to prevent the kiwi from damaging the player
            if (enemyInTruck)
                __instance.timeSinceHittingPlayer = 0.4f;
        }
    }
}