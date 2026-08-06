using HarmonyLib;
using UnityEngine;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(TerrainObstacleTrigger))]
public static class TerrainObstacleTriggerPatches
{
    [HarmonyPatch(nameof(TerrainObstacleTrigger.OnTriggerEnter))]
    [HarmonyPrefix]
    static bool TerrainObstacleTrigger_Pre_OnTriggerEnter(TerrainObstacleTrigger __instance, bool __runOriginal, Collider other)
    {
        if (!__runOriginal)
            return false;

        if (!other.TryGetComponent<v55VehicleController>(out var truckController))
            return true;

        if (!truckController.IsOwner || !truckController.canDestroyTrees)
            return false;

        bool velocityMagnitude = truckController.averageVelocity.magnitude >= 5f;
        bool angleToObstacle = 
            Vector3.Angle(
            truckController.averageVelocity, 
            __instance.transform.position - truckController.mainRigidbody.position) < 80f;

        if (!velocityMagnitude || !angleToObstacle)
            return true;

        RoundManager.Instance.DestroyTreeOnLocalClient(
            __instance.transform.position);

        bool isTree = 
            __instance.transform.parent != null && 
            __instance.transform.parent.CompareTag("Wood");

        truckController.CarReactToObstacle(
            truckController.mainRigidbody.position - __instance.transform.position, 
            __instance.transform.position,
            Vector3.zero,
            CarObstacleType.Object,
            1f,
            null!,
            isTree);

        return false;
    }
}