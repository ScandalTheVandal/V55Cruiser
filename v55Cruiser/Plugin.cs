using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using v55Cruiser.Compatibility;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using v55Cruiser.Utils;
using static v55Cruiser.Utils.UserVehicleControls;

namespace v55Cruiser
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("ScandalsTweaks", BepInDependency.DependencyFlags.HardDependency)] // need to rename this
    [BepInDependency("voxx.LethalElementsPlugin", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private static bool initialized;

        internal static List<GameObject> networkPrefabs = new List<GameObject>();
        public static GameObject CompanyCruiserPrefab { get; internal set; } = null!;
        public static GameObject CompanyCruiserManualPrefab { get; internal set; } = null!;

        public void Awake()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;
            Logger = base.Logger;
            Instance = this;

            AssetBundle CompanyCruiserBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "v55cruiser"));
            if (CompanyCruiserBundle == null)
            {
                Logger.LogError("[AssetBundle] Failed to load asset bundle: v55cruiser");
                return;
            }

            CompanyCruiserPrefab = CompanyCruiserBundle.LoadAsset<GameObject>("CompanyCruiser.prefab");
            CompanyCruiserManualPrefab = CompanyCruiserBundle.LoadAsset<GameObject>("CompanyCruiserManual.prefab");
            if (CompanyCruiserPrefab != null)
            {
                if (!networkPrefabs.Contains(CompanyCruiserPrefab))
                    networkPrefabs.Add(CompanyCruiserPrefab);
                Logger.LogInfo("[AssetBundle] Successfully loaded prefab: CompanyCruiser");
            }
            else
            {
                Logger.LogError("[AssetBundle] Failed to load prefab: CompanyCruiser");
            }

            if (CompanyCruiserManualPrefab != null)
            {
                if (!networkPrefabs.Contains(CompanyCruiserManualPrefab))
                    networkPrefabs.Add(CompanyCruiserManualPrefab);
                Logger.LogInfo("[AssetBundle] Successfully loaded prefab: CompanyCruiserManual");
            }
            else
            {
                Logger.LogError("[AssetBundle] Failed to load prefab: CompanyCruiserManual");
            }

            AssetBundle PlayerAnimationBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "playeranimationbundles"));
            if (PlayerAnimationBundle == null)
            {
                Logger.LogError("[AssetBundle] Failed to load asset bundle: playeranimationbundles");
                return;
            }

            References.truckPlayerAnimator = PlayerAnimationBundle.LoadAsset<RuntimeAnimatorController>("truckPlayerMetarig.controller");
            if (References.truckPlayerAnimator != null)
            {
                Logger.LogInfo("[AssetBundle] Successfully loaded runtime controller: truckPlayerMetarig");
            }
            else
            {
                Logger.LogError("[AssetBundle] Failed to load runtime controller: truckPlayerMetarig");
            }

            VehicleControlsInstance = new VehicleControls();
            UserConfig.InitConfig();

            NetcodePatcher();
            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            // todo: look into a more modular system for this
            if (CompatibilityUtils.IsModPresent("voxx.LethalElementsPlugin"))
            {
                LethalElementsCompatibility.PatchAllElements(Harmony);
            }

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }

        private void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}