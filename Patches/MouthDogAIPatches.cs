using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(MouthDogAI))]
public static class MouthDogAIPatches
{
    [HarmonyPatch(nameof(MouthDogAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(MouthDogAI __instance, Collider other)
    {
        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation, false);
        if (playerControllerB == null)
            return true;

        if (References.truckController == null)
            return true;
        v55VehicleController controller = References.truckController;

        // check if the player is seated in our truck
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller))
        {
            // force kill the player, otherwise the original inVehicleAnimation check takes prescedant, which we don't want
            // besides, i'm too lazy to transpile this shit, that's way beyond my paygrade
            if (__instance.currentBehaviourStateIndex == 3)
            {
                playerControllerB.inAnimationWithEnemy = __instance;
                __instance.KillPlayerServerRpc((int)playerControllerB.playerClientId);
            }
            return false;
        }

        // not seated in our truck, but within the vehicle bounds
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            if (VehicleUtils.IsPlayerProtectedByVehicle(playerControllerB, controller))
                return false; // player is protected (i.e. in storage or standing in cab), so do not allow the kill

            return true; // player is not protected, allow vanilla logic to run
        }

        // not in our truck, run vanilla logic
        return true;
    }
}