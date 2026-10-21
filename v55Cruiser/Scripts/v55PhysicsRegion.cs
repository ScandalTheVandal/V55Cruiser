using GameNetcodeStuff;
using ScandalLib.Scripts;
using UnityEngine;

namespace v55Cruiser.Scripts;

public class v55PhysicsRegion : ExtendedPhysicsRegion
{
    public v55VehicleController vehicleController = null!;

    // must null check before calling
    internal bool IsLocalPlayerInShelter()
    {
        return vehicleController.vehicleStorageZone.playerInZone && vehicleController.IsStorageEnclosed();
    }

    public override bool IsTargetPlayerInVehicle(PlayerControllerB playerScript, EnemyAI enemyAi)
    {
        switch (enemyAi)
        {
            case GiantKiwiAI:
                return vehicleController.IsPlayerSeated(playerScript) || 
                    (vehicleController.IsPlayerInStorage(playerScript) && vehicleController.IsStorageEnclosed() || 
                    vehicleController.ontopOfTruckCollider.ClosestPoint(playerScript.transform.position) == playerScript.transform.position);
            default:
                return false;
        }
    }

    public override bool IsPlayerProtectedFromEnemy(PlayerControllerB playerScript, EnemyAI enemyAi)
    {
        switch (enemyAi)
        {
            case BaboonBirdAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 8f, enemyCanBeInTruck: true);
            case ForestGiantAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 10f);
            case GiantKiwiAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 12f, enemyCanBeInTruck: true);
            case MaskedPlayerEnemy:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 4f, enemyCanBeInTruck: true);
            case MouthDogAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 12f);
            case PumaAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 9f, enemyCanBeInTruck: true);
            case RadMechAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 10f);
            case RedLocustBees:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 8f, enemyCanBeInTruck: true);
            case SandWormAI:
                return vehicleController.IsPlayerSeated(playerScript) ||
                       vehicleController.IsPlayerInStorage(playerScript);
            case BushWolfEnemy:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, velocityCheck: true, velocityThreshold: 6f, enemyCanBeInTruck: true);
            default:
                return false;
        }
    }

    internal bool IsPlayerProtectedByVehicle(PlayerControllerB playerScript, EnemyAI enemyAi, bool velocityCheck = false, float velocityThreshold = 0f, bool enemyCanBeInTruck = false)
    {
        var enemyInTruck = enemyCanBeInTruck && IsEnemyInTruck(enemyAi, vehicleController.collisionTrigger.insideTruckNavMeshBounds);

        var playerOnVan = vehicleController.IsPlayerAttached(playerScript);
        var playerInStorage = vehicleController.IsPlayerInStorage(playerScript);
        var playerSeated = vehicleController.IsPlayerSeated(playerScript);
        var storageEnclosed = vehicleController.IsStorageEnclosed();

        if (playerSeated)
            return IsSeatedPlayerProtected(playerScript, velocityCheck, velocityThreshold);

        if (!playerOnVan)
            return !enemyInTruck;

        if (playerInStorage && storageEnclosed && !enemyInTruck)
            return true;

        if (enemyInTruck && playerInStorage)
            return false;

        if (velocityCheck)
        {
            float vehicleSpeed = vehicleController.averageVelocity.magnitude;
            if (vehicleSpeed >= velocityThreshold)
                return true;
        }

        return IsPlayerProtectedByTruck(playerScript, velocityCheck, velocityThreshold);
    }

    public bool IsSeatedPlayerProtected(PlayerControllerB playerScript, bool velocityCheck = false, float velocityThreshold = 0f)
    {
        float vehicleSpeed = vehicleController.averageVelocity.magnitude;
        if (velocityCheck && vehicleSpeed >= velocityThreshold)
            return true;
        return false;
    }

    public bool IsPlayerProtectedByTruck(PlayerControllerB playerScript, bool velocityCheck = false, float velocityThreshold = 0f)
    {
        if (vehicleController.carDestroyed)
            return false;
        if (velocityCheck && vehicleController.averageVelocity.magnitude >= velocityThreshold)
        {
            return true;
        }
        return vehicleController.IsPlayerInStorage(playerScript)
            && vehicleController.IsStorageEnclosed();
    }

    public bool IsEnemyInTruck(EnemyAI? enemyScript, BoxCollider? navMeshBounds)
    {
        return enemyScript != null && navMeshBounds != null &&
              (navMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position ||
               navMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination);
    }

    public override bool IsLocalPlayerSheltered()
    {
        return vehicleController != null && IsLocalPlayerInShelter();
    }
}