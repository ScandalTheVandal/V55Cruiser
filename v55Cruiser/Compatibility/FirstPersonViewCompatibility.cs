using HarmonyLib;
using System.Runtime.CompilerServices;
using FirstPersonView;

namespace v55Cruiser.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>

public static class FirstPersonViewCompatibility
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAllMethods(Harmony harmony)
    {
        ApplyPatch(harmony);
    }

    [HarmonyPrefix]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ApplyPatch(Harmony harmony)
    {
        var keyEffectMethod = AccessTools.Method(typeof(v55VehicleController), nameof(v55VehicleController.SetCarKeyEffects));
        var prefixKeyEffectMethod = AccessTools.Method(typeof(FirstPersonViewCompatibility), nameof(SetCarKeyEffects_Prefix));

        harmony.Patch(keyEffectMethod, prefix: new HarmonyMethod(prefixKeyEffectMethod));
    }

    public static void SetCarKeyEffects_Prefix(v55VehicleController __instance, ref bool localUseBodyHands)
    {
        if (!LocalBodyViewController.LocalBodyShown)
            return;
        localUseBodyHands = true;
    }
}
