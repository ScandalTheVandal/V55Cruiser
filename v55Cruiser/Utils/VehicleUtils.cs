using GameNetcodeStuff;
using UnityEngine;
using v55Cruiser.Patches;

namespace v55Cruiser.Utils;
public static class VehicleUtils
{
    internal static v55VehicleController truckController = null!;

    public static bool IsEnemyInTruck(EnemyAI enemyScript, BoxCollider navMeshBounds)
    {
        return navMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position ||
               navMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination;
    }

    public static bool IsTruckStorageEnclosed(v55VehicleController truckScript)
    {
        return !truckScript.liftGateOpen;
    }

    public static bool IsPlayerInTruckBounds(PlayerControllerB playerScript, v55VehicleController truckScript, bool usePlayerData = false)
    {
        return usePlayerData
            ? PlayerControllerBPatches.playerData[playerScript].playerRidingOnTruck
            : truckScript.vehicleZone.playerInZone;
    }

    public static bool IsPlayerInTruckStorage(PlayerControllerB playerScript, v55VehicleController truckScript, bool usePlayerData = false)
    {
        return usePlayerData
            ? PlayerControllerBPatches.playerData[playerScript].playerRidingInTruckStorage
            : truckScript.vehicleStorageZone.playerInZone;
    }

    public static bool IsPlayerSeatedInTruck(PlayerControllerB playerScript, v55VehicleController truckScript)
    {
        return playerScript != null && playerScript?.overridePhysicsParent == truckScript?.transform;
    }

    public static bool IsPlayerOnOutsideOfTruck(PlayerControllerB playerScript, v55VehicleController truckScript, bool usePlayerData = false)
    {
        return IsPlayerInTruckBounds(playerScript, truckScript, usePlayerData) && 
               !IsPlayerInTruckStorage(playerScript, truckScript, usePlayerData) &&
               !IsPlayerSeatedInTruck(playerScript, truckScript);
    }

    public static bool IsTruckMovingFastEnough(v55VehicleController truckScript, float threshold)
    {
        return truckScript.averageVelocity.magnitude > threshold;
    }

    public static bool IsPlayerProtectedInTruckStorage(PlayerControllerB playerScript, v55VehicleController truckScript, bool usePlayerData = false)
    {
        return IsPlayerInTruckStorage(playerScript, truckScript, usePlayerData) &&
               IsTruckStorageEnclosed(truckScript);
    }

    public static bool IsSeatedPlayerProtectedByTruck(PlayerControllerB playerScript, v55VehicleController truckScript, bool velocityCheck = false, float velocityMagnitude = 0f)
    {
        return velocityCheck && IsTruckMovingFastEnough(truckScript, velocityMagnitude);
    }

    public static bool IsPlayerProtectedByTruck(PlayerControllerB playerScript, v55VehicleController truckScript, bool velocityCheck = false, float velocityMagnitude = 0f, bool usePlayerData = false)
    {
        if (truckScript.carDestroyed)
            return false;

        if (velocityCheck &&
            truckScript.averageVelocity.magnitude > velocityMagnitude)
            return true;

        if (IsPlayerOnOutsideOfTruck(playerScript, truckScript, usePlayerData))
            return false;

        if (!IsTruckStorageEnclosed(truckScript) &&
            IsPlayerInTruckStorage(playerScript, truckScript, usePlayerData))
            return false;

        return true;
    }

    public static bool IsPlayerNearTruck(PlayerControllerB playerScript, v55VehicleController truckScript)
    {
        Vector3 vehicleTransform = truckScript.mainRigidbody.position;
        Vector3 playerTransform = playerScript.transform.position;

        return Vector3.Distance(playerTransform, vehicleTransform) <= 10f;
    }
}