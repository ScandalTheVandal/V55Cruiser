using GameNetcodeStuff;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace v55Cruiser.Utils;
public static class VehicleUtils
{
    internal static v55VehicleController truckController = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // internal is like public, but only we can use this!
    internal static bool IsPlayerLocal(PlayerControllerB? player)
    {
        return player != null && player == GameNetworkManager.Instance.localPlayerController;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPlayerNearTruck(PlayerControllerB? playerScript, v55VehicleController? truckScript)
    {
        return playerScript != null && truckScript != null && Vector3.Distance(playerScript.transform.position, truckScript.mainRigidbody.position) <= 10f;
    }
}