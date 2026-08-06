using HarmonyLib;
using v55Cruiser.Networking;
using System;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public static class StartOfRoundPatches
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPrefix]
    private static void StartOfRound_Pre_Awake(StartOfRound __instance)
    {
        V55Networker.Create();
        __instance.VehiclesList[0] = References.companyCruiserPrefab;
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/StartOfRound.cs
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Network/Patches/StartOfRound.cs
    /// </summary>
    [HarmonyPatch(nameof(StartOfRound.SyncAlreadyHeldObjectsServerRpc))]
    [HarmonyPostfix]
    static void StartOfRound_Post_SyncAlreadyHeldObjectsServerRpc(StartOfRound __instance, int joiningClientId)
    {
        if (!__instance.attachedVehicle || __instance.attachedVehicle is not v55VehicleController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.LogError("Attempted to send client data, but the Truck is null? please report this to Scandal.");
                return;
            }
            controller.SendClientSyncData();
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught sending saved Truck data:\n" + e);
        }
    }

    [HarmonyPatch(nameof(StartOfRound.LoadAttachedVehicle))]
    [HarmonyPostfix]
    static void StartOfRound_Post_LoadAttachedVehicle(StartOfRound __instance)
    {
        if (!__instance.attachedVehicle || __instance.attachedVehicle is not v55VehicleController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.LogError("Attempted to load saved data, but the Truck is null? please report this to Scandal.");
                return;
            }
            if (SaveManager.TryLoad<int>(SaveManager.SavedTruckInterior, out var interior))
            {
                if (UserConfig.PostBeta.Value) interior = 0;
                controller.interiorType = interior;
                controller.SetInteriorType(controller.interiorType);
            }
            controller.inBetaMode = UserConfig.PostBeta.Value;
            controller.canDestroyTrees = !UserConfig.NoTreeDestruction.Value;
            controller.hasAdditionalMusic = UserConfig.AdditionalRadioMusic.Value;
            controller.useSteeringCurve = UserConfig.AlternateSteering.Value && !UserConfig.PostBeta.Value;
            if (!controller.inBetaMode)
            {
                controller.canDestroyTrees = true;
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught loading saved Truck data:\n" + e);
        }
    }
}