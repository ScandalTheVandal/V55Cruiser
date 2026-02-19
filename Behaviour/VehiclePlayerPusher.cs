using v55Cruiser.Utils;
using GameNetcodeStuff;
using UnityEngine;

namespace v55Cruiser.Behaviour;

public class VehiclePlayerPusher : MonoBehaviour
{
    public v55VehicleController thisController = null!;

    public void OnTriggerStay(Collider other)
    {
        //if (!thisController.IsOwner)
        //    return;

        if (thisController.averageVelocity.magnitude > 8f)
            return;

        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer == null)
            return;

        if (other.gameObject != localPlayer.gameObject)
            return;

        if (PlayerUtils.seatedInTruck ||
            PlayerUtils.isPlayerOnTruck)
            return;

        Vector3 vel = thisController.mainRigidbody.position - thisController.previousVehiclePosition;
        localPlayer.externalForceAutoFade += (vel * 1.5f) / Time.deltaTime;
    }
}
