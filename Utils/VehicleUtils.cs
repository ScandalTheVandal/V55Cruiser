using GameNetcodeStuff;
using UnityEngine;

namespace v55Cruiser.Utils;
public static class VehicleUtils
{
    public static float lastCheckTime = 0f;
    public static float cooldown = 0.25f;

    // kind of unused
    public static bool MeetsSpecialConditionsToCheck()
    {
        if (Time.realtimeSinceStartup - lastCheckTime < cooldown)
            return false;

        lastCheckTime = Time.realtimeSinceStartup;
        return true;
    }

    public static bool IsEnemyInVehicle(EnemyAI enemyScript, v55VehicleController controller)
    {
        if ((controller.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position) ||
            (controller.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination))
            return true;
        return false;
    }

    public static bool IsPlayerInVehicleBounds()
    {
        return PlayerUtils.isPlayerOnTruck;
    }

    public static bool IsPlayerSeatedInVehicle(v55VehicleController controller)
    {
        return PlayerUtils.seatedInTruck;
    }

    public static bool IsSeatedPlayerProtected(PlayerControllerB player, v55VehicleController controller)
    {
        return false; // no protection lol
    }

    public static bool IsPlayerProtectedByVehicle(PlayerControllerB player, v55VehicleController controller)
    {
        if (controller.carDestroyed)
            return false;

        bool backDoorOpen = controller.liftGateOpen;

        if (PlayerUtils.isPlayerInStorage && backDoorOpen)
            return false;
        else if (PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInStorage)
            return false;

        return true;
    }

    public static bool IsPlayerNearTruck(PlayerControllerB player, v55VehicleController vehicle)
    {
        Vector3 vehicleTransform = vehicle.mainRigidbody.position;
        Vector3 playerTransform = player.transform.position;

        if (Vector3.Distance(playerTransform, vehicleTransform) > 10f)
            return false;

        return true;
    }
}