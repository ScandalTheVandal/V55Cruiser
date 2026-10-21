using System;
using v55Cruiser.Utils;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode;

namespace v55Cruiser.Patches;

[HarmonyPatch]
public static class GameNetworkManagerPatches
{
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    private static void GameNetworkManager_Post_Start(GameNetworkManager __instance)
    {
        foreach (GameObject obj in Plugin.networkPrefabs)
        {
            if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(obj))
                NetworkManager.Singleton.AddNetworkPrefab(obj);
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveItemsInShip))]
    [HarmonyPostfix]
    static void GameNetworkManager_Post_SaveItemsInShip(GameNetworkManager __instance)
    {
        try
        {
            if (StartOfRound.Instance.attachedVehicle && StartOfRound.Instance.attachedVehicle is v55VehicleController controller)
            {
                SaveManager.Save(SaveManager.SavedTruckInterior, controller.interiorType);
                Plugin.LogMessage("Successfully saved Truck data.");
            }
            else
            {
                SaveManager.Delete(SaveManager.SavedTruckInterior);
                Plugin.LogMessage("Successfully deleted Truck data.");
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught saving Truck data:\n" + e);
        }
    }
}