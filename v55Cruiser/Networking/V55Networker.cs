using GameNetcodeStuff;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using v55Cruiser.Patches;

namespace v55Cruiser.Networking;

/// <summary>
///  Available from BRBNetworker, licensed under GNU General Public License.
///  Source: https://github.com/ButteryStancakes/ButteRyBalance/blob/master/Network/BRBNetworker.cs
/// </summary>
internal class V55Networker : NetworkBehaviour
{
    // --- INIT ---

    internal static GameObject networkPrefab = null!;
    internal static V55Networker? Instance { get; private set; }

    internal static void Init()
    {
        if (networkPrefab != null)
        {
            Plugin.Logger.LogDebug("Skipped network handler registration, because it has already been initialized");
            return;
        }
        try
        {
            // create "prefab" to hold our network references
            networkPrefab = new(nameof(V55Networker))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            // assign a unique hash so it can be network registered
            NetworkObject netObj = networkPrefab.AddComponent<NetworkObject>();
            byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(typeof(V55Networker).Assembly.GetName().Name + networkPrefab.name));
            netObj.GlobalObjectIdHash = System.BitConverter.ToUInt32(hash, 0);

            // and now it holds our network handler!
            networkPrefab.AddComponent<V55Networker>();

            // register it, and then it can be spawned
            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(networkPrefab);

            Plugin.Logger.LogDebug("Successfully registered network handler. This is good news!");
            return;
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Encountered some fatal error while registering network handler. The mod will not function like this!\n{e}");
        }
    }

    internal static void Create()
    {
        try
        {
            if (NetworkManager.Singleton.IsServer && networkPrefab != null)
                Instantiate(networkPrefab).GetComponent<NetworkObject>().Spawn(true);
        }
        catch
        {
            Plugin.Logger.LogError($"Encountered some fatal error while spawning network handler. It is likely that registration failed earlier on start-up, please consult your logs.");
        }
    }

    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (Instance != this)
        {
            if (Instance != null && Instance.TryGetComponent(out NetworkObject netObj) && !netObj.IsSpawned && Instance != networkPrefab)
                Destroy(Instance);

            Plugin.Logger.LogWarning($"There are 2 {nameof(V55Networker)}s instantiated, and the wrong one was assigned as Instance. This shouldn't happen, but is recoverable");

            Instance = this;
        }
        Plugin.Logger.LogDebug("Successfully spawned network handler.");
    }

    // --- NETWORKING ---

    void Start()
    {
        if (this != Instance || !IsSpawned)
            return;
        Plugin.Logger.LogDebug($"V55: Start on 'V55Networker' called!");
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    internal void SyncPlayerZoneRpc(NetworkObjectReference player, bool setSeated, bool setStorage, bool setRiding)
    {
        if (player.TryGet(out NetworkObject netObj))
        {
            if (!netObj.TryGetComponent<PlayerControllerB>(out var playerObj))
            {
                Plugin.Logger.LogError("V55: Failed to find player network object!");
                return;
            }
            var playerObjData = PlayerControllerBPatches.playerData[playerObj];
            if (playerObjData == null)
            {
                Plugin.Logger.LogError($"V55: Failed to find player data. clientId? {playerObj.playerClientId}");
                return;
            }
            //Plugin.Logger.LogDebug($"V55: Setting zones for player {playerObj.playerClientId} with params: seated? {setSeated}, storage? {setStorage}, riding? {setRiding}");
            playerObjData.playerSeatedInTruck = setSeated;
            playerObjData.playerRidingInTruckStorage = setStorage;
            playerObjData.playerRidingOnTruck = setRiding;
        }
        else
            Plugin.Logger.LogError($"V55: Failed to set player zone data.");
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    internal void SyncPlayerLookInputRpc(NetworkObjectReference player, float lookInput)
    {
        if (player.TryGet(out NetworkObject netObj))
        {
            if (!netObj.TryGetComponent<PlayerControllerB>(out var playerObj))
            {
                Plugin.Logger.LogError("V55: Failed to find player network object!");
                return;
            }
            var playerObjData = PlayerControllerBPatches.playerData[playerObj];
            if (playerObjData == null)
            {
                Plugin.Logger.LogError($"V55: Failed to find player data. clientId? {playerObj.playerClientId}");
                return;
            }
            //Plugin.Logger.LogDebug($"V55: Setting look-input for player {playerObj.playerClientId} with params: float? {lookInput}");
            playerObjData.syncedCameraHorizontal = lookInput;
        }
        else
            Plugin.Logger.LogError($"V55: Failed to set player look-input");
    }
}