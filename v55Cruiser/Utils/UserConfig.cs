using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace v55Cruiser.Utils;

internal class UserConfig
{
    // Host
    internal static ConfigEntry<bool> BabyFaceRadio = null!;
    internal static ConfigEntry<bool> RightHandedWheel = null!;
    internal static ConfigEntry<bool> PostBeta = null!;

    internal static ConfigEntry<bool> NoTreeDestruction = null!;
    //internal static ConfigEntry<bool> PreBeta = null!;

    internal static void InitConfig()
    {
        ConfigFile config = Plugin.Instance.Config;
        config.SaveOnConfigSet = false;

        // Host
        BabyFaceRadio = config.Bind("Host", "Baby-Face", false, "[Host] If true, will enable the unused (copyrighted) 'Baby-Face' radio track");
        RightHandedWheel = config.Bind("Host", "Right-Hand-Drive", false, "[Host] [Must disable True-V55] If true, will enable a random chance for the Cruiser to be right-hand-drive");
        PostBeta = config.Bind("Host", "True-V55", true, "[Host] If true, will unfix some issues, and make the Cruiser 'more true to V55'");
        NoTreeDestruction = config.Bind("Host", "No Tree Destruction", false, "[Host] [Must enable True-V55] If true, Trees will act like any other solid wall and be unbreakable by the Cruiser like >6 hours before V55 beta went live. Includes snowmen.");
        //PreBeta = config.Bind("Host", "Pre-Beta", false, "[Host] [Must enable True-V55] If true, will bring the Cruiser slightly closer to its pre-release form as seen in various videos from Zeekerss (No ejector seat, No cabin light, Smaller + more in-set tyres)");

        ClearOrphanedEntries(config);
        config.Save();
        config.SaveOnConfigSet = true;
    }

    static void ClearOrphanedEntries(ConfigFile config)
    {
        PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(config);
        orphanedEntries.Clear();
    }
}
