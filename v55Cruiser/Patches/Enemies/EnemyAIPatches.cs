using GameNetcodeStuff;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(EnemyAI))]
public static class EnemyAIPatches
{
    [HarmonyPatch(nameof(EnemyAI.PlayerIsTargetable))]
    [HarmonyPostfix]
    static void EnemyAI_Post_PlayerIsTargetable(EnemyAI __instance, PlayerControllerB playerScript, bool cannotBeInShip, bool overrideInsideFactoryCheck, bool checkForMineshaftStartTile, ref bool __result)
    {
        if (__instance is not BushWolfEnemy)
            return;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = playerScript;
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(playerController, truckController, usePlayerData: true);
        bool storageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);

        if (playerInStorage && storageEnclosed)
            __result = false;
    }
}