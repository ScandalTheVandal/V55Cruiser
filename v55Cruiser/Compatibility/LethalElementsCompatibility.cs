using v55Cruiser.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using VoxxWeatherPlugin.Patches;
using System.Runtime.CompilerServices;
using VoxxWeatherPlugin.Weathers;
using VoxxWeatherPlugin.Utils;
using System.Collections;
using v55Cruiser;
using VoxxWeatherPlugin;

namespace CruiserXL.Compatibility;

public class LethalElementsCompatibility
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAllElements(Harmony harmony)
    {
        ApplyElementsPatch(harmony);
    }

    [HarmonyPrefix]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ApplyElementsPatch(Harmony harmony)
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
        if (References.truckController == null)
            return;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.seatedInTruck &&
            !PlayerUtils.isPlayerInStorage)
            return;

        v55VehicleController controller = References.truckController;
        bool isStorageEnclosed = !controller.liftGateOpen;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.seatedInTruck && !PlayerUtils.isPlayerInStorage;
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        if (PlayerUtils.seatedInTruck)
        {
            PlayerEffectsManager.heatTransferRate = controller.windshieldBroken ? 0.9f : 0.2f;
        }
        else if (PlayerUtils.isPlayerInStorage && isStorageEnclosed)
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

        bool enableTracker = __instance.FrontLeftWheel.isGrounded ||
                                __instance.FrontRightWheel.isGrounded ||
                                __instance.BackLeftWheel.isGrounded ||
                                __instance.BackRightWheel.isGrounded;
        SnowTrackersManager.UpdateFootprintTracker(__instance, enableTracker);
    }

    public static void VFXUpdate_Postfix()
    {
        if (References.truckController == null)
            return;

        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.seatedInTruck &&
            !PlayerUtils.isPlayerInStorage)
            return;

        if (PlayerUtils.seatedInTruck || PlayerUtils.isPlayerInStorage)
        {
            PlayerEffectsManager.isUnderSnow = false;
            SnowfallVFXManager.snowMovementHindranceMultiplier = 1f;
        }
    }

    public static bool SetColdZoneState_Prefix(BlizzardWeather __instance)
    {
        if (References.truckController == null)
            return true;
        if (PlayerUtils.isPlayerOnTruck ||
            PlayerUtils.seatedInTruck ||
            PlayerUtils.isPlayerInStorage)
            return false;
        return true;
    }

    public static bool SetBaseColdZoneState_Prefix(SnowfallWeather __instance)
    {
        if (BlizzardWeather.Instance == null)
            return true;
        if (References.truckController == null)
            return true;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.seatedInTruck &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        v55VehicleController controller = References.truckController;
        bool isStorageEnclosed = !controller.liftGateOpen;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.seatedInTruck && !PlayerUtils.isPlayerInStorage;
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        if (PlayerUtils.seatedInTruck)
        {
            PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(localPlayer, controller) && BlizzardWeather.Instance.isLocalPlayerInWind;
            return false;
        }
        else if (PlayerUtils.isPlayerInStorage)
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
        if (References.truckController == null)
            return true;

        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.seatedInTruck &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        v55VehicleController controller = References.truckController;
        bool isStorageEnclosed = !controller.liftGateOpen;
        bool inCabOrStorage = PlayerUtils.seatedInTruck || PlayerUtils.isPlayerInStorage;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.seatedInTruck && !PlayerUtils.isPlayerInStorage;
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
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck || PlayerUtils.seatedInTruck;

        if (PlayerUtils.isPlayerInStorage && !isStorageEnclosed)
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
        if (References.truckController == null)
            return true;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.seatedInTruck &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        v55VehicleController controller = References.truckController;
        bool isStorageEnclosed = !controller.liftGateOpen;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.seatedInTruck && !PlayerUtils.isPlayerInStorage;
        bool inDoorLighting = playerController.currentAudioTrigger != null && playerController.currentAudioTrigger.insideLighting;

        if (PlayerUtils.isPlayerInStorage && isStorageEnclosed)
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
        if (References.truckController == null)
            return true;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.seatedInTruck &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        __result = false;
        return false;
    }
}
