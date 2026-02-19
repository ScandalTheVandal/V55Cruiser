using v55Cruiser.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(MaskedPlayerEnemy))]
internal class MaskedPlayerEnemyPatches
{
    [HarmonyPatch(nameof(MaskedPlayerEnemy.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(MaskedPlayerEnemy __instance, Collider other)
    {
        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation || __instance.startingKillAnimationLocalClient || !__instance.enemyEnabled, false);
        if (playerControllerB == null)
            return true;

        if (References.truckController == null)
            return true;
        v55VehicleController controller = References.truckController;
        var avgSpeed = controller.averageVelocity.magnitude;

        // check if the player is seated in our truck
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller))
            return false;

        bool enemyInTruck = VehicleUtils.IsEnemyInVehicle(__instance, controller);
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            // enemy is not in the back with the player
            if (PlayerUtils.isPlayerInStorage && !enemyInTruck)
                return false;

            // player is just riding on the truck
            if (!PlayerUtils.isPlayerInStorage)
                return avgSpeed < 2f;

            return true;
        }
        else
        {
            if (enemyInTruck)
                return false;
        }
        return true; // run vanilla logic
    }
}