using v55Cruiser.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using VoxxWeatherPlugin.Patches;
using System.Runtime.CompilerServices;
using VoxxWeatherPlugin.Weathers;
using VoxxWeatherPlugin.Utils;

namespace v55Cruiser.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>

public static class LethalElementsCompatibility
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAllMethods(Harmony harmony)
    {
        ApplyPatch(harmony);
    }

    [HarmonyPrefix]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ApplyPatch(Harmony harmony)
    {
        var originalHeatwaveStopMethod = AccessTools.Method(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingStop));
        var originalHeatwavePauseMethod = AccessTools.Method(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingPause));
        var originalSetTempMethod = AccessTools.Method(typeof(PlayerEffectsManager), nameof(PlayerEffectsManager.SetPlayerTemperature));
        var originalUpdateMethod = AccessTools.Method(typeof(BlizzardWeather), nameof(BlizzardWeather.Update));
        var originalVFXUpdateMethod = AccessTools.Method(typeof(SnowfallVFXManager), nameof(SnowfallVFXManager.Update));
        var originalSetZoneMethod = AccessTools.Method(typeof(BlizzardWeather), nameof(BlizzardWeather.SetColdZoneState));
        var originalBaseSetZoneMethod = AccessTools.Method(typeof(SnowfallWeather), nameof(SnowfallWeather.SetColdZoneState));

        var originalVehicleStartMethod = AccessTools.Method(typeof(v55VehicleController), nameof(v55VehicleController.Start));
        var originalVehicleFixedUpdateMethod = AccessTools.Method(typeof(v55VehicleController), nameof(v55VehicleController.FixedUpdate));

        var prefixHeatwaveStopMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(CheckConditionsForHeatingStop_Prefix));
        var prefixHeatwavePauseMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(CheckConditionsForHeatingPause_Prefix));
        var prefixSetTempMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(SetPlayerTemperature_Heatwave));
        var prefixUpdateMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(Update_Prefix));
        var postfixVFXUpdateMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(VFXUpdate_Postfix));
        var prefixSetZoneMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(SetColdZoneState_Prefix));
        var prefixBaseSetZoneMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(SetBaseColdZoneState_Prefix));

        var prefixVehicleStartMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(VehicleSnowTracksPatch_Prefix));
        var prefixVehicleFixedUpdateMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(VehicleSnowTracksFixedUpdatePatch_Prefix));

        harmony.Patch(originalHeatwaveStopMethod, prefix: new HarmonyMethod(prefixHeatwaveStopMethod));
        harmony.Patch(originalHeatwavePauseMethod, prefix: new HarmonyMethod(prefixHeatwavePauseMethod));
        harmony.Patch(originalSetTempMethod, prefix: new HarmonyMethod(prefixSetTempMethod));
        harmony.Patch(originalUpdateMethod, prefix: new HarmonyMethod(prefixUpdateMethod));
        harmony.Patch(originalVFXUpdateMethod, postfix: new HarmonyMethod(postfixVFXUpdateMethod));
        harmony.Patch(originalSetZoneMethod, prefix: new HarmonyMethod(prefixSetZoneMethod));
        harmony.Patch(originalBaseSetZoneMethod, prefix: new HarmonyMethod(prefixBaseSetZoneMethod));

        harmony.Patch(originalVehicleStartMethod, prefix: new HarmonyMethod(prefixVehicleStartMethod));
        harmony.Patch(originalVehicleFixedUpdateMethod, prefix: new HarmonyMethod(prefixVehicleFixedUpdateMethod));
    }

    // hacky method to alter the heat transfer rate during heatwave
    public static void SetPlayerTemperature_Heatwave(PlayerEffectsManager __instance, float temperatureDelta)
    {
        if (HeatwaveWeather.Instance == null || !HeatwaveWeather.Instance.IsActive)
            return;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return;

        if (!VehicleUtils.IsPlayerInTruckBounds(controller) &&
            !VehicleUtils.IsPlayerSeatedInTruck() &&
            !VehicleUtils.IsPlayerInTruckStorage(controller))
            return;

        bool isStorageEnclosed = !controller.liftGateOpen;
        bool outsideOfTruck = VehicleUtils.IsPlayerInTruckBounds(controller) && !VehicleUtils.IsPlayerSeatedInTruck() && !VehicleUtils.IsPlayerInTruckStorage(controller);
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            PlayerEffectsManager.heatTransferRate = controller.windshieldBroken ? 0.9f : 0.2f;
        }
        else if (VehicleUtils.IsPlayerInTruckStorage(controller) && isStorageEnclosed)
        {
            if (!inDoorLighting)
            {
                PlayerEffectsManager.heatTransferRate = 0.75f;
            }
            else PlayerEffectsManager.heatTransferRate = 1f;
        }
        else PlayerEffectsManager.heatTransferRate = 1f;
        if (outsideOfTruck) PlayerEffectsManager.heatTransferRate = 1f;
    }

    public static void VehicleSnowTracksPatch_Prefix(v55VehicleController __instance)
    {
        SnowTrackersManager.AddFootprintTracker(__instance, 6f, 0.75f, 1f, new Vector3(0, 0, -1f));
    }

    public static void VehicleSnowTracksFixedUpdatePatch_Prefix(v55VehicleController __instance)
    {
        if (!SnowPatches.IsSnowActive())
        {
            return;
        }
        SnowTrackersManager.UpdateFootprintTracker(__instance, !__instance.allWheelsAirborne);
    }

    public static void VFXUpdate_Postfix()
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return;

        if (!VehicleUtils.IsPlayerInTruckBounds(controller) &&
            !VehicleUtils.IsPlayerSeatedInTruck() &&
            !VehicleUtils.IsPlayerInTruckStorage(controller))
            return;

        if (VehicleUtils.IsPlayerSeatedInTruck() || VehicleUtils.IsPlayerInTruckStorage(controller))
        {
            PlayerEffectsManager.isUnderSnow = false;
            SnowfallVFXManager.snowMovementHindranceMultiplier = 1f;
        }
    }

    public static bool SetColdZoneState_Prefix(BlizzardWeather __instance)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        if (VehicleUtils.IsPlayerInTruckBounds(controller) ||
            VehicleUtils.IsPlayerSeatedInTruck() ||
            VehicleUtils.IsPlayerInTruckStorage(controller))
            return false;
        return true;
    }

    public static bool SetBaseColdZoneState_Prefix(SnowfallWeather __instance)
    {
        if (BlizzardWeather.Instance == null)
            return true;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        if (!VehicleUtils.IsPlayerInTruckBounds(controller) &&
            !VehicleUtils.IsPlayerSeatedInTruck() &&
            !VehicleUtils.IsPlayerInTruckStorage(controller))
            return true;

        bool isStorageEnclosed = !controller.liftGateOpen;
        bool outsideOfTruck = VehicleUtils.IsPlayerInTruckBounds(controller) && !VehicleUtils.IsPlayerSeatedInTruck() && !VehicleUtils.IsPlayerInTruckStorage(controller);
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(localPlayer, controller) && BlizzardWeather.Instance.isLocalPlayerInWind;
            return false;
        }
        else if (VehicleUtils.IsPlayerInTruckStorage(controller))
        {
            if (isStorageEnclosed)
            {
                PlayerEffectsManager.isInColdZone = !inDoorLighting;
            }
            else
            {
                PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(localPlayer, controller) && BlizzardWeather.Instance.isLocalPlayerInWind;
            }
            return false;
        }
        else if (outsideOfTruck)
        {
            PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(localPlayer, controller) && BlizzardWeather.Instance.isLocalPlayerInWind;
            return false;
        }
        return true;
    }

    public static bool Update_Prefix(BlizzardWeather __instance)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        if (!VehicleUtils.IsPlayerInTruckBounds(controller) &&
            !VehicleUtils.IsPlayerSeatedInTruck() &&
            !VehicleUtils.IsPlayerInTruckStorage(controller))
            return true;

        bool isStorageEnclosed = !controller.liftGateOpen;
        bool inCabOrStorage = VehicleUtils.IsPlayerSeatedInTruck() || VehicleUtils.IsPlayerInTruckStorage(controller);
        bool outsideOfTruck = VehicleUtils.IsPlayerInTruckBounds(controller) && !VehicleUtils.IsPlayerSeatedInTruck() && !VehicleUtils.IsPlayerInTruckStorage(controller);
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        SnowfallWeather.Instance?.Update();

        if (inCabOrStorage)
        {
            PlayerEffectsManager.heatTransferRate = 0.75f;
        }
        if (outsideOfTruck)
        {
            PlayerEffectsManager.heatTransferRate = 1f;
            __instance.isPlayerInBlizzard = __instance.isLocalPlayerInWind;
        }
        return false;
    }

    public static bool IsWindAllowedVehicle(PlayerControllerB localPlayer, v55VehicleController controller)
    {
        if (localPlayer.currentAudioTrigger != null &&
                localPlayer.currentAudioTrigger.insideLighting) return false;

        bool isStorageEnclosed = !controller.liftGateOpen;
        bool outsideOfTruck = VehicleUtils.IsPlayerInTruckBounds(controller) || VehicleUtils.IsPlayerSeatedInTruck();

        if (VehicleUtils.IsPlayerInTruckStorage(controller) && !isStorageEnclosed)
        {
            return true;
        }
        else if (outsideOfTruck)
        {
            return true;
        }
        return false;
    }

    public static bool CheckConditionsForHeatingStop_Prefix(PlayerControllerB playerController, ref bool __result)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        if (!VehicleUtils.IsPlayerInTruckBounds(controller) &&
            !VehicleUtils.IsPlayerSeatedInTruck() &&
            !VehicleUtils.IsPlayerInTruckStorage(controller))
            return true;

        bool isStorageEnclosed = !controller.liftGateOpen;
        bool outsideOfTruck = VehicleUtils.IsPlayerInTruckBounds(controller) && !VehicleUtils.IsPlayerSeatedInTruck() && !VehicleUtils.IsPlayerInTruckStorage(controller);
        bool inDoorLighting = playerController.currentAudioTrigger != null && playerController.currentAudioTrigger.insideLighting;

        if (VehicleUtils.IsPlayerInTruckStorage(controller) && isStorageEnclosed)
        {
            __result = true;
        }
        else
        {
            __result = inDoorLighting ? true : false;
        }
        if (outsideOfTruck) __result = inDoorLighting ? true : false;
        return false;
    }

    public static bool CheckConditionsForHeatingPause_Prefix(PlayerControllerB playerController, ref bool __result)
    {
        v55VehicleController controller = References.truckController;
        if (controller == null)
            return true;

        if (!VehicleUtils.IsPlayerInTruckBounds(controller) &&
            !VehicleUtils.IsPlayerSeatedInTruck() &&
            !VehicleUtils.IsPlayerInTruckStorage(controller))
            return true;

        __result = false;
        return false;
    }
}
