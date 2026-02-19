using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using v55Cruiser.Behaviour;
using v55Cruiser.Utils;


namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBPatches
{
    public static float checkInterval;

    public class PlayerControllerBData
    {
        public float syncLookInputInterval;
        public float vehicleCameraHorizontal;
        public float lastVehicleCameraHorizontal;
        public int currentCarAnimation = -1;

        public bool isPlayerOnTruck;
        public bool isPlayerInStorage;
    }

    public static Dictionary<PlayerControllerB, PlayerControllerBData> playerData = new();


    private static void RemoveStalePlayerData()
    {
        List<PlayerControllerB> playersToRemove = new();
        foreach (PlayerControllerB player in playerData.Keys)
        {
            if (!player)
            {
                playersToRemove.Add(player);
            }
        }

        foreach (PlayerControllerB player in playersToRemove)
        {
            playerData.Remove(player);
        }
    }

    public static PlayerControllerBData GetData(PlayerControllerB player)
    {
        if (!playerData.TryGetValue(player, out var data))
        {
            data = new PlayerControllerBData();
            playerData[player] = data;
        }
        return data;
    }

    [HarmonyPatch(nameof(PlayerControllerB.Awake))]
    [HarmonyPostfix]
    static void Awake_Postfix(PlayerControllerB __instance)
    {
        RemoveStalePlayerData();
        if (!playerData.ContainsKey(__instance))
        {
            playerData.Add(__instance, new PlayerControllerBData());
        }
    }

    [HarmonyPatch(nameof(PlayerControllerB.TeleportPlayer))]
    [HarmonyPostfix]
    static void TeleportPlayer_Postfix(PlayerControllerB __instance, Vector3 pos, bool withRotation = false, float rot = 0f, bool allowInteractTrigger = false, bool enableController = true)
    {
        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (References.truckController == null)
            return;

        PlayerUtils.isPlayerOnTruck = false;
        PlayerUtils.isPlayerInStorage = false;
    }

    [HarmonyPatch(nameof(PlayerControllerB.UpdatePlayerAnimationsToOtherClients))]
    [HarmonyPrefix]
    static bool UpdatePlayerAnimationsToOtherClients_Prefix(PlayerControllerB __instance, Vector2 moveInputVector)
    {
        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return true;

        if (PlayerUtils.disableAnimationSync) return false;
        return true;
    }

    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPostfix]
    public static void SyncZoneStateLateUpdate_Postfix(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (References.truckController == null)
            return;
        v55VehicleController controller = References.truckController;

        if (checkInterval < 0.3f)
        {
            checkInterval += Time.deltaTime;
            return;
        }
        checkInterval = 0f;
        var data = GetData(__instance);

        if (data.isPlayerOnTruck != PlayerUtils.isPlayerOnTruck ||
            data.isPlayerInStorage != PlayerUtils.isPlayerInStorage)
        {
            data.isPlayerOnTruck = PlayerUtils.isPlayerOnTruck;
            data.isPlayerInStorage = PlayerUtils.isPlayerInStorage;

            controller.SyncPlayerZoneRpc(
                (int)__instance.playerClientId,
                PlayerUtils.isPlayerOnTruck,
                PlayerUtils.isPlayerInStorage);
        }
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/PlayerController.cs
    /// </summary>
    [HarmonyPatch(nameof(PlayerControllerB.Update))]
    [HarmonyPostfix]
    public static void Update_Postfix(PlayerControllerB __instance)
    {
        if (__instance == null || 
            __instance.isPlayerDead || 
            !__instance.isPlayerControlled)
            return;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (References.truckController == null)
            return;
        v55VehicleController controller = References.truckController;

        bool validTruck = __instance.inVehicleAnimation && __instance.currentTriggerInAnimationWith && __instance.currentTriggerInAnimationWith.overridePlayerParent;
        if (validTruck && __instance.currentTriggerInAnimationWith.overridePlayerParent == controller.transform)
        {
            PlayerUtils.seatedInTruck = true;
            PlayerUtils.isPlayerOnTruck = true;
            PlayerUtils.isPlayerInStorage = false;
        }
        else if (!__instance.inVehicleAnimation && PlayerUtils.seatedInTruck == true)
            PlayerUtils.seatedInTruck = false;
    }

    // this fixes a really annoying visual bug with the players model, as 
    // various parts such as the first person arms can become disaligned
    // and cause obvious visual problems such as the ignition key not 
    // aligning properly during the ignition animation, or even causing
    // the players body to shift backwards, resulting in their hands
    // not visually holding anything.
    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPostfix]
    private static void LateUpdate_Postfix(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        bool validTruck = __instance.inVehicleAnimation &&
            __instance.currentTriggerInAnimationWith &&
            __instance.currentTriggerInAnimationWith.overridePlayerParent;

        if (validTruck &&
            __instance.currentTriggerInAnimationWith.overridePlayerParent.TryGetComponent<v55VehicleController>(out var controller))
        {
            // fix players first-person arms orientation after interacting with certain objects (i.e. terminal, start round lever) causing visual issues such as the ignition-key animation being off
            __instance.playerModelArmsMetarig.parent.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            __instance.playerModelArmsMetarig.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            __instance.localArmsTransform.localPosition = new Vector3(0, -0.008f, -0.43f);
            __instance.localArmsTransform.localRotation = Quaternion.Euler(84.78056f, 0f, 0f);
            __instance.playerBodyAnimator.transform.localPosition = controller.playerPositionOffset;
            __instance.playerBodyAnimator.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        }
    }
}