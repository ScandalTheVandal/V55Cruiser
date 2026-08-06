using HarmonyLib;
using FirstPersonView;

namespace v55Cruiser.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
[HarmonyPatch]
public static class FirstPersonViewCompatibility
{
    [HarmonyPatch(typeof(v55VehicleController), nameof(v55VehicleController.SetIgnitionKey))]
    [HarmonyPrefix]
    public static void v55VehicleController_Pre_SetCarKeyEffects(v55VehicleController __instance, ref bool localUseBodyHands)
    {
        if (!LocalBodyViewController.LocalBodyShown)
            return;
        localUseBodyHands = true;
    }
}
