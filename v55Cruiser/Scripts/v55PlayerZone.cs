using GameNetcodeStuff;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser.Scripts;

// A stripped down version of the v55PhysicsRegion, this is just used for the storage compartment space to determine whether a player is in the storage compartment.
// Ignores seated players.
public class v55PlayerZone : MonoBehaviour
{
    public v55VehicleController haulerController = null!;

    public Transform physicsTransform = null!;
    public Collider physicsCollider = null!;

    private float playerInsideInterval;
    private bool playerInsideThisFrame;

    private bool removePlayerFromZoneNextFrame;
    private float checkZoneInterval;

    public bool unsetInZoneWhileSeated;
    public bool setInZoneWhileSeated;

    public bool playerInZone;
    public bool disableZone;


    public void OnEnable()
    {
        if (setInZoneWhileSeated && unsetInZoneWhileSeated)
        {
            Plugin.Logger.LogWarning("V55: 'Set in zone' and 'Unset in zone' are set simulteanously! this will cause issues!");
            Plugin.Logger.LogWarning("V55: Fallback to set behaviour 'Set zone --> not seated'");
            setInZoneWhileSeated = false;
            unsetInZoneWhileSeated = true;
        }
        else if (!setInZoneWhileSeated && !unsetInZoneWhileSeated)
        {
            Plugin.Logger.LogWarning("V55: 'Set in zone' and 'Unset in zone' are unset simulteanously! this will cause issues!");
            Plugin.Logger.LogWarning("V55: Fallback to set behaviour 'Set zone --> not seated'");
            setInZoneWhileSeated = false;
            unsetInZoneWhileSeated = true;
        }
    }

    public void OnDestroy()
    {
        disableZone = true;
    }

    public void OnTriggerStay(Collider other)
    {
        if (disableZone)
        {
            return;
        }
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer == null)
        {
            return;
        }
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }
        if (other.gameObject != localPlayer.gameObject)
        {
            return;
        }
        playerInsideThisFrame = true;
        playerInsideInterval = 0f;
    }


    public void Update()
    {
        if (disableZone)
        {
            return;
        }
        if (VehicleUtils.IsPlayerSeatedInTruck())
        {
            if (setInZoneWhileSeated)
            {
                playerInZone = true;
            }
            else if (unsetInZoneWhileSeated)
            {
                playerInZone = false;
            }
            removePlayerFromZoneNextFrame = false;
            checkZoneInterval = 0f;
            return;
        }
        UpdatePlayerZone();
        SetPlayerZone(ref checkZoneInterval, ref removePlayerFromZoneNextFrame, ref playerInZone);
    }

    public void UpdatePlayerZone()
    {
        if (!playerInsideThisFrame)
        {
            return;
        }
        if (playerInsideThisFrame)
        {
            SetPlayerZoneActive();
        }
        if (playerInsideInterval <= 0.15f)
        {
            playerInsideInterval += Time.deltaTime;
            return;
        }
        playerInsideThisFrame = false;
        playerInsideInterval = 0f;
    }

    private void SetPlayerZoneActive()
    {
        checkZoneInterval = 0f;
        removePlayerFromZoneNextFrame = false;
        playerInZone = true;
    }

    public void SetPlayerZone(ref float checkInterval, ref bool removeNextFrame, ref bool hasPlayer)
    {
        if (!hasPlayer)
        {
            return;
        }
        if (checkInterval <= 0.15f)
        {
            checkInterval += Time.deltaTime;
            return;
        }
        if (!removeNextFrame)
        {
            removeNextFrame = true;
            return;
        }
        removeNextFrame = false;
        checkInterval = 0f;
        hasPlayer = false;
    }
}
