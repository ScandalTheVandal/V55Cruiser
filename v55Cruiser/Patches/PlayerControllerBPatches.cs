using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using v55Cruiser.Networking;
using v55Cruiser.Utils;


namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBPatches
{
    public class PlayerControllerBData
    {
        public float syncedCameraHorizontal;

        public bool playerRidingOnTruck;
        public bool playerRidingInTruckStorage;
    }

    public static Dictionary<PlayerControllerB, PlayerControllerBData> playerData = new();
    private static float checkInterval;

    // optimisation
    private static Quaternion armsMetarigParentRot = Quaternion.Euler(90f, 0f, 0f);
    private static Quaternion armsMetarigRot = Quaternion.Euler(-90f, 0f, 0f);

    private static Vector3 localArmsPos = new Vector3(0f, -0.008f, -0.43f);
    private static Quaternion localArmsRot = Quaternion.Euler(84.78056f, 0f, 0f);

    private static Vector3 playerBodyPos = Vector3.zero;
    private static Quaternion playerBodyRot = Quaternion.Euler(-90f, 0f, 0f);

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

    [HarmonyPatch(nameof(PlayerControllerB.Awake))]
    [HarmonyPostfix]
    static void PlayerControllerB_Post_Awake(PlayerControllerB __instance)
    {
        RemoveStalePlayerData();
        if (!playerData.ContainsKey(__instance))
        {
            PlayerControllerBData thisData = new();
            playerData.Add(__instance, thisData);
        }
    }

    [HarmonyPatch(nameof(PlayerControllerB.SetItemInElevator))]
    [HarmonyPrefix]
    static bool PlayerControllerB_Pre_SetItemInElevator(PlayerControllerB __instance, bool droppedInShipRoom, bool droppedInElevator, GrabbableObject gObject)
    {
        v55VehicleController vehicle = VehicleUtils.truckController;
        if (vehicle == null)
            return true;

        if (gObject.transform.parent == vehicle.transform)
            return false;
        return true;
    }

    [HarmonyPatch(nameof(PlayerControllerB.Update))]
    [HarmonyPostfix]
    public static void PlayerControllerB_Post_Update(PlayerControllerB __instance)
    {
        if (__instance == null ||
            !__instance.isPlayerControlled ||
            __instance != GameNetworkManager.Instance.localPlayerController)
        {
            return;
        }
        SyncPlayerLookInput(__instance);
    }

    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPrefix]
    public static void PlayerControllerB_Pre_LateUpdate(PlayerControllerB __instance)
    {
        if (__instance == null ||
            !__instance.isPlayerControlled ||
            __instance != GameNetworkManager.Instance.localPlayerController)
        {
            return;
        }
        SetPlayerVehicleZone(__instance);
    }


    private static void SetPlayerVehicleZone(PlayerControllerB playerController)
    {
        v55VehicleController truckController = VehicleUtils.truckController;

        var playerData = PlayerControllerBPatches.playerData[playerController];
        bool ridingInTruckStorage = truckController?.vehicleStorageZone.playerInZone ?? false;
        bool ridingOnTruck = truckController?.vehicleZone.playerInZone ?? false;

        if (playerData.playerRidingInTruckStorage == ridingInTruckStorage &&
            playerData.playerRidingOnTruck == ridingOnTruck)
        {
            return;
        }

        playerData.playerRidingInTruckStorage = ridingInTruckStorage;
        playerData.playerRidingOnTruck = ridingOnTruck;
        V55Networker.Instance?.SyncPlayerZoneRpc(playerController.NetworkObject,
                                                 ridingInTruckStorage,
                                                 ridingOnTruck);
    }

    private static void SyncPlayerLookInput(PlayerControllerB playerController)
    {
        if (checkInterval >= 0.15f)
        {
            if (playerData[playerController].syncedCameraHorizontal != playerController.ladderCameraHorizontal)
            {
                checkInterval = 0f;
                playerData[playerController].syncedCameraHorizontal = playerController.ladderCameraHorizontal;
                V55Networker.Instance?.SyncPlayerLookInputRpc(playerController.NetworkObject, playerController.ladderCameraHorizontal);
                return;
            }
        }
        else
        {
            checkInterval += Time.deltaTime;
        }
    }

    // this fixes a really annoying visual bug with the players model, as 
    // various parts such as the first person arms can become disaligned
    // and cause obvious visual problems such as the ignition key not 
    // aligning properly during the ignition animation, or even causing
    // the players body to shift backwards, resulting in their hands
    // not visually holding anything.
    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPostfix]
    private static void PlayerControllerB_Post_LateUpdate(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
        {
            return;
        }

        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        if (!VehicleUtils.IsPlayerSeatedInTruck(__instance, truckController))
            return;

        var playerModelArmsRig = __instance.playerModelArmsMetarig;
        var localArmsTransform = __instance.localArmsTransform;
        var playerBodyAnimator = __instance.playerBodyAnimator.transform;

        playerModelArmsRig.parent.transform.localRotation = armsMetarigParentRot;
        playerModelArmsRig.localRotation = armsMetarigRot;
        localArmsTransform.localPosition = localArmsPos;
        localArmsTransform.localRotation = localArmsRot;
        playerBodyAnimator.localPosition = playerBodyPos;
        playerBodyAnimator.localRotation = playerBodyRot;
    }
}