using HarmonyLib;
using Woecust.ImmersiveVisor;
using v55Cruiser.Utils;
using GameNetcodeStuff;

namespace v55Cruiser.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
[HarmonyPatch]
public static class ImmersiveVisorCompatibility
{
    [HarmonyPatch(typeof(VisorRainState), nameof(VisorRainState.LineCastForCeiling))]
    [HarmonyPrefix]
    public static bool VisorRainState_Pre_LineCastForCeiling(VisorRainState __instance, ref bool __result)
    {
        v55VehicleController truckController = VehicleUtils.truckController;
        if (truckController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (VehicleUtils.IsPlayerInTruckStorage(playerController, truckController))
        {
            __result = true;
            return false;
        }
        return true;
    }
}
