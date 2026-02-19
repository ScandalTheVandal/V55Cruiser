using BepInEx.Bootstrap;
using v55Cruiser.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using v55Cruiser;
using VoxxWeatherPlugin.Patches;

namespace v55Cruiser.Compatibility;

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
