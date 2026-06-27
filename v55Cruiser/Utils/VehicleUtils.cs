using GameNetcodeStuff;
using UnityEngine;

namespace v55Cruiser.Utils;
public static class VehicleUtils
{
    public static bool IsEnemyInTruck(EnemyAI enemyScript, v55VehicleController truckController)
    {
        if ((truckController.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position) ||
            (truckController.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination))
            return true;
        return false;
    }

    public static bool IsPlayerInTruckBounds(v55VehicleController truckController)
    {
        return truckController.vehicleZone.playerInZone;
    }

    public static bool IsPlayerInTruckStorage(v55VehicleController truckController)
    {
        return truckController.vehicleStorageZone.playerInZone;
    }

    public static bool IsPlayerSeatedInTruck()
    {
        return PlayerUtils.isSeatedInTruck;
    }

    public static bool IsSeatedPlayerProtected(PlayerControllerB playerController, v55VehicleController truckController, bool velocityCheck = false, float velocityMagnitude = 0f)
    {
        float avgVel = truckController.averageVelocity.magnitude;

        if (velocityCheck && avgVel > velocityMagnitude)
            return true;

        if ((playerController == truckController.currentDriver) ||
            (playerController == truckController.currentPassenger))
            return false;

        return true;
    }

    public static bool IsPlayerProtectedByTruck(PlayerControllerB playerController, v55VehicleController truckController, bool velocityCheck = false, float velocityMagnitude = 0f)
    {
        if (truckController.carDestroyed)
            return false;

        float avgVel = truckController.averageVelocity.magnitude;

        if (velocityCheck && avgVel > velocityMagnitude)
            return true;

        bool backDoorOpen = truckController.liftGateOpen;

        if (IsPlayerInTruckStorage(truckController) && 
            backDoorOpen)
            return false;
        else if (IsPlayerInTruckBounds(truckController) &&
            !IsPlayerInTruckStorage(truckController))
            return false;

        return true;
    }

    public static bool IsPlayerNearTruck(PlayerControllerB playerController, v55VehicleController truckController)
    {
        Vector3 vehicleTransform = truckController.mainRigidbody.position;
        Vector3 playerTransform = playerController.transform.position;

        if (Vector3.Distance(playerTransform, vehicleTransform) > 10f)
            return false;

        return true;
    }
}