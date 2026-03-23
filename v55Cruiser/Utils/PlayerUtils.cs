using GameNetcodeStuff;
using UnityEngine;
using v55Cruiser.Patches;

namespace v55Cruiser.Utils;
public static class PlayerUtils
{
    public static bool seatedInTruck = false;
    public static bool disableAnimationSync;

    public static bool isPlayerOnTruck;
    public static bool isPlayerInStorage;

    public static Animator playerAnimator = null!;
    public static Animator driverPlayerAnimator = null!;
    public static Animator passengerPlayerAnimator = null!;

    public static RuntimeAnimatorController localDriverCachedAnimatorController = null!;
    public static RuntimeAnimatorController localPassengerCachedAnimatorController = null!;

    public static RuntimeAnimatorController driverCachedAnimatorController = null!;
    public static RuntimeAnimatorController passengerCachedAnimatorController = null!;

    private static float[] storedParameters = new float[0];
    private static bool[] storedBools = new bool[0];
    private static int[] storedInts = new int[0];
    private static AnimationInfo[] storedAnimations = new AnimationInfo[0];

    private struct AnimationInfo
    {
        public string stateName;
        public float normalizedTime;
    }

    public static void ResetHUDToolTips(PlayerControllerB player)
    {
        if (player == null ||
            player.isPlayerDead ||
            !player.isPlayerControlled)
            return;

        if (player.currentlyHeldObjectServer != null)
        {
            player.currentlyHeldObjectServer.SetControlTipsForItem();
            return;
        }
        HUDManager.Instance.ClearControlTips();
    }

    public static void ReplaceClientPlayerAnimator(int playerId, InteractTrigger seatTrigger, bool isPassenger)
    {
        // find the player
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];

        // safeguarding
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled)
            return;

        ResetPlayerData(playerController);
        // save a reference of the players current animator
        if (!isPassenger)
        {
            driverCachedAnimatorController = null!;
            driverCachedAnimatorController = GameObject.Instantiate(playerController.playerBodyAnimator.runtimeAnimatorController);
            driverCachedAnimatorController.name = "metarigOtherPlayers";
            driverPlayerAnimator = playerController.playerBodyAnimator;
        }
        else
        {
            passengerCachedAnimatorController = null!;
            passengerCachedAnimatorController = GameObject.Instantiate(playerController.playerBodyAnimator.runtimeAnimatorController);
            passengerCachedAnimatorController.name = "metarigOtherPlayers";
            passengerPlayerAnimator = playerController.playerBodyAnimator;
        }

        if (References.truckPlayerAnimator != null)
            playerController.playerBodyAnimator.runtimeAnimatorController = References.truckPlayerAnimator;

        playerController.playerBodyAnimator.ResetTrigger("SA_stopAnimation");
        playerController.playerBodyAnimator.ResetTrigger(seatTrigger.animationString);
        playerController.playerBodyAnimator.SetTrigger(seatTrigger.animationString);
    }

    /// <summary>
    ///  Available from LethalMin, licensed under MIT License.
    ///  Source: https://github.com/NoteBoxz/LethalMin/blob/main/Scripts/CustomPlayerAnimationManager.cs
    /// </summary>
    public static void StoreParameters()
    {
        var parameters = playerAnimator.parameters;
        storedParameters = new float[parameters.Length];
        storedBools = new bool[parameters.Length];
        storedInts = new int[parameters.Length];
        storedAnimations = new AnimationInfo[playerAnimator.layerCount];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    storedParameters[i] = playerAnimator.GetFloat(param.name);
                    break;
                case AnimatorControllerParameterType.Bool:
                    storedBools[i] = playerAnimator.GetBool(param.name);
                    break;
                case AnimatorControllerParameterType.Int:
                    storedInts[i] = playerAnimator.GetInteger(param.name);
                    break;
            }
        }

        // store current animations for each layer
        for (int layer = 0; layer < playerAnimator.layerCount; layer++)
        {
            var stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(layer);
            var animInfo = new AnimationInfo
            {
                stateName = stateInfo.fullPathHash.ToString(),
                normalizedTime = stateInfo.normalizedTime
            };
            storedAnimations[layer] = animInfo;
        }
    }

    public static void ReturnClientPlayerAnimator(int playerId, InteractTrigger seatTrigger, bool isPassenger)
    {
        // find the player
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];

        // safeguarding
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled)
        {
            // clear old references
            if (!isPassenger)
            {
                driverCachedAnimatorController = null!;
                driverPlayerAnimator = null!;
            }
            else
            {
                passengerCachedAnimatorController = null!;
                passengerPlayerAnimator = null!;
            }
            return;
        }

        // reapply the original players animator, if it exists (which it should, and would be weird if it didn't)
        if (!isPassenger)
        {
            playerController.playerBodyAnimator.runtimeAnimatorController =
                driverCachedAnimatorController ?? StartOfRound.Instance.otherClientsAnimatorController;

            // clear old references
            driverCachedAnimatorController = null!;
            driverPlayerAnimator = null!;
        }
        else
        {
            playerController.playerBodyAnimator.runtimeAnimatorController =
                passengerCachedAnimatorController ?? StartOfRound.Instance.otherClientsAnimatorController;

            // clear old references
            passengerCachedAnimatorController = null!;
            passengerPlayerAnimator = null!;
        }
        ResetPlayerData(playerController);
    }

    /// <summary>
    ///  Available from LethalMin, licensed under MIT License.
    ///  Source: https://github.com/NoteBoxz/LethalMin/blob/main/Scripts/CustomPlayerAnimationManager.cs
    /// </summary>
    public static void RestoreParameters()
    {
        var parameters = playerAnimator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    playerAnimator.SetFloat(param.name, storedParameters[i]);
                    break;
                case AnimatorControllerParameterType.Bool:
                    playerAnimator.SetBool(param.name, storedBools[i]);
                    break;
                case AnimatorControllerParameterType.Int:
                    playerAnimator.SetInteger(param.name, storedInts[i]);
                    break;
            }
        }

        // restore animations for each layer
        for (int layer = 0; layer < playerAnimator.layerCount; layer++)
        {
            var animInfo = storedAnimations[layer];
            playerAnimator.Play(int.Parse(animInfo.stateName), layer, animInfo.normalizedTime);
        }
    }

    public static void ResetPlayerData(PlayerControllerB player)
    {
        var data = PlayerControllerBPatches.GetData(player);
        data.currentCarAnimation = -1;
        player.ladderCameraHorizontal = 0f;
        data.vehicleCameraHorizontal = 0f;
        data.lastVehicleCameraHorizontal = 0f;
    }
}