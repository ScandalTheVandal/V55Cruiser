using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using v55Cruiser.Patches;
using v55Cruiser.Utils;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(EnemyAI))]
public static class EnemyAIPatches
{
    [HarmonyPatch(nameof(EnemyAI.PlayerIsTargetable))]
    [HarmonyPostfix]
    static void PlayerIsTargetable_Postfix(EnemyAI __instance, PlayerControllerB playerScript, bool cannotBeInShip, bool overrideInsideFactoryCheck, ref bool __result)
    {
        if (__instance is not BushWolfEnemy bushWolf) return;
        if (References.truckController == null) return;
        v55VehicleController controller = References.truckController;
        var data = PlayerControllerBPatches.GetData(playerScript);

        if (data.isPlayerInStorage && !controller.liftGateOpen)
            __result = false;
    }
}