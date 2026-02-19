using v55Cruiser.Patches;
using v55Cruiser.Utils;
using GameNetcodeStuff;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace v55Cruiser.Behaviour;
[MovedFrom("v55Cruiser.MonoBehaviours.Vehicles.v55Cruiser")]
public class TruckPlayerZone : MonoBehaviour
{
    public v55VehicleController controller = null!;
    public int priority;
    public float checkInterval;
    public float stayTimer;

    public void LateUpdate()
    {
        if (controller == null) return;
        if (checkInterval < 0.3f)
        {
            checkInterval += Time.deltaTime;
            return;
        }
        checkInterval = 0f;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerNearTruck(playerController, controller) &&
            (PlayerUtils.isPlayerOnTruck ||
            PlayerUtils.isPlayerInStorage))
        {
            PlayerUtils.isPlayerOnTruck = false;
            PlayerUtils.isPlayerInStorage = false;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (controller == null) return;
        if (PlayerUtils.seatedInTruck) return;
        if (other.gameObject != GameNetworkManager.Instance.localPlayerController.gameObject) return;

        switch (priority)
        {
            case 1:
                PlayerUtils.isPlayerOnTruck = false;
                if (controller.averageVelocity.magnitude >= 2f &&
                    controller.mainRigidbody.velocity.magnitude >= 2f)
                {
                    GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += controller.averageVelocity * 0.9f;
                }
                break;
            case 2:
                if (!PlayerUtils.seatedInTruck)
                    PlayerUtils.isPlayerInStorage = false;
                break;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (controller == null) return;
        if (PlayerUtils.seatedInTruck) return;
        if (other.gameObject != GameNetworkManager.Instance.localPlayerController.gameObject) return;

        stayTimer += Time.fixedDeltaTime;
        if (stayTimer < 0.4f) return;
        stayTimer = 0f;

        switch (priority)
        {
            case 1:
                PlayerUtils.isPlayerOnTruck = true;
                break;
            case 2:
                if (!PlayerUtils.seatedInTruck)
                    PlayerUtils.isPlayerInStorage = true;
                break;
        }
    }

    public void ClearZoneState()
    {
        switch (priority)
        {
            case 1:
                PlayerUtils.isPlayerOnTruck = false;
                break;
            case 2:
                PlayerUtils.isPlayerInStorage = false;
                break;
        }
    }

    public void OnDisable() => ClearZoneState();
    public void OnDestroy() => ClearZoneState();
}
