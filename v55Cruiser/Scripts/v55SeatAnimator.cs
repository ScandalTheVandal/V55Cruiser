using GameNetcodeStuff;
using ScandalLib.Scripts;
using v55Cruiser.Patches;
using v55Cruiser.Utils;

namespace v55Cruiser.Scripts;

public class v55SeatAnimator : VehicleSeatAnimator
{
    public v55VehicleController vehicleController = null!;

    public override void OnPlayerLeaveGame()
    {
        if (seatTrigger == vehicleController.driverSeatTrigger) vehicleController.OnDriverLeaveGameServerRpc((int)seatTrigger.playerScriptInSpecialAnimation.playerClientId);
        else if (seatTrigger == vehicleController.passengerSeatTrigger) vehicleController.OnPassengerLeaveGameRpc((int)seatTrigger.playerScriptInSpecialAnimation.playerClientId);
        else return;
    }

    public override void ResetPlayerData(PlayerControllerB player)
    {
        player.ladderCameraHorizontal = 0f;
        PlayerControllerBPatches.playerData[player].syncedCameraHorizontal = 0f;
    }
}
