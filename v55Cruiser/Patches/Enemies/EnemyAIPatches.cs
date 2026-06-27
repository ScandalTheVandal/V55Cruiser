using GameNetcodeStuff;
using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches.Enemies;

[HarmonyPatch(typeof(EnemyAI))]
public static class EnemyAIPatches
{
    [HarmonyPatch(nameof(EnemyAI.PlayerIsTargetable))]
    [HarmonyPostfix]
    static void PlayerIsTargetable_Postfix(EnemyAI __instance, PlayerControllerB playerScript, bool cannotBeInShip, bool overrideInsideFactoryCheck, bool checkForMineshaftStartTile, ref bool __result)
    {
        if (__instance is not BushWolfEnemy)
            return;

        v55VehicleController controller = References.truckController;
        if (controller == null)
            return;

        var playerData = PlayerControllerBPatches.playerData[playerScript];
        if (playerData.playerRidingInTruckStorage && !controller.liftGateOpen)
            __result = false;
    }
}