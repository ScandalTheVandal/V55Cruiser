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
    private static void Awake_Prefix(StartOfRound __instance)
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
    static void SyncAlreadyHeldObjectsServerRpc_Postfix(StartOfRound __instance, int joiningClientId)
    {
        if (!__instance.attachedVehicle ||  __instance.attachedVehicle is not v55VehicleController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.Logger.LogError("V55: Attempted to send client data, but the truck is null? please report this to Scandal.");
                return;
            }
            controller.SendClientSyncData();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("V55: Exception caught sending saved Cruiser data:\n" + e);
        }
    }

    [HarmonyPatch(nameof(StartOfRound.LoadAttachedVehicle))]
    [HarmonyPostfix]
    static void LoadAttachedVehicle_Postfix(StartOfRound __instance)
    {
        if (!__instance.attachedVehicle ||  __instance.attachedVehicle is not v55VehicleController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.Logger.LogError("V55: Attempted to load saved data, but the van is null? please report this to Scandal.");
                return;
            }
            if (SaveManager.TryLoad<int>("AttachedVehicleInterior", out var interior))
            {
                controller.interiorType = interior;
                controller.SetInteriorType(controller.interiorType);
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("V55: Exception caught loading saved Cruiser data:\n" + e);
        }
    }
}