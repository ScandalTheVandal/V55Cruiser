using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using v55Cruiser.Compatibility;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using v55Cruiser.Utils;

namespace v55Cruiser
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("scandal.scandalstweaks", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("voxx.LethalElementsPlugin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("NoteBoxz.LethalMin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("ImmersiveVisor", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.seeya.firstpersonview", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        internal static List<GameObject> networkPrefabs = new List<GameObject>();

        public void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            LoadAssetBundlesAndRegister();
            UserConfig.InitConfig();

            Patch();
            Logger.LogInfo($"V55: {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void LoadAssetBundlesAndRegister()
        {
            AssetBundle CompanyCruiserBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "v55cruiser"));
            if (CompanyCruiserBundle == null)
            {
                Logger.LogError("V55: [AssetBundle] Failed to load asset bundle: v55cruiser");
                return;
            }

            References.companyCruiserPrefab = CompanyCruiserBundle.LoadAsset<GameObject>("CompanyCruiser.prefab");
            References.companyCruiserManualPrefab = CompanyCruiserBundle.LoadAsset<GameObject>("CompanyCruiserManual.prefab");
            if (References.companyCruiserPrefab != null)
            {
                if (!networkPrefabs.Contains(References.companyCruiserPrefab))
                    networkPrefabs.Add(References.companyCruiserPrefab);
                Logger.LogInfo("V55: [AssetBundle] Successfully loaded prefab: CompanyCruiser");
            }
            else
            {
                Logger.LogError("V55: [AssetBundle] Failed to load prefab: CompanyCruiser");
            }

            if (References.companyCruiserManualPrefab != null)
            {
                if (!networkPrefabs.Contains(References.companyCruiserManualPrefab))
                    networkPrefabs.Add(References.companyCruiserManualPrefab);
                Logger.LogInfo("V55: [AssetBundle] Successfully loaded prefab: CompanyCruiserManual");
            }
            else
            {
                Logger.LogError("V55: [AssetBundle] Failed to load prefab: CompanyCruiserManual");
            }
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            if (IsModPresent("voxx.LethalElementsPlugin")) LethalElementsCompatibility.PatchAllMethods(Harmony);
            if (IsModPresent("NoteBoxz.LethalMin")) LethalMinCompatibility.PatchAllMethods(Harmony);
            if (IsModPresent("ImmersiveVisor")) ImmersiveVisorCompatibility.PatchAllMethods(Harmony);
            if (IsModPresent("com.seeya.firstpersonview")) FirstPersonViewCompatibility.PatchAllMethods(Harmony);

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }

        internal static bool IsModPresent(string name)
        {
            return Chainloader.PluginInfos.ContainsKey(name);
        }
    }
}