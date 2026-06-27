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
    private static void Start_Postfix(GameNetworkManager __instance)
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
    static void SaveItemsInShip_Postfix(GameNetworkManager __instance)
    {
        try
        {
            if (StartOfRound.Instance.attachedVehicle && StartOfRound.Instance.attachedVehicle is v55VehicleController controller)
            {
                SaveManager.Save("AttachedVehicleInterior", controller.interiorType);
                Plugin.Logger.LogMessage("V55: Successfully saved Cruiser data.");
            }
            else
            {
                SaveManager.Delete("AttachedVehicleInterior");
                Plugin.Logger.LogMessage("V55: Successfully deleted Cruiser data.");
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("V55: Exception caught saving Cruiser data:\n" + e);
        }
    }
}