using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(RadMechAI))]
public static class RadMechAIPatches
{
    [HarmonyPatch(nameof(RadMechAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool RadMechAI_Pre_OnCollideWithPlayer(RadMechAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = 
            __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerController == null)
            return true;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController);
        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(playerController, truckController);

        if (playerSeated)
            return true;

        if (!playerOnTruck)
            return true;

        return !VehicleUtils.IsPlayerProtectedByTruck(playerController, truckController);
    }
}