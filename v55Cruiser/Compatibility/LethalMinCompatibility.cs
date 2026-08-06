using HarmonyLib;
using System;
using UnityEngine;
using LethalMin;

namespace v55Cruiser.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
[HarmonyPatch]
public static class LethalMinCompatibility
{
    [HarmonyPatch(typeof(PikminVehicleController), nameof(PikminVehicleController.InitializeReferences))]
    [HarmonyPrefix]
    public static bool PikminVehicleController_Pre_InitializeReferences(PikminVehicleController __instance)
    {
        if (__instance.TryGetComponent<v55VehicleController>(out var truckController))
        {
            __instance.controller = truckController;
            __instance.PointsRegion = truckController.collisionTrigger.insideTruckNavMeshBounds;
            __instance.PikminCheckRegion = truckController.storageCompartment;
            __instance.PikminWarpPoint = new GameObject("Pikmin Warp Point").transform;
            __instance.PikminWarpPoint.SetParent(__instance.transform);
            __instance.PikminWarpPoint.localPosition = new Vector3(0f, -2f, -5f);
            __instance.PikminWarpPoint.localScale = new Vector3(1f, 1f, 1f);
            __instance.OriginalWTLocalPosition = __instance.PikminWarpPoint.localPosition;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(v55VehicleCollisionTrigger), nameof(v55VehicleCollisionTrigger.OnTriggerEnter))]
    [HarmonyPrefix]
    public static bool v55VehicleCollisionTrigger_Pre_OnTriggerEnter(v55VehicleCollisionTrigger __instance, Collider other)
    {
        try
        {
            if (other.gameObject.CompareTag("Enemy") && other.gameObject.TryGetComponent<PikminCollisionDetect>(out _))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Plugin.LogError(string.Format("Error in `v55VehicleCollisionTrigger.OnTriggerEnter`: {0}", e));
            return true;
        }
        return true;
    }
}
