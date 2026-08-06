using GameNetcodeStuff;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(ElevatorAnimationEvents))]
public static class ElevatorAnimationEventsPatches
{
    [HarmonyPatch(nameof(ElevatorAnimationEvents.ElevatorFullyRunning))]
    [HarmonyPrefix]
    static void ElevatorAnimationEvents_Pre_ElevatorFullyRunning()
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInTruck(playerController, truckController);
        bool playerOnTruck = VehicleUtils.IsPlayerInTruckBounds(playerController, truckController);
        bool playerInStorage = VehicleUtils.IsPlayerInTruckStorage(playerController, truckController);

        if (playerSeated || playerOnTruck || playerInStorage)
            playerController.isInElevator = false;
    }
}
