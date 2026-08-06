using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using v55Cruiser.Utils;
using System.Runtime.CompilerServices;
using v55Cruiser.Patches.InternalUtils;
using v55Cruiser.Patches.Enemies;
using v55Cruiser.Patches;

namespace v55Cruiser;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(CompatibilityUtils.GUID_SCANDALLIB, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(CompatibilityUtils.GUID_SCANDALS_TWEAKS, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(CompatibilityUtils.GUID_LETHALELEMENTS, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(CompatibilityUtils.GUID_LETHALMIN, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(CompatibilityUtils.GUID_IMMERSIVE_VISOR, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(CompatibilityUtils.GUID_FIRST_PERSON_VIEW, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    internal static List<GameObject> networkPrefabs = new List<GameObject>();

    internal static bool initialized = false;

    public void Awake()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;
        Logger = base.Logger;
        Instance = this;

        LoadAssetBundlesAndRegister();
        UserConfig.InitConfig();

        Patch();
        LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private void LoadAssetBundlesAndRegister()
    {
        AssetBundle CompanyCruiserBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "v55cruiser"));
        if (CompanyCruiserBundle == null)
        {
            LogError("[AssetBundle] Failed to load asset bundle: v55cruiser");
            return;
        }

        References.companyCruiserPrefab = CompanyCruiserBundle.LoadAsset<GameObject>("CompanyCruiser.prefab");
        if (References.companyCruiserPrefab != null)
        {
            if (!networkPrefabs.Contains(References.companyCruiserPrefab))
                networkPrefabs.Add(References.companyCruiserPrefab);
            LogInfo("[AssetBundle] Successfully loaded prefab: CompanyCruiser");
        }
        else
        {
            LogError("[AssetBundle] Failed to load prefab: CompanyCruiser");
        }

        References.companyCruiserManualPrefab = CompanyCruiserBundle.LoadAsset<GameObject>("CompanyCruiserManual.prefab");
        if (References.companyCruiserManualPrefab != null)
        {
            if (!networkPrefabs.Contains(References.companyCruiserManualPrefab))
                networkPrefabs.Add(References.companyCruiserManualPrefab);
            LogInfo("[AssetBundle] Successfully loaded prefab: CompanyCruiserManual");
        }
        else
        {
            LogError("[AssetBundle] Failed to load prefab: CompanyCruiserManual");
        }
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        LogDebug("Patching...");

        CompatibilityUtils.Init(Harmony);

        Harmony.PatchAll(type: typeof(ScandalsTweaksPatches));
        Harmony.PatchAll(type: typeof(BaboonBirdAIPatches));
        //Harmony.PatchAll(type: typeof(EnemyAIPatches));
        Harmony.PatchAll(type: typeof(ForestGiantAIPatches));
        Harmony.PatchAll(type: typeof(GiantKiwiAIPatches));
        Harmony.PatchAll(type: typeof(MaskedPlayerEnemyPatches));
        Harmony.PatchAll(type: typeof(MouthDogAIPatches));
        Harmony.PatchAll(type: typeof(PumaAIPatches));
        Harmony.PatchAll(type: typeof(RadMechAIPatches));
        Harmony.PatchAll(type: typeof(RedLocustBeesPatches));
        Harmony.PatchAll(type: typeof(ElevatorAnimationEventsPatches));
        Harmony.PatchAll(type: typeof(GameNetworkManagerPatches));
        Harmony.PatchAll(type: typeof(HUDManagerPatches));
        Harmony.PatchAll(type: typeof(LandminePatches));
        Harmony.PatchAll(type: typeof(PlayerControllerBPatches));
        Harmony.PatchAll(type: typeof(StartOfRoundPatches));
        Harmony.PatchAll(type: typeof(TerminalPatches));
        Harmony.PatchAll(type: typeof(TerrainObstacleTriggerPatches));
        Harmony.PatchAll(type: typeof(VehicleControllerPatches));

        LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        LogDebug("Finished unpatching!");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void LogMessage(string messageLog)
    {
        Logger.LogMessage(messageLog);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void LogDebug(string debugLog)
    {
        Logger.LogDebug(debugLog);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void LogInfo(string infoLog)
    {
        Logger.LogInfo(infoLog);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void LogWarning(string warningLog)
    {
        Logger.LogWarning(warningLog);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void LogError(string errorLog)
    {
        Logger.LogError(errorLog);
    }
}