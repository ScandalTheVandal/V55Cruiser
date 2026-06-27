using HarmonyLib;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(Landmine))]
public static class LandminePatches
{
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPrefix]
    private static void SpawnExplosion_Prefix(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return;

        if (!VehicleUtils.IsPlayerNearTruck(GameNetworkManager.Instance.localPlayerController, truckController: controller))
            return;

        bool isProbablyLightning = !goThroughCar && !spawnExplosionEffect && 
            killRange == 2.4f && damageRange == 5f && nonLethalDamage == 1f && physicsForce == 0f;

        if (isProbablyLightning && (VehicleUtils.IsPlayerInTruckStorage(controller) && controller.storageCompartment.ClosestPoint(explosionPosition) != explosionPosition))
        {
            killRange = -1f;
            damageRange = -1f;
        }
    }
}
