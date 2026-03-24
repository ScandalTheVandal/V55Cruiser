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
        if (PlayerUtils.seatedInTruck)
        {
            checkInterval = 0f;
            if (priority == 1) hasLocalPlayer = true;
            else hasLocalPlayer = false;
            return;
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
        if (priority == 1) PlayerUtils.isPlayerOnTruck = false;
        else PlayerUtils.isPlayerInStorage = false;
    }
}
