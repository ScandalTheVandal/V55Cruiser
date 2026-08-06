using GameNetcodeStuff;
using HarmonyLib;
using ScandalsTweaks.Utils;
using ScandalsTweaks.Compatibility;
using ScandalsTweaks.Patches.Enemies;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.InternalUtils;

[HarmonyPatch]
public static class ScandalsTweaksPatches
{
    public static bool IsPlayerInTruck(PlayerControllerB player, bool checkBlowback = false, bool checkTrunkOnly = false, bool usePlayerData = false)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return false;

        var playerData = PlayerControllerBPatches.playerData[player];
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(player, truckController, usePlayerData);

        if (checkBlowback || checkTrunkOnly)
            return playerInStorage;

        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(player, truckController, usePlayerData);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(player, truckController);

        return playerSeated || playerOnTruck || playerInStorage;
    }

    [HarmonyPatch(typeof(JLLCompatibility), nameof(JLLCompatibility.CanBlowPlayer))]
    [HarmonyPrefix]
    private static bool JLLCompatibility_Pre_CanBlowPlayer(PlayerControllerB player, ref bool __result)
    {
        if (IsPlayerInTruck(player, true, false, false))
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(JLLCompatibility), nameof(JLLCompatibility.CanDamagePlayer))]
    [HarmonyPrefix]
    private static bool JLLCompatibility_Pre_CanDamagePlayer(PlayerControllerB player, ref bool __result)
    {
        if (IsPlayerInTruck(player, false, true, false))
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Utilities), nameof(Utilities.ShouldAllowSightThroughVehicle))]
    [HarmonyPrefix]
    private static bool Utilities_Pre_ShouldAllowSightThroughVehicle(PlayerControllerB player, EnemyAI enemy, ref bool __result)
    {
        if (IsPlayerInTruck(player, false, false, true))
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(GiantKiwiAIPatches), nameof(GiantKiwiAIPatches.IsTargetPlayerInVehicle))]
    [HarmonyPrefix]
    private static bool GiantKiwiAIPatches_Pre_IsTargetPlayerInVehicle(GiantKiwiAI giantKiwiAi, VehicleController vehicleController, ref bool __result)
    {
        if (vehicleController is not v55VehicleController truckController)
            return true;

        PlayerControllerB targetPlayer = giantKiwiAi.targetPlayer;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(targetPlayer, truckController);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(targetPlayer, truckController, usePlayerData: true);
        bool storageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);

        bool playerOnTruck =
            truckController.ontopOfTruckCollider.ClosestPoint(targetPlayer.transform.position) ==
            targetPlayer.transform.position;

        if (playerSeated ||
            (playerInStorage && storageEnclosed) ||
            playerOnTruck)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(BushWolfEnemyPatches), nameof(BushWolfEnemyPatches.IsTargetPlayerProtected))]
    [HarmonyPrefix]
    private static bool BushWolfEnemyPatches_Pre_IsTargetPlayerProtected(PlayerControllerB player, VehicleController vehicle, ref bool __result)
    {
        if (vehicle is not v55VehicleController truckController)
            return true;

        if (!VehicleUtils.IsPlayerProtectedInTruckStorage(player, truckController, usePlayerData: true))
            return true;

        __result = true;
        return false;
    }
}
