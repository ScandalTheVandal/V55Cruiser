using v55Cruiser.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using VoxxWeatherPlugin.Patches;
using VoxxWeatherPlugin.Weathers;
using VoxxWeatherPlugin.Utils;

namespace v55Cruiser.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
[HarmonyPatch]
public static class LethalElementsCompatibility
{
    public static bool IsIndoorLighting(PlayerControllerB playerScript)
    {
        return playerScript.currentAudioTrigger != null && playerScript.currentAudioTrigger.insideLighting;
    }

    [HarmonyPatch(typeof(PlayerEffectsManager), nameof(PlayerEffectsManager.SetPlayerTemperature))]
    [HarmonyPrefix]
    public static void PlayerEffectsManager_Pre_SetPlayerTemperature(PlayerEffectsManager __instance, float temperatureDelta)
    {
        if (HeatwaveWeather.Instance == null || !HeatwaveWeather.Instance.IsActive)
            return;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, truckController) &&
            !VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) &&
            !VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return;

        bool isStorageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfTruck(playerController, truckController);
        bool inDoorLighting = IsIndoorLighting(playerController);

        if (VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController))
        {
            PlayerEffectsManager.heatTransferRate = inDoorLighting ? 1 : truckController.windshieldBroken ? 0.9f : 0.2f;
        }
        else if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController) && isStorageEnclosed)
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

    [HarmonyPatch(typeof(v55VehicleController), nameof(v55VehicleController.Start))]
    [HarmonyPostfix]
    public static void v55VehicleController_Post_Start(v55VehicleController __instance)
    {
        SnowTrackersManager.AddFootprintTracker(__instance, 6f, 0.75f, 1f, new Vector3(0, 0, -1f));
    }

    [HarmonyPatch(typeof(v55VehicleController), nameof(v55VehicleController.FixedUpdate))]
    [HarmonyPostfix]
    public static void v55VehicleController_Post_FixedUpdate(v55VehicleController __instance)
    {
        if (!SnowPatches.IsSnowActive())
        {
            return;
        }
        SnowTrackersManager.UpdateFootprintTracker(__instance, !__instance.allWheelsAirborne);
    }

    [HarmonyPatch(typeof(SnowfallVFXManager), nameof(SnowfallVFXManager.Update))]
    [HarmonyPostfix]
    public static void SnowfallVFXManager_Post_Update_VFX()
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, truckController) &&
            !VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) &&
            !VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return;

        if (VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) ||
            VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
        {
            PlayerEffectsManager.isUnderSnow = false;
            SnowfallVFXManager.snowMovementHindranceMultiplier = 1f;
        }
    }

    [HarmonyPatch(typeof(BlizzardWeather), nameof(BlizzardWeather.SetColdZoneState))]
    [HarmonyPrefix]
    public static bool BlizzardWeather_Pre_SetColdZoneState(BlizzardWeather __instance)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (VehicleUtils.IsPlayerInTruckBounds(playerController, truckController) ||
            VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) ||
            VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return false;

        return true;
    }

    [HarmonyPatch(typeof(SnowfallWeather), nameof(SnowfallWeather.SetColdZoneState))]
    [HarmonyPrefix]
    public static bool SnowfallWeather_Pre_SetColdZoneState(SnowfallWeather __instance)
    {
        if (BlizzardWeather.Instance == null)
            return true;

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, truckController) &&
            !VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) &&
            !VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return true;

        bool isStorageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfTruck(playerController, truckController);
        bool inDoorLighting = IsIndoorLighting(playerController);

        if (VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController))
        {
            PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(playerController, truckController) && BlizzardWeather.Instance.isLocalPlayerInWind;
            return false;
        }
        else if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
        {
            if (isStorageEnclosed)
            {
                PlayerEffectsManager.isInColdZone = !inDoorLighting;
            }
            else
            {
                PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(playerController, truckController) && BlizzardWeather.Instance.isLocalPlayerInWind;
            }
            return false;
        }
        else if (outsideOfTruck)
        {
            PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(playerController, truckController) && BlizzardWeather.Instance.isLocalPlayerInWind;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(BlizzardWeather), nameof(BlizzardWeather.Update))]
    [HarmonyPrefix]
    public static bool BlizzardWeather_Pre_Update(BlizzardWeather __instance)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, truckController) &&
            !VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) &&
            !VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return true;

        bool isStorageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfTruck(playerController, truckController);
        SnowfallWeather.Instance?.Update();

        if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
        {
            PlayerEffectsManager.heatTransferRate = 0.75f;
            __instance.isPlayerInBlizzard = __instance.isLocalPlayerInWind && !VehicleUtils.IsTruckStorageEnclosed(truckController);
        }
        else if (VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) || outsideOfTruck)
        {
            PlayerEffectsManager.heatTransferRate = 1f;
            __instance.isPlayerInBlizzard = __instance.isLocalPlayerInWind;
        }
        return false;
    }

    public static bool IsWindAllowedVehicle(PlayerControllerB playerController, v55VehicleController truckController)
    {
        if (IsIndoorLighting(playerController)) return false;

        bool isStorageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfTruck(playerController, truckController);

        if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController) && !isStorageEnclosed)
        {
            return true;
        }
        else if (VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) || outsideOfTruck)
        {
            return true;
        }
        return false;
    }

    [HarmonyPatch(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingStop))]
    [HarmonyPrefix]
    public static bool HeatwavePatches_Pre_CheckConditionsForHeatingStop(PlayerControllerB playerController, ref bool __result)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, truckController) &&
            !VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) &&
            !VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return true;

        bool isStorageEnclosed = VehicleUtils.IsTruckStorageEnclosed(truckController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfTruck(playerController, truckController);
        bool inDoorLighting = IsIndoorLighting(playerController);

        if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController) && isStorageEnclosed)
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

    [HarmonyPatch(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingPause))]
    [HarmonyPrefix]
    public static bool HeatwavePatches_Pre_CheckConditionsForHeatingPause(PlayerControllerB playerController, ref bool __result)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, truckController) &&
            !VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController) &&
            !VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
            return true;

        __result = false;
        return false;
    }
}