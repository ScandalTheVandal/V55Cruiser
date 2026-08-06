using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(Landmine))]
public static class LandminePatches
{
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPrefix]
    private static void Landmine_Pre_SpawnExplosion(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerNearTruck(playerController, truckController))
            return;

        bool isProbablyLightning =
            !goThroughCar &&
            !spawnExplosionEffect &&
            killRange == 2.4f &&
            damageRange == 5f &&
            nonLethalDamage == 1 &&
            physicsForce == 0f;

        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(playerController, truckController);
        bool lightningOutsideStorage =
            truckController.storageCompartment.ClosestPoint(explosionPosition) != explosionPosition;

        if (isProbablyLightning &&
            playerInStorage &&
            lightningOutsideStorage)
        {
            killRange = -1f;
            damageRange = -1f;
        }
    }
}
