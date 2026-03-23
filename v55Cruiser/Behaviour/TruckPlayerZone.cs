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
    public bool hasLocalPlayer;
    public int priority;
    public float checkInterval;

    public void Start()
    {
        checkInterval = Random.Range(0f, 0.4f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (controller == null) return;
        if (other.gameObject != GameNetworkManager.Instance.localPlayerController.gameObject) return;
        if (PlayerUtils.seatedInTruck) return;

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
                PlayerUtils.isPlayerInStorage = false;
                break;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (controller == null) return;
        if (other.gameObject != GameNetworkManager.Instance.localPlayerController.gameObject) return;
        if (PlayerUtils.seatedInTruck) return;

        switch (priority)
        {
            case 1:
                if (!PlayerUtils.isPlayerInStorage)
                    PlayerUtils.isPlayerOnTruck = true;
                break;
            case 2:
                PlayerUtils.isPlayerOnTruck = true;
                PlayerUtils.isPlayerInStorage = true;
                break;
        }
        checkInterval = 0f;
        hasLocalPlayer = true;
    }

    private void Update()
    {
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerNearTruck(playerController, controller) &&
            (PlayerUtils.isPlayerOnTruck ||
            PlayerUtils.isPlayerInStorage))
        {
            PlayerUtils.isPlayerOnTruck = false;
            PlayerUtils.isPlayerInStorage = false;
            checkInterval = 0f;
            hasLocalPlayer = false;
        }
        if (!hasLocalPlayer)
        {
            return;
        }
        if (checkInterval <= 0.2f)
        {
            checkInterval += Time.deltaTime;
            return;
        }
        checkInterval = 0f;
        hasLocalPlayer = false;
    }
}
