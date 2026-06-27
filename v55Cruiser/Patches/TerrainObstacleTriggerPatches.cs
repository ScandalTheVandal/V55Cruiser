using HarmonyLib;
using UnityEngine;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(TerrainObstacleTrigger))]
public static class TerrainObstacleTriggerPatches
{
    [HarmonyPatch(nameof(TerrainObstacleTrigger.OnTriggerEnter))]
    [HarmonyPrefix]
    static bool OnTriggerEnter_Prefix(TerrainObstacleTrigger __instance, bool __runOriginal, Collider other)
    {
        if (!__runOriginal)
        {
            return false;
        }
        if (!other.TryGetComponent<v55VehicleController>(out var controller))
        {
            return true;
        }
        if (!controller.IsOwner || !controller.canDestroyTrees)
        {
            return false;
        }
        if (controller.averageVelocity.magnitude >= 5f && Vector3.Angle(controller.averageVelocity, __instance.transform.position - controller.mainRigidbody.position) < 80f)
        {
            RoundManager.Instance.DestroyTreeOnLocalClient(__instance.transform.position);
            bool isObjectATree = __instance.transform.parent != null && __instance.transform.parent.CompareTag("Wood");
            controller.CarReactToObstacle(controller.mainRigidbody.position - __instance.transform.position, __instance.transform.position, Vector3.zero, CarObstacleType.Object, 1f, null!, isObjectATree);
            return false;
        }
        return true;
    }
}