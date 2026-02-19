using HarmonyLib;
using v55Cruiser.Utils;

namespace v55Cruiser.Patches;

[HarmonyPatch(typeof(SoundManager))]
public class SoundManagerPatches
{
    [HarmonyPatch(nameof(SoundManager.Start))]
    [HarmonyPrefix]
    static void Start_Prefix(SoundManager __instance)
    {
        if (References.diageticSFXGroup == null)
        {
            var sfxGroup = __instance.diageticMixer.FindMatchingGroups("Master/SFX")[0];
            References.diageticSFXGroup = sfxGroup;
        }
    }
}
