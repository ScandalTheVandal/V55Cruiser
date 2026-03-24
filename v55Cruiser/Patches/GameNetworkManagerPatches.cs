using System;
using v55Cruiser.Utils;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
public static class GameNetworkManagerPatches
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    private static void GameNetworkManager_Start(GameNetworkManager __instance)
    {
        foreach (GameObject obj in Plugin.networkPrefabs)
        {
            if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(obj))
                NetworkManager.Singleton.AddNetworkPrefab(obj);
        }
    }

    [HarmonyPatch(nameof(GameNetworkManager.SaveItemsInShip))]
    [HarmonyPostfix]
    static void SaveItemsInShip_Postfix(GameNetworkManager __instance)
    {
        //save Cruiser data if we have one
        try
        {
            if (StartOfRound.Instance.attachedVehicle && StartOfRound.Instance.attachedVehicle.TryGetComponent<v55VehicleController>(out var controller))
            {
                SaveManager.Save("AttachedVehicleInterior", controller.interiorType);
                Plugin.Logger.LogMessage("Successfully saved Cruiser data.");
            }
            else
            {
                SaveManager.Delete("AttachedVehicleInterior");
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception caught saving Cruiser data:\n" + e);
        }
    }
}