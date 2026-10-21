using GameNetcodeStuff;
using ScandalLib.Interfaces;
using ScandalLib.Scripts;

namespace v55Cruiser.Scripts;

public class v55SeatAnimator : VehicleSeatAnimator
{
    public v55VehicleController vehicleController = null!;

    public override void OnPlayerLeaveGame()
    {
        if (seatTrigger == vehicleController.driverSeatTrigger) vehicleController.OnDriverLeaveGameServerRpc(seatTrigger.playerScriptInSpecialAnimation);
        else if (seatTrigger == vehicleController.passengerSeatTrigger) vehicleController.OnPassengerLeaveGameRpc(seatTrigger.playerScriptInSpecialAnimation);
        else return;
    }

    public override void ResetPlayerData(PlayerControllerB player)
    {
        player.ladderCameraHorizontal = 0f;
        if (player is IPlayerManager playerInterface)
        {
            playerInterface.SyncedLookInput = 0f;
        }
    }
}
