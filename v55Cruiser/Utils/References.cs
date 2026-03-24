using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Audio;

namespace v55Cruiser.Utils;

public static class References
{
    // optimisation
    internal static ItemDropship itemShip = null!;
    internal static v55VehicleController truckController = null!;

    // fixes
    internal static PlayerControllerB lastDriver = null!;

    // custom animations
    internal static RuntimeAnimatorController truckPlayerAnimator = null!;
}
