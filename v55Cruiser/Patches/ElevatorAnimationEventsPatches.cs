using GameNetcodeStuff;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(ElevatorAnimationEvents))]
public static class ElevatorAnimationEventsPatches
{
    [HarmonyPatch(nameof(ElevatorAnimationEvents.ElevatorFullyRunning))]
    [HarmonyPrefix]
    static void ElevatorFullyRunning_Prefix()
    {
        v55VehicleController controller = References.truckController;
        if (controller == null) 
            return;

        // do not save players who are on the magneted truck from being abandoned
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (PlayerUtils.isSeatedInTruck ||
            VehicleUtils.IsPlayerInTruckBounds(controller) ||
            VehicleUtils.IsPlayerInTruckStorage(controller))
            localPlayer.isInElevator = false;
    }
}
