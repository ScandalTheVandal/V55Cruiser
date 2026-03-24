using BepInEx.Bootstrap;
using HarmonyLib;

namespace v55Cruiser.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>

[HarmonyPatch]
public class CompatibilityUtils
{
    internal static bool lethalElementsPresent = false;

    // ArtificeBlizzard compatibility
    //internal static bool artificeBlizzardPresent = false;
    //internal static GameObject artificeBlizzardObj = null!;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
    private static void OnGameLoad()
    {
        lethalElementsPresent = IsModPresent("voxx.LethalElementsPlugin", "Lethal Elements detected!");
        //artificeBlizzardPresent = IsModPresent("butterystancakes.lethalcompany.artificeblizzard", "Artifice Blizzard detected!");
    }

    public static bool IsModPresent(string name, string logMessage = "")
    {
        bool isPresent = Chainloader.PluginInfos.ContainsKey(name);
        if (isPresent)
        {
            Plugin.Logger.LogInfo($"{name} is present. {logMessage}");
        }
        return isPresent;
    }
}
