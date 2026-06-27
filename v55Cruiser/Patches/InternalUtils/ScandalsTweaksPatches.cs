using GameNetcodeStuff;
using HarmonyLib;
using ScandalsTweaks.Patches;
using ScandalsTweaks.Utils;
using ScandalsTweaks.Compatibility;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.InternalUtils;

[HarmonyPatch]
public static class ScandalsTweaksPatches
{
    public static bool IsPlayerInCruiser(PlayerControllerB player, bool checkBlowback = false)
    {
        if (checkBlowback && 
            (PlayerControllerBPatches.playerData[player].playerSeatedInTruck || 
             PlayerControllerBPatches.playerData[player].playerRidingInTruckStorage))
            return true;

        if (PlayerControllerBPatches.playerData[player].playerSeatedInTruck ||
            PlayerControllerBPatches.playerData[player].playerRidingOnTruck)
            return true;

        return false;
    }

    [HarmonyPatch(typeof(JLLCompatibility), nameof(JLLCompatibility.CanPlayerBeBlown))]
    [HarmonyPrefix]
    private static bool CanPlayerBeBlown_Prefix(PlayerControllerB player, ref bool __result)
    {
        if (IsPlayerInCruiser(player, true))
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(GlobalUtilities), nameof(GlobalUtilities.ShouldAllowSightForVehicle))]
    [HarmonyPrefix]
    private static bool ShouldAllowSightForVehicle_Prefix(PlayerControllerB player, EnemyAI enemy, ref bool __result)
    {
        if (IsPlayerInCruiser(player))
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(GiantKiwiAI_Patches), nameof(GiantKiwiAI_Patches.IsTargetPlayerInVehicle))]
    [HarmonyPrefix]
    private static bool IsTargetPlayerInVehicle_Prefix(GiantKiwiAI giantKiwiAi, VehicleController vehicleController, ref bool __result)
    {
        if (vehicleController is not v55VehicleController controller)
            return true;

        var targetData = PlayerControllerBPatches.playerData[giantKiwiAi.targetPlayer];
        bool targetInTruck = targetData.playerSeatedInTruck ||
                            (targetData.playerRidingInTruckStorage && !controller.liftGateOpen) ||
                             controller.ontopOfTruckCollider.ClosestPoint(giantKiwiAi.targetPlayer.transform.position) ==
                             giantKiwiAi.targetPlayer.transform.position;

        if (targetInTruck)
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Landmine_Patches), nameof(Landmine_Patches.ShouldCheckCustomKnockback))]
    [HarmonyPrefix]
    private static bool ShouldCheckCustomKnockback_Prefix(ref bool __result)
    {
        return true;
    }

    [HarmonyPatch(typeof(Landmine_Patches), nameof(Landmine_Patches.CanPlayerBeKnockedBack))]
    [HarmonyPrefix]
    private static bool CanPlayerBeKnockedBack_Prefix(ref bool __result)
    {
        return true;
    }

    [HarmonyPatch(typeof(Landmine_Patches), nameof(Landmine_Patches.CurrentForceMultiplier))]
    [HarmonyPrefix]
    private static bool CurrentForceMultiplier_Prefix(ref float __result)
    {
        if (References.truckController != null)
        {
            __result = 1f;
            return false;
        }
        return true;
    }
}
