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
        if (References.truckController == null) return;
        if (!References.truckController.magnetedToShip) return;

        // save players who are on the magneted truck from being abandoned
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (PlayerUtils.seatedInTruck || PlayerUtils.isPlayerOnTruck)
            localPlayer.isInElevator = true;
    }
}
