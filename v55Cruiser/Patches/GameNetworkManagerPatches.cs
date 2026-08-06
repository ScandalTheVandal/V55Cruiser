using System;
using v55Cruiser.Utils;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode;
using v55Cruiser.Networking;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
public static class GameNetworkManagerPatches
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    private static void GameNetworkManager_Post_Start(GameNetworkManager __instance)
    {
        V55Networker.Init();
        foreach (GameObject obj in Plugin.networkPrefabs)
        {
            if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(obj))
                NetworkManager.Singleton.AddNetworkPrefab(obj);
        }
    }

    [HarmonyPatch(nameof(GameNetworkManager.SaveItemsInShip))]
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