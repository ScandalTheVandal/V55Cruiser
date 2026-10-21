using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace v55Cruiser.Utils;

internal class UserConfig
{
    // DEBUG
    public static ConfigEntry<bool> EnableDebugLogging = null!;
    public static ConfigEntry<bool> EnableDebugContent = null!;

    // Host
    internal static ConfigEntry<bool> AdditionalRadioMusic = null!;
    internal static ConfigEntry<bool> RightHandedWheel = null!;
    internal static ConfigEntry<bool> PostBeta = null!;
    internal static ConfigEntry<bool> NoTreeDestruction = null!;
    //internal static ConfigEntry<bool> PreBeta = null!;

    internal static void InitConfig()
    {
        ConfigFile config = Plugin.Instance.Config;
        config.SaveOnConfigSet = false;

        // DEBUG
        EnableDebugLogging = config.Bind("Debug", "Debug-Logging", false, "If true, activates debug logging");
        EnableDebugContent = config.Bind("Debug", "Debug-Content", false, "If true, activates debug content");

        // Host
        AdditionalRadioMusic = config.Bind("Host", "Additional Radio Music", false, "[Host] If true, will enable two unused (one copyrighted) radio tracks");
        RightHandedWheel = config.Bind("Host", "Right Hand Drive", true, "[Host] [Must disable True-V55] If true, will enable a random chance for the Cruiser to be right hand drive");
        PostBeta = config.Bind("Host", "True-V55", true, "[Host] If false, will fix some minor issues that do not heavily impact experience (steering, suspension, etc)");
        NoTreeDestruction = config.Bind("Host", "No Tree Destruction", false, "[Host] [Must enable True-V55] If true, Trees will act like any other solid wall and be unbreakable by the Cruiser. (like >6 hours before V55 beta went live). Includes snowmen.");
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
