using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using v55Cruiser;
using v55Cruiser.Patches;
using v55Cruiser.Scripts;
using v55Cruiser.Utils;

public class v55VehicleController : VehicleController
{
    [Header("Gimmicks/Variety")]

    //public TruckVersionType truckType;

    public v55InteriorType LHDInterior = null!;
    public v55InteriorType RHDInterior = null!;
    public v55InteriorType currentInterior = null!;

    public Transform v55HealthMeter = null!;
    public Transform v56HealthMeter = null!;

    public GameObject v55EngineBay = null!;
    public GameObject v56EngineBay = null!;

    public int interiorType;
    public bool isInteriorRHD;

    [Header("Pre-V55 Beta")]

    public MeshFilter mainBodyAltMeshFilter = null!;
    public MeshFilter mainBodyAltLODMeshFilter = null!;

    public MeshFilter mainBodyMeshFilter = null!;
    public MeshFilter mainBodyLOD1MeshFilter = null!;
    public MeshFilter mainBodyLOD2MeshFilter = null!;

    [Header("Physics")]

    public List<WheelCollider> wheels = null!;
    public v55VehicleCollisionTrigger collisionTrigger = null!;
    public Rigidbody playerPhysicsBody = null!;

    public WheelCollider backRightWheel = null!;
    public WheelCollider centerRightWheel = null!;
    public WheelCollider backLeftWheel = null!;
    public WheelCollider frontRightWheel = null!;
    public WheelCollider frontLeftWheel = null!;
    public WheelCollider centerFrontWheel = null!;

    public AnimationCurve steeringCurve = null!;
    public bool useSteeringCurve;

    private WheelHit[] wheelHits = new WheelHit[4];
    public Vector3 previousVehiclePosition;
    public Quaternion previousVehicleRotation;

    public bool inBetaMode;

    public float tyreSteeringAngle;
    public float steeringAngle;

    public float currentMotorTorque;
    public float currentBrakeTorque = 2000f;
    public float maxBrakingTorque = 2000f;

    public float forwardWheelSpeed;
    public float reverseWheelSpeed;

    public float wheelsRPM;

    public float wheelRPM;
    public float frontWheelRPM;
    public float backWheelRPM;

    public bool backWheelsGrounded;
    public bool allWheelsAirborne;

    public float forwardsSlip;
    public float sidewaysSlip;

    public bool hasDeliveredVehicle;

    [Header("Networking/Player")]

    private bool receivedSyncData;

    public v55PhysicsRegion vehicleZone = null!;
    public v55PlayerZone vehicleStorageZone = null!;

    public Collider vehicleBounds = null!;
    public Collider storageCompartment = null!;

    public PlayerControllerB lastDriver = null!;
    public PlayerControllerB playerWhoShifted = null!;

    public Vector3 playerPositionOffset;
    public Vector3 seatNodePositionOffset;

    public float syncedPlayerSteeringAnim;
    public float syncedWheelRotation;
    public float syncedSteeringInput;

    public float syncedFrontWheelRPM;
    public float syncedBackWheelRPM;
    public float syncedWheelRPM;

    public float syncedEngineRPM;

    public float syncedMotorTorque;
    public float syncedBrakeTorque;

    public int syncedCarHP;

    public bool syncedDrivePedalPressed;
    public bool syncedBrakePedalPressed;

    public bool forwardSlipping;
    public bool sidewaySlipping;
    public bool psuedoSlipping;

    public float tyreStress;
    public bool wheelSlipping;

    public float syncCarEffectsInterval;
    public float syncWheelTorqueInterval;
    public float syncCarDrivetrainInterval;

    public bool canDestroyTrees;

    private float syncedSongTime;

    [Header("VFX")]

    public InteractTrigger pushTruckTrigger = null!;
    public Collider[] weatherEffectBlockers = null!;

    public ParticleSystem frontTireSparks = null!;

    public AnimationCurve engineAudio1Curve = null!;
    public AnimationCurve engineAudio2Curve = null!;

    public GameObject destroyedTruckMeshAlt = null!;
    public MeshRenderer windshieldMesh = null!;
    public GameObject windshieldObject = null!;
    public GameObject carKeyInHand = null!;

    public MeshRenderer radarMapIcon = null!;
    public MeshRenderer radarMapDestroyedIcon = null!;

    public InteractTrigger startIgnitionTrigger = null!;
    public InteractTrigger stopIgnitionTrigger = null!;

    public Animator ignitionAnimator = null!;
    public GameObject carKeyContainer = null!;

    public Animator verticalColumnAnimator = null!;
    public Animator ejectorButtonAnimator = null!;

    public GameObject ejectorButtonContainer = null!;

    // ignition key stuff
    public Transform ignitionKeyPosition = null!;
    private Transform leftHandServerItemTarget = null!;

    public MeshRenderer frontLeftDoorMeshLOD = null!;
    public MeshRenderer frontRightDoorMeshLOD = null!;
    public MeshRenderer frontLeftDoorMesh = null!;
    public MeshRenderer frontRightDoorMesh = null!;
    public MeshRenderer steeringWheelMesh = null!;

    public Material destroyedTruckMaterial = null!;
    public Material greyLightOffMat = null!;
    public Material redLightOffMat = null!;

    public Transform tempPushTransform = null!;

    public Light leftHeadlight = null!;
    public Light rightHeadlight = null!;

    private Vector3 ignitionKeyScale = Vector3.one;

    private Vector3 LHD_Pos_Local = new Vector3(0.0489f, 0.1371f, -0.1566f);
    private Vector3 LHD_Pos_Server = new Vector3(0.0366f, 0.1023f, -0.1088f);
    private Vector3 LHD_Rot_Local = new Vector3(-3.446f, 3.193f, 172.642f);
    private Vector3 LHD_Rot_Server = new Vector3(-191.643f, 174.051f, -7.768005f);

    private Vector3 RHD_Pos_Local = new Vector3(-0.02776055f, 0.1709576f, -0.1114562f);
    private Vector3 RHD_Pos_Server = new Vector3(-0.02314448f, 0.1360526f, -0.108739f);
    private Vector3 RHD_Rot_Local = new Vector3(22.679f, -8.263f, -158.794f);
    private Vector3 RHD_Rot_Server = new Vector3(9.026f, -18.147f, -162.389f);
    // ignition end

    public bool cabinLightOn;
    public bool liftGateOpen;

    public bool disableAnimations;
    public bool inIgnitionAnimation;

    public float playerSteeringWheelAnimFloat;
    public float ignitionRotSpeed = 45f;


    // animations
    private string STEERING_WHEEL_SPEED = "steeringWheelTurnSpeed";
    private string ANIMATION_SPEED = "animationSpeed";
    private string IGNITION_ANIM = "SAIgnition_Anim";
    private string CAR_ANIM = "SA_CarAnim";
    private string JUMP_WHILE_IN_CAR = "SA_JumpInCar";
    private string CAR_MOTION_TIME = "SA_CarMotionTime";

    // triggers
    private readonly string DOOR_ENTER_HOVERTIP = "Use door : [LMB]";
    private readonly string DOOR_EXIT_HOVERTIP = "Exit : [LMB]";

    // player
    private readonly string PLAYER_MOVEMENT = "Move";

    [Header("Destruction")]

    public GameObject[] disableOnDestroy = null!;

    public GameObject mainBodyContainer = null!;
    public GameObject hoodDoorContainer = null!;

    [Header("Audio")]

    public AudioSource carKeyAudio = null!;
    public AudioSource ejectorButtonAudio = null!;
    public AudioSource reverseWhineAudio = null!;
    public AudioSource verticalColumnAudio = null!;

    public AudioClip dashboardButton = null!;
    public AudioClip engineRev1 = null!;
    public AudioClip revEngineStart1 = null!;

    public bool hasAdditionalMusic;
    public AudioClip radioBabyFace = null!;
    public AudioClip radio1 = null!;

    public float timeLastSyncedRadio;
    public float radioPingTimestamp;


    // --- INIT ---
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!NetworkManager.IsServer || StartOfRound.Instance.inShipPhase)
            return;

        int interiorVariant = 0;
        bool postBetaMode = UserConfig.PostBeta.Value;
        bool noTree = UserConfig.NoTreeDestruction.Value;
        bool addExtraMusic = UserConfig.AdditionalRadioMusic.Value;
        bool useAltSteering = UserConfig.AlternateSteering.Value && !postBetaMode;
        if (!postBetaMode)
        {
            if (UserConfig.RightHandedWheel.Value)
            {
                interiorVariant = new System.Random(StartOfRound.Instance.randomMapSeed).Next(0, 2);
            }
            noTree = false;
        }    
        SyncClientDataRpc(interiorVariant, postBetaMode, noTree, addExtraMusic, useAltSteering);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SyncClientDataRpc(int interiorType, bool postBetaMode, bool noTree, bool addExtraMusic, bool useAltSteering)
    {
        if (receivedSyncData)
        {
            return;
        }
        receivedSyncData = true;
        useSteeringCurve = useAltSteering;
        SetInteriorType(interiorType);
        SetPostBetaMode(postBetaMode);
        canDestroyTrees = !noTree;
        hasAdditionalMusic = addExtraMusic;
        AddExtraRadioTracks(addExtraMusic);
        hasBeenSpawned = true;
    }

    public void SetInteriorType(int interiorType)
    {
        switch (interiorType)
        {
            case 0:
                currentInterior = LHDInterior;
                isInteriorRHD = false;
                RHDInterior.gameObject.SetActive(false);

                ejectorButtonAudio.transform.localPosition = new Vector3(-0.325f, ejectorButtonAudio.transform.localPosition.y, ejectorButtonAudio.transform.localPosition.z);
                steeringWheelAudio.transform.localPosition = new Vector3(-0.956f, steeringWheelAudio.transform.localPosition.y, steeringWheelAudio.transform.localPosition.z);
                carKeyAudio.transform.localPosition = new Vector3(-0.5525f, carKeyAudio.transform.localPosition.y, carKeyAudio.transform.localPosition.z);
                verticalColumnAudio.transform.localPosition = new Vector3(-1.4215f, verticalColumnAudio.transform.localPosition.y, verticalColumnAudio.transform.localPosition.z);
                springAudio.transform.localPosition = new Vector3(-1f, springAudio.transform.localPosition.y, springAudio.transform.localPosition.z);
                ignitionAnimator.transform.localPosition = new Vector3(-0.5525247f, ignitionAnimator.transform.localPosition.y, ignitionAnimator.transform.localPosition.z);
                steeringWheelAnimator.transform.localPosition = new Vector3(-0.97943f, 0.18512f, 2.30265f);
                verticalColumnAnimator.transform.localPosition = new Vector3(-1.42f, 0.095f, 2.55f);
                ejectorButtonContainer.transform.localPosition = new Vector3(-0.3385f, -0.02501607f, 2.508915f);
                break;
            case 1:
                currentInterior = RHDInterior;
                isInteriorRHD = true;
                LHDInterior.gameObject.SetActive(false);

                ejectorButtonAudio.transform.localPosition = new Vector3(0.325f, ejectorButtonAudio.transform.localPosition.y, ejectorButtonAudio.transform.localPosition.z);
                steeringWheelAudio.transform.localPosition = new Vector3(0.956f, steeringWheelAudio.transform.localPosition.y, steeringWheelAudio.transform.localPosition.z);
                carKeyAudio.transform.localPosition = new Vector3(0.5525f, carKeyAudio.transform.localPosition.y, carKeyAudio.transform.localPosition.z);
                verticalColumnAudio.transform.localPosition = new Vector3(1.4215f, verticalColumnAudio.transform.localPosition.y, verticalColumnAudio.transform.localPosition.z);
                springAudio.transform.localPosition = new Vector3(1f, springAudio.transform.localPosition.y, springAudio.transform.localPosition.z);
                ignitionAnimator.transform.localPosition = new Vector3(0.5525247f, ignitionAnimator.transform.localPosition.y, ignitionAnimator.transform.localPosition.z);
                steeringWheelAnimator.transform.localPosition = new Vector3(0.97943f, 0.18512f, 2.30265f);
                verticalColumnAnimator.transform.localPosition = new Vector3(1.42f, 0.095f, 2.55f);
                ejectorButtonContainer.transform.localPosition = new Vector3(0.3385f, -0.02501607f, 2.508915f);
                break;
        }

        v55InteriorType iType = currentInterior;
        driverSeatTrigger = iType.driverSeatTrigger;
        passengerSeatTrigger = iType.passengerSeatTrigger;
        driverSeatSpringAnimator = iType.driverSeatSpringAnimator;
        gearStickAnimator = iType.gearStickAnimator;
    }

    public void SetPostBetaMode(bool postBetaMode)
    {
        inBetaMode = postBetaMode;

        JointSpring suspensionSpring;
        if (inBetaMode)
        {
            suspensionSpring = new JointSpring
            {
                spring = 2700f,
                damper = 500f,
                targetPosition = 0.5f,
            };

            FrontLeftWheel.suspensionSpring = suspensionSpring;
            FrontRightWheel.suspensionSpring = suspensionSpring;
            BackRightWheel.suspensionSpring = suspensionSpring;
            BackLeftWheel.suspensionSpring = suspensionSpring;

            FrontLeftWheel.sprungMass = 8.976847f;
            FrontRightWheel.sprungMass = 8.976847f;
            BackLeftWheel.sprungMass = 33.38108f;
            BackRightWheel.sprungMass = 33.38108f;
            return;
        }

        suspensionSpring = new JointSpring
        {
            spring = 6200f,
            damper = 500f,
            targetPosition = 0.8f,
        };

        FrontLeftWheel.suspensionSpring = suspensionSpring;
        FrontRightWheel.suspensionSpring = suspensionSpring;
        BackRightWheel.suspensionSpring = suspensionSpring;
        BackLeftWheel.suspensionSpring = suspensionSpring;

        FrontLeftWheel.sprungMass = 15f;
        FrontRightWheel.sprungMass = 15f;
        BackLeftWheel.sprungMass = 55f;
        BackRightWheel.sprungMass = 55f;

        SetHeadlightMaterial(on: headlightsContainer.activeSelf);
        SetHeadlightShadows(setOn: true);
    }

    public void SetHeadlightShadows(bool setOn = false)
    {
        leftHeadlight.shadows = setOn ? LightShadows.Soft : LightShadows.None;
        rightHeadlight.shadows = setOn ? LightShadows.Soft : LightShadows.None;
    }

    public void AddExtraRadioTracks(bool addExtraMusic)
    {
        if (!addExtraMusic)
        {
            return;
        }
        /*
        radioClips = radioClips.AddToArray<AudioClip>(radioBabyFace);
        radioClips = radioClips.AddToArray<AudioClip>(radio1);
        */
        AudioClip[] array = new AudioClip[radioClips.Length + 2];
        array[0] = radioClips[0];
        array[1] = radio1;
        array[2] = radioClips[1];
        array[3] = radioClips[2];
        array[4] = radioBabyFace;
        array[5] = radioClips[3];
        radioClips = array;
    }


    public void OnEnable()
    {
        VehicleUtils.truckController = this;
    }


    public new void Awake()
    {
        if (itemShip == null && ScandalsTweaks.Utils.References.itemShip != null)
            itemShip = ScandalsTweaks.Utils.References.itemShip;

        ragdollPhysicsBody.interpolation = RigidbodyInterpolation.Interpolate;
        windwiperPhysicsBody1.interpolation = RigidbodyInterpolation.Interpolate;
        windwiperPhysicsBody2.interpolation = RigidbodyInterpolation.Interpolate;
        playerPhysicsBody.interpolation = RigidbodyInterpolation.None;
        playerPhysicsBody.freezeRotation = true;
        backDoorOpen = true; // hacky shit
        base.Awake();

        physicsRegion.priority = 1;
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;

        seatNodePositionOffset = Vector3.zero;
        playerPositionOffset = Vector3.zero;
        InitializeTruck();
    }


    private void InitializeTruck()
    {
        // physics
        gear = CarGearShift.Park;
        MaxEngineRPM = 300f;
        MinEngineRPM = 100f;
        engineIntensityPercentage = 180f;
        EngineTorque = 1100f;
        carAcceleration = 250f;
        idleSpeed = 15f;

        forwardWheelSpeed = 10000f;
        reverseWheelSpeed = -10000f;

        backRightWheel.sprungMass = 32.32506f;
        centerRightWheel.sprungMass = 21.03854f;
        backLeftWheel.sprungMass = 30.04548f;
        frontRightWheel.sprungMass = 9.794424f;
        frontLeftWheel.sprungMass = 18.06794f;
        centerFrontWheel.sprungMass = 4.012677f;

        mainRigidbody.drag = 0f;
        mainRigidbody.angularDrag = 0.5f;

        mainRigidbody.automaticCenterOfMass = false;
        mainRigidbody.centerOfMass = new Vector3(0f, -0.5534029f, -0.9468046f);
        mainRigidbody.automaticInertiaTensor = false;

        carMaxSpeed = 60f;
        mainRigidbody.maxLinearVelocity = carMaxSpeed;
        mainRigidbody.maxAngularVelocity = 4f;

        brakeSpeed = 2000f;
        currentBrakeTorque = maxBrakingTorque;
        pushForceMultiplier = 27f;
        pushVerticalOffsetAmount = 1f;
        steeringWheelTurnSpeed = 4f;
        torqueForce = 2.5f;

        SetWheelFriction();

        FrontLeftWheel.brakeTorque = maxBrakingTorque;
        FrontRightWheel.brakeTorque = maxBrakingTorque;
        BackLeftWheel.brakeTorque = maxBrakingTorque;
        BackRightWheel.brakeTorque = maxBrakingTorque;

        // boost ability
        turboBoostForce = 3000f;
        turboBoostUpwardForce = 7200f;
        jumpForce = 600f;

        // health
        baseCarHP = 30;
        carHP = baseCarHP;
        syncedCarHP = carHP;
        carFragility = 1f;

        // misc
        positionOffset = new Vector3(0.0472f, 0.103f, -0.09f);
        rotationOffset = Vector3.zero;

        // unfinished
        //truckType = TruckVersionType.V55;

        v55EngineBay.SetActive(true);
        v56EngineBay.SetActive(false);

        v55HealthMeter.gameObject.SetActive(true);
        v56HealthMeter.gameObject.SetActive(false);
        turboMeter.gameObject.SetActive(false);

        if (v55HealthMeter.gameObject.activeSelf)
        {
            healthMeter = v55HealthMeter;
        }
        else
        {
            healthMeter = v56HealthMeter;
        }
    }

    private new void SetWheelFriction()
    {
        WheelFrictionCurve wheelFrictionCurve = default(WheelFrictionCurve);

        wheelFrictionCurve.extremumSlip = 0.2f;
        wheelFrictionCurve.extremumValue = 1f;
        wheelFrictionCurve.asymptoteSlip = 0.8f;
        wheelFrictionCurve.asymptoteValue = 0.4f;
        wheelFrictionCurve.stiffness = 2.7f;
        FrontLeftWheel.forwardFriction = wheelFrictionCurve;
        FrontRightWheel.forwardFriction = wheelFrictionCurve;

        wheelFrictionCurve.stiffness = 0.75f;
        BackLeftWheel.forwardFriction = wheelFrictionCurve;
        BackRightWheel.forwardFriction = wheelFrictionCurve;

        wheelFrictionCurve.stiffness = 0.8f;
        wheelFrictionCurve.asymptoteValue = 0.75f;
        wheelFrictionCurve.extremumSlip = 0.7f;
        FrontLeftWheel.sidewaysFriction = wheelFrictionCurve;
        FrontRightWheel.sidewaysFriction = wheelFrictionCurve;
        BackLeftWheel.sidewaysFriction = wheelFrictionCurve;
        BackRightWheel.sidewaysFriction = wheelFrictionCurve;
    }

    public new void Start()
    {
        StartCoroutine(SetRainCollision());

        currentRadioClip = new System.Random(StartOfRound.Instance.randomMapSeed).Next(0, radioClips.Length);
        radioAudio.clip = radioClips[currentRadioClip];
        decals = new DecalProjector[24];

        if (StartOfRound.Instance.inShipPhase)
        {
            magnetedToShip = true;
            loadedVehicleFromSave = true;
            hasDeliveredVehicle = true;
            inDropshipAnimation = false;
            hasBeenSpawned = true;
            SetVehicleKinematic(setKinematic: true);
            transform.position = StartOfRound.Instance.magnetPoint.position + StartOfRound.Instance.magnetPoint.forward * 7f;
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            StartMagneting();
        }
    }

    public IEnumerator SetRainCollision()
    {
        yield return new WaitForSeconds(4f);

        var particleTriggers = new[]
        {
            ScandalsTweaks.Utils.References.rainParticles,
            ScandalsTweaks.Utils.References.rainHitParticles,
            ScandalsTweaks.Utils.References.stormyRainParticles,
            ScandalsTweaks.Utils.References.stormyRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneSandParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainHitParticles
        };

        for (int i = 0; i < particleTriggers.Length; i++)
        {
            if (particleTriggers[i] == null)
            {
                Plugin.LogDebug("Weather particle (or trigger) is null!");
                continue;
            }

            var trigger = particleTriggers[i]!.trigger;
            for (int j = 0; j < weatherEffectBlockers.Length; j++)
            {
                int index = trigger.colliderCount + j;
                trigger.SetCollider(index, weatherEffectBlockers[j]);
            }
        }
        yield break;
    }


    // --- SYNC DATA ---
    public void SendClientSyncData()
    {
        if (magnetedToShip)
        {
            Vector3 eulerAngles = magnetTargetRotation.eulerAngles;
            MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, RoundManager.Instance.tempTransform.eulerAngles, averageVelocityAtMagnetStart);
        }

        if (interiorType == -1)
            interiorType = 0;

        SyncClientDataRpc(interiorType, inBetaMode, !canDestroyTrees, hasAdditionalMusic, useSteeringCurve);
    }


    // --- STORAGE DOOR ---
    public new void SetBackDoorOpen(bool open)
    {
        liftGateOpen = open;
    }


    // --- CAB LIGHTING ---
    public new void SetFrontCabinLightOn(bool setOn)
    {
        cabinLightOn = setOn;
        frontCabinLightContainer.SetActive(setOn);
        frontCabinLightMesh.material = setOn ? headlightsOnMat : headlightsOffMat;
    }


    // --- TRY IGNITION METHOD ---
    public new void StartTryCarIgnition()
    {
        if (!localPlayerInControl ||
            ignitionStarted ||
            inIgnitionAnimation ||
            (inIgnitionAnimation && startIgnitionTrigger.isBeingHeldByPlayer))
            return;

        CancelIgnitionCoroutine();
        disableAnimations = true;
        inIgnitionAnimation = true;
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: true));
        TryIgnitionRpc(keyIsInIgnition, cabinLightOn);
    }

    private new IEnumerator TryIgnition(bool isLocalDriver)
    {
        if (currentDriver == null)
        {
            keyIgnitionCoroutine = null;
            yield break;   
        }
        if (keyIsInIgnition)
        {
            SetKeyIgnitionValues(keyInHand: false, keyInSlot: true);
            currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, 12);
            ignitionAnimator.SetInteger(IGNITION_ANIM, 12);
            if (inBetaMode) yield return new WaitForSeconds(0.035f);
            else yield return new WaitForSeconds(0.02f);
            carKeyAudio.PlayOneShot(twistKey);
            SetKeyIgnitionValues(keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.1467f);
            SetKeyIgnitionValues(keyInHand: true, keyInSlot: true);
        }
        else
        {
            SetKeyIgnitionValues(keyInHand: true, keyInSlot: false);
            currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 2);
            ignitionAnimator.SetInteger(IGNITION_ANIM, 2);
            if (inBetaMode)
            {
                yield return new WaitForSeconds(0.66f);
                carKeyAudio.PlayOneShot(insertKey);
                SetKeyIgnitionValues(keyInHand: true, keyInSlot: false);
                yield return new WaitForSeconds(0.2f);
                carKeyAudio.PlayOneShot(twistKey);
                SetKeyIgnitionValues(keyInHand: true, keyInSlot: false);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                yield return new WaitForSeconds(0.6f);
                carKeyAudio.PlayOneShot(insertKey);
                SetKeyIgnitionValues(keyInHand: true, keyInSlot: true);
                yield return new WaitForSeconds(0.2f);
                carKeyAudio.PlayOneShot(twistKey);
                SetKeyIgnitionValues(keyInHand: true, keyInSlot: true);
                yield return new WaitForSeconds(0.185f);
                SetKeyIgnitionValues(keyInHand: true, keyInSlot: true);
            }
        }
        if (!isLocalDriver) yield break;
        SetKeyIgnitionValues(keyInHand: true, keyInSlot: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        PlayIgnitionAudio();
        TryStartIgnitionRpc();
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 1.1f));
        if ((float)UnityEngine.Random.Range(0, 100) < chanceToStartIgnition)
        {
            CancelIgnitionAnimation(ignitionOn: true, setIgnitionAnim: true);
            disableAnimations = false;
            inIgnitionAnimation = false;
            currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
            SetKeyIgnitionValues(keyInHand: false, keyInSlot: true);
            SetIgnition(setStarted: true, setCabinLightOn: true);
            SetFrontCabinLightOn(setOn: keyIsInIgnition);
            StartIgnitionRpc();
        }
        else
        {
            chanceToStartIgnition += 15f;
            chanceToStartIgnition = Mathf.Clamp(chanceToStartIgnition, 0f, 101f);
        }
        yield break;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void TryIgnitionRpc(bool setKeyInSlot, bool cabLightActive)
    {
        CancelIgnitionCoroutine();
        disableAnimations = true;
        inIgnitionAnimation = true;
        SetKeyIgnitionValues(keyInHand: false, keyInSlot: setKeyInSlot);
        if (!cabinLightOn && cabLightActive) SetFrontCabinLightOn(cabLightActive);
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: false));
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void TryStartIgnitionRpc()
    {
        SetKeyIgnitionValues(keyInHand: true, keyInSlot: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        PlayIgnitionAudio();
    }

    public void PlayIgnitionAudio()
    {
        if (inBetaMode)
        {
            engineAudio1.clip = revEngineStart;
            engineAudio1.volume = 0.7f;
            engineAudio1.PlayOneShot(engineRev1);
            carEngine1AudioActive = true;
            return;
        }
        engineAudio1.Stop();
        engineAudio1.clip = revEngineStart1;
        engineAudio1.volume = 0.7f;
        /*
        if (engineAudio1.clip == revEngineStart1)
            engineAudio1.PlayOneShot(engineRev);
        */
        engineAudio1.PlayOneShot(engineRev);
        carEngine1AudioActive = true;
        /*
        if (engineAudio1.clip == revEngineStart1)
            engineAudio1.pitch = 1f;
        */
    }


    // --- CANCEL IGNITION METHOD ---
    public new void CancelTryCarIgnition()
    {
        if (!localPlayerInControl ||
            ignitionStarted ||
            !inIgnitionAnimation ||
            (!inIgnitionAnimation && startIgnitionTrigger.isBeingHeldByPlayer))
            return;

        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        int playerAnimIndex = localPlayer.playerBodyAnimator.GetInteger(CAR_ANIM);
        if (playerAnimIndex == 0 && keyIsInIgnition)
            localPlayer.playerBodyAnimator.SetInteger(CAR_ANIM, 13);
        else if ((playerAnimIndex == 2 || playerAnimIndex == 12) && keyIsInIgnition)
            localPlayer.playerBodyAnimator.SetInteger(CAR_ANIM, 3);
        else
            localPlayer.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        int playerCarAnimIndex = localPlayer.playerBodyAnimator.GetInteger(CAR_ANIM);

        CancelIgnitionAnimation(ignitionOn: false, setIgnitionAnim: false);
        disableAnimations = true;
        inIgnitionAnimation = false;

        int ignitionAnimIndex = playerCarAnimIndex;
        if (playerAnimIndex == 13) ignitionAnimIndex = 3;
        ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionAnimIndex);

        CancelTryIgnitionRpc(keyIsInIgnition, cabinLightOn, playerCarAnimIndex, ignitionAnimIndex);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CancelTryIgnitionRpc(bool setKeyInSlot, bool setCabinLightOn, int playerAnimIndex, int ignitionAnimIndex)
    {
        CancelIgnitionAnimation(ignitionOn: false, setIgnitionAnim: false);
        disableAnimations = true;
        inIgnitionAnimation = false;

        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, playerAnimIndex);
        ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionAnimIndex);

        // account for netlag when the key is first inserted
        if (!inBetaMode && setKeyInSlot == true && !keyIsInIgnition)
        {
            carKeyAudio.PlayOneShot(insertKey);
        }
        SetKeyIgnitionValues(keyInHand: false, keyInSlot: setKeyInSlot);
        if (setKeyInSlot == true && cabinLightOn != setCabinLightOn)
            SetFrontCabinLightOn(setOn: setCabinLightOn);
    }


    // --- START IGNITION METHOD ---
    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void StartIgnitionRpc()
    {
        CancelIgnitionAnimation(ignitionOn: true, setIgnitionAnim: true);
        disableAnimations = false;
        inIgnitionAnimation = false;
        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
        SetKeyIgnitionValues(keyInHand: false, keyInSlot: true);
        SetIgnition(setStarted: true, setCabinLightOn: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
    }

    public void SetIgnition(bool setStarted, bool setCabinLightOn)
    {
        SetFrontCabinLightOn(setCabinLightOn);
        carEngine1AudioActive = setStarted;
        if (setStarted)
        {
            disableAnimations = false;
            inIgnitionAnimation = false;

            startKeyIgnitionTrigger.SetActive(false);
            removeKeyIgnitionTrigger.SetActive(true);

            if (setStarted == ignitionStarted)
                return;

            ignitionStarted = true;
            carExhaustParticle.Play();
            if (!inBetaMode) engineAudio1.Stop();
            engineAudio1.PlayOneShot(engineStartSuccessful);
            engineAudio1.clip = engineRun;
            return;
        }
        startKeyIgnitionTrigger.SetActive(true);
        removeKeyIgnitionTrigger.SetActive(false);
        ignitionStarted = false;
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }


    // --- REMOVE IGNITION METHOD ---
    public new void RemoveKeyFromIgnition()
    {
        if (!localPlayerInControl ||
            !ignitionStarted)
            return;

        if (inIgnitionAnimation)
            return;

        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
        RemoveKeyFromIgnitionRpc();
    }

    private new IEnumerator RemoveKey()
    {
        disableAnimations = true;
        inIgnitionAnimation = false;
        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 6);
        ignitionAnimator.SetInteger(IGNITION_ANIM, 6);
        yield return new WaitForSeconds(0.26f);
        SetKeyIgnitionValues(keyInHand: true, keyInSlot: false);
        carKeyAudio.PlayOneShot(removeKey);
        SetIgnition(setStarted: false, setCabinLightOn: false);
        yield return new WaitForSeconds(0.73f);
        SetKeyIgnitionValues(keyInHand: false, keyInSlot: false);
        keyIgnitionCoroutine = null;
        yield break;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void RemoveKeyFromIgnitionRpc()
    {
        if (!ignitionStarted)
            return;

        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
    }


    // --- MISC IGNITION STUFF ---
    public void CancelIgnitionAnimation(bool ignitionOn, bool setIgnitionAnim)
    {
        CancelIgnitionCoroutine();
        carEngine1AudioActive = ignitionOn;
        keyIsInDriverHand = false;
        if (setIgnitionAnim) ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionOn ? 1 : 0);
    }

    private void CancelIgnitionCoroutine()
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
            keyIgnitionCoroutine = null;
        }
    }

    public void SetKeyIgnitionValues(bool keyInHand, bool keyInSlot)
    {
        keyIsInDriverHand = keyInHand;
        keyIsInIgnition = keyInSlot;
    }


    // --- GENERAL REPEAT METHODS ---
    public void ResetTruckVelocityTimer()
    {
        if (averageVelocity.magnitude < 3f) limitTruckVelocityTimer = 0.7f;
    }

    public void SetTriggerHoverTip(InteractTrigger trigger, string tip)
    {
        trigger.hoverTip = tip;
    }


    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void CancelSetPlayerInVehicleRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (GameNetworkManager.Instance.localPlayerController != playerController)
            return;
        ScandalLib.Patches.InteractTriggerPatches.CancelVehicleSeatInteraction();
    }



    // --- DRIVER OCCUPANT METHODS ---
    public void SetDriverInVehicle()
    {
        SetDriverInVehicleServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    protected void SetDriverInVehicleServerRpc(int playerId, RpcParams rpcParams = default)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentDriver != null)
        {
            CancelSetPlayerInVehicleRpc(playerId);
            return;
        }
        currentDriver = playerController;
        NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        SetDriverInVehicleOwnerRpc();
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    public void SetDriverInVehicleOwnerRpc()
    {
        currentInterior.driverSeat.SetLocalPlayerIntoSeat();
        ActivateControl();
        InteractTrigger doorTrigger = isInteriorRHD ? passengerSideDoorTrigger : driverSideDoorTrigger;
        AnimatedObjectTrigger door = isInteriorRHD ? passengerSideDoor : driverSideDoor;
        SetTriggerHoverTip(doorTrigger, DOOR_EXIT_HOVERTIP);
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
        CancelIgnitionCoroutine();
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, 0.5f);
        playerSteeringWheelAnimFloat = 0.5f;
        syncedPlayerSteeringAnim = 0.5f;
        if (cabinLightOn && !keyIsInIgnition)
        {
            SetFrontCabinLightOn(setOn: false);
        }
        if (keyIsInIgnition) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        else if (ignitionStarted) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
        else GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        int playerAnimIndex = GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger(CAR_ANIM);
        if (door.boolValue) door.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        SetDriverInVehicleNotOwnerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, cabinLightOn, keyIsInIgnition, ignitionStarted, playerAnimIndex);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetDriverInVehicleNotOwnerRpc(int playerId, bool setCabinLight, bool setKeyInSlot, bool setStarted, int playerAnimIndex)
    {
        PlayerControllerB playerObj = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerObj == null)
        {
            Plugin.LogError("SetDriverInVehicleNotOwnerRpc failed to find player object reference from player client id!");
            return;
        }
        currentDriver = playerObj;
        disableAnimations = !setStarted;
        inIgnitionAnimation = false;
        currentInterior.driverSeat.SetPlayerAnimations(playerObj, false);
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
        CancelIgnitionCoroutine();
        playerObj.playerBodyAnimator.SetFloat(ANIMATION_SPEED, 0.5f);
        playerSteeringWheelAnimFloat = 0.5f;
        syncedPlayerSteeringAnim = 0.5f;
        SetIgnition(setStarted: setStarted, setCabinLightOn: setCabinLight);
        keyIsInIgnition = setKeyInSlot;
        playerObj.playerBodyAnimator.SetInteger(CAR_ANIM, playerAnimIndex);
        InteractTrigger doorTrigger = isInteriorRHD ? passengerSideDoorTrigger : driverSideDoorTrigger;
        doorTrigger.interactable = false;
    }

    public void OnDriverExitVehicle()
    {
        if (!IsSpawned ||
            NetworkManager == null ||
            !NetworkManager.IsListening)
        {
            return;
        }
        if (currentDriver != GameNetworkManager.Instance.localPlayerController)
        {
            return;
        }
        localPlayerInControl = false;
        InteractTrigger doorTrigger = isInteriorRHD ? passengerSideDoorTrigger : driverSideDoorTrigger;
        SetTriggerHoverTip(doorTrigger, DOOR_ENTER_HOVERTIP);
        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        DisableControl();
        CancelIgnitionAnimation(ignitionOn: ignitionStarted, setIgnitionAnim: true);
        SetIgnition(setStarted: ignitionStarted, setCabinLightOn: cabinLightOn);
        chanceToStartIgnition = 20f;
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
        OnDriverExitVehicleServerRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            syncedPosition,
            syncedRotation,
            drivePedalPressed,
            brakePedalPressed,
            keyIsInIgnition,
            ignitionStarted,
            cabinLightOn);
    }

    [Rpc(SendTo.Server)]
    public void OnDriverExitVehicleServerRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setGasPedal, bool setBrakePedal, bool setKeyInSlot, bool setStarted, bool setCabinLight)
    {
        OnDriverExitVehicleRpc(playerId, carLocation, carRotation, setGasPedal, setBrakePedal, setKeyInSlot, setStarted, setCabinLight);
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void OnDriverExitVehicleRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setGasPedal, bool setBrakePedal, bool setKeyInSlot, bool setStarted, bool setCabinLight)
    {
        PlayerControllerB playerObj = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerObj == null)
        {
            Plugin.LogError("OnDriverExitVehicleRpc failed to find player object reference from player client id!");
            return;
        }
        if (GameNetworkManager.Instance.localPlayerController == playerObj)
        {
            Plugin.LogDebug("OnDriverExitVehicleRpc player argument was the previous occupant!");
            return;
        }
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = null;
        steeringAnimValue = 0f;
        currentInterior.driverSeat.ReturnPlayerAnimations(playerObj, false);
        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setStarted;
        if (ignitionStarted && !carExhaustParticle.isEmitting) carExhaustParticle.Play();
        else if (!ignitionStarted && carExhaustParticle.isEmitting) carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
        CancelIgnitionAnimation(ignitionOn: ignitionStarted, setIgnitionAnim: true);
        SetIgnition(setStarted: ignitionStarted, setCabinLightOn: setCabinLight);
        InteractTrigger doorTrigger = isInteriorRHD ? passengerSideDoorTrigger : driverSideDoorTrigger;
        doorTrigger.interactable = true;
    }


    // --- PASSENGER OCCUPANT METHODS ---
    public void SetPassengerInVehicle()
    {
        SetPassengerInVehicleServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    protected void SetPassengerInVehicleServerRpc(int playerId, RpcParams rpcParams = default)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentPassenger != null)
        {
            CancelSetPlayerInVehicleRpc(playerId);
            return;
        }
        currentPassenger = playerController;
        SetPassengerInVehicleRpc(playerController);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    protected void SetPassengerInVehicleRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerObj))
        {
            Plugin.LogError("SetPassengerInVehicleRpc failed to find player object reference from network behaviour!");
            return;
        }
        playerObj.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        InteractTrigger doorTrigger = isInteriorRHD ? driverSideDoorTrigger : passengerSideDoorTrigger;
        if (playerObj == GameNetworkManager.Instance.localPlayerController)
        {
            currentInterior.passengerSeat.SetLocalPlayerIntoSeat();
            localPlayerInPassengerSeat = true;
            AnimatedObjectTrigger door = isInteriorRHD ? driverSideDoor : passengerSideDoor;
            SetTriggerHoverTip(doorTrigger, DOOR_EXIT_HOVERTIP);
            if (door.boolValue) door.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        }
        else
        {
            currentInterior.passengerSeat.SetPlayerAnimations(playerObj, false);
            doorTrigger.interactable = false;
        }
        currentPassenger = playerObj;
        playerObj.playerBodyAnimator.SetFloat(ANIMATION_SPEED, 0.5f);
    }

    public void OnPassengerExitVehicle()
    {
        if (!IsSpawned ||
            NetworkManager == null ||
            !NetworkManager.IsListening)
        {
            return;
        }
        if (currentPassenger != GameNetworkManager.Instance.localPlayerController)
        {
            return;
        }
        localPlayerInPassengerSeat = false;
        InteractTrigger doorTrigger = isInteriorRHD ? driverSideDoorTrigger : passengerSideDoorTrigger;
        SetTriggerHoverTip(doorTrigger, DOOR_ENTER_HOVERTIP);
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        currentPassenger = null;
        OnPassengerExitVehicleRpc(GameNetworkManager.Instance.localPlayerController, GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnPassengerExitVehicleRpc(NetworkBehaviourReference playerNetObjRef, Vector3 exitPoint)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerObj))
        {
            Plugin.LogError("OnPassengerExitVehicleRpc failed to find player object reference from network behaviour!");
            return;
        }
        currentInterior.passengerSeat.ReturnPlayerAnimations(playerObj, false);
        playerObj.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        playerObj.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentPassenger = null;
        InteractTrigger doorTrigger = isInteriorRHD ? driverSideDoorTrigger : passengerSideDoorTrigger;
        doorTrigger.interactable = true;
    }


    // --- LEAVE OCCUPANT MID-GAME METHODS ---
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void OnDriverLeaveGameServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            return;
        }
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
        OnDriverLeave(playerController, ignitionStarted, keyIsInIgnition, drivePedalPressed, brakePedalPressed, cabinLightOn);
        OnDriverLeaveGameRpc(playerId, syncedPosition, syncedRotation, ignitionStarted, keyIsInIgnition, drivePedalPressed, brakePedalPressed, cabinLightOn);
    }

    public void OnDriverLeave(PlayerControllerB playerController, bool setIgnitionState, bool setKeyInSlot, bool gasFloored, bool brakeFloored, bool setCabinLightOn)
    {
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = null;

        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setIgnitionState;

        if (ignitionStarted && !carExhaustParticle.isEmitting) carExhaustParticle.Play();
        else if (!ignitionStarted && carExhaustParticle.isEmitting) carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;

        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;

        currentInterior.driverSeat.ReturnPlayerAnimations(playerController, false);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);

        CancelIgnitionAnimation(ignitionOn: ignitionStarted, setIgnitionAnim: true);
        SetIgnition(setStarted: ignitionStarted, setCabinLightOn: setCabinLightOn);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void OnDriverLeaveGameRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot, bool gasFloored, bool brakeFloored, bool setCabinLightOn)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            return;
        }
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        OnDriverLeave(playerController, setIgnitionState, setKeyInSlot, gasFloored, brakeFloored, setCabinLightOn);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void OnPassengerLeaveGameRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            return;
        }
        currentInterior.passengerSeat.ReturnPlayerAnimations(playerController, false);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);
        currentPassenger = null!;
        InteractTrigger doorTrigger = isInteriorRHD ? driverSideDoorTrigger : passengerSideDoorTrigger;
        doorTrigger.interactable = true;
    }


    // --- OCCUPANT EXITING METHODS ---
    public void ExitFrontLeftSideSeat()
    {
        if (!localPlayerInControl && !isInteriorRHD) return;
        if (!localPlayerInPassengerSeat && isInteriorRHD) return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        if (!driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        int exitPoint = CanExitVehicle(passengerSide: false);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[1].position);
    }

    public void ExitFrontRightSideSeat()
    {
        if (!localPlayerInPassengerSeat && !isInteriorRHD) return;
        if (!localPlayerInControl && isInteriorRHD) return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        int exitPoint = CanExitVehicle(passengerSide: true);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[1].position);
    }


    private int CanExitVehicle(bool passengerSide)
    {
        if (!passengerSide)
        {
            for (int i = 0; i < driverSideExitPoints.Length; i++)
            {
                if (!CheckExitPointInvalid(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, driverSideExitPoints[i].position, exitCarLayerMask, QueryTriggerInteraction.Ignore))
                {
                    return i;
                }
            }
            return -1;
        }
        for (int j = 0; j < passengerSideExitPoints.Length; j++)
        {
            if (!CheckExitPointInvalid(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, passengerSideExitPoints[j].position, exitCarLayerMask, QueryTriggerInteraction.Ignore))
            {
                return j;
            }
        }
        return -1;
    }

    public bool CheckExitPointInvalid(Vector3 playerPos, Vector3 exitPoint, int layerMask, QueryTriggerInteraction interaction)
    {
        if (Physics.Linecast(playerPos, exitPoint, layerMask, interaction))
        {
            return true;
        }

        if (Physics.CheckCapsule(exitPoint, exitPoint + Vector3.up, 0.5f, layerMask, interaction))
        {
            return true;
        }

        LayerMask maskAndVehicle = layerMask | LayerMask.GetMask("Vehicle");

        if (!Physics.Linecast(exitPoint, exitPoint + Vector3.down * 4f, maskAndVehicle, interaction))
        {
            return true;
        }

        return false;
    }


    // --- PLAYER INPUT TO VEHICLE INPUT & VEHICLE CONTROL METHODS ---
    private new void GetVehicleInput()
    {
        PlayerControllerB localDriver = GameNetworkManager.Instance.localPlayerController;
        if (localDriver == null)
            return;
        if (localDriver.isTypingChat ||
            localDriver.quickMenuManager.isMenuOpen)
            return;

        SyncVehicleInput();

        if (!ignitionStarted)
        {
            moveInputVector = Vector2.zero;
            steeringAnimValue = 0f;
            drivePedalPressed = false;
            brakePedalPressed = false;
            return;
        }
        moveInputVector = IngamePlayerSettings.Instance.playerInput.actions.FindAction(PLAYER_MOVEMENT).ReadValue<Vector2>();
        steeringAnimValue = moveInputVector.x;
        steeringInput = Mathf.Clamp(steeringInput + steeringAnimValue * steeringWheelTurnSpeed * Time.deltaTime, -3f, 3f);
        drivePedalPressed = moveInputVector.y > 0.1f;
        brakePedalPressed = moveInputVector.y < -0.1f;
    }

    private void SyncVehicleInput()
    {
        if (syncedDrivePedalPressed != drivePedalPressed || 
            syncedBrakePedalPressed != brakePedalPressed)
        {
            syncedDrivePedalPressed = drivePedalPressed;
            syncedBrakePedalPressed = brakePedalPressed;
            SyncVehicleInputRpc(drivePedalPressed, brakePedalPressed);
        }
    }

    private new void ActivateControl()
    {
        localPlayerInControl = true;
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = GameNetworkManager.Instance.localPlayerController;
    }

    private new void DisableControl()
    {
        localPlayerInControl = false;
        steeringAnimValue = 0f;
        currentDriver = null;
    }


    // --- SHIFTING GEARS METHODS ---
    public new void ShiftGearForwardInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        ShiftGearForward();
    }

    public new void ShiftGearForward()
    {
        if (gear != CarGearShift.Park)
        {
            if (gear == CarGearShift.Reverse)
            {
                ShiftToGearAndSync(3);
            }
            else if (gear == CarGearShift.Drive)
            {
                ShiftToGearAndSync(2);
            }
        }
    }

    public new void ShiftGearBackInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        ShiftGearBack();
    }

    private new void ShiftGearBack()
    {
        if (gear != CarGearShift.Drive)
        {
            if (gear == CarGearShift.Park)
            {
                ShiftToGearAndSync(2);
            }
            else if (gear == CarGearShift.Reverse)
            {
                ShiftToGearAndSync(1);
            }
        }
    }

    public new void ShiftToGearAndSync(int setGear)
    {
        if (gear == (CarGearShift)setGear)
            return;

        timeAtLastGearShift = Time.realtimeSinceStartup;
        playerWhoShifted = GameNetworkManager.Instance.localPlayerController;
        gear = (CarGearShift)setGear;
        gearStickAudio.PlayOneShot(gearStickAudios[setGear - 1]);
        ShiftToGearRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, setGear);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ShiftToGearRpc(int playerId, int setGear)
    {
        timeAtLastGearShift = Time.realtimeSinceStartup;
        playerWhoShifted = StartOfRound.Instance.allPlayerScripts[playerId];
        gear = (CarGearShift)setGear;
        gearStickAudio.PlayOneShot(gearStickAudios[setGear - 1]);
    }


    // --- AUTOPILOT MAGNET ---
    public new void StartMagneting()
    {
        if (!IsOwner)
        {
            return;
        }
        SetVehicleKinematic(setKinematic: true);
        magnetedToShip = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;
        RoundManager.Instance.tempTransform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
        Vector3 tempRotation = RoundManager.Instance.tempTransform.eulerAngles;
        averageVelocityAtMagnetStart = averageVelocity;
        float tempAngle = Vector3.Angle(RoundManager.Instance.tempTransform.forward, -StartOfRound.Instance.magnetPoint.forward);
        Vector3 eulerAngles = transform.eulerAngles;
        if (tempAngle < 45f)
        {
            if (eulerAngles.y < 0f)
            {
                eulerAngles.y -= 46f - tempAngle;
            }
            else
            {
                eulerAngles.y += 46f - tempAngle;
            }
        }
        eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
        eulerAngles.z = Mathf.Round(eulerAngles.z / 90f) * 90f;
        eulerAngles.x += UnityEngine.Random.Range(-25f, 25f);
        magnetTargetRotation = Quaternion.Euler(eulerAngles);
        magnetStartRotation = transform.rotation;
        Quaternion rotation = transform.rotation;
        transform.rotation = magnetTargetRotation;
        magnetTargetPosition = boundsCollider.ClosestPoint(StartOfRound.Instance.magnetPoint.position) - transform.position;
        if (magnetTargetPosition.y >= boundsCollider.bounds.extents.y)
        {
            magnetTargetPosition.y -= boundsCollider.bounds.extents.y / 2f;
        }
        else if (magnetTargetPosition.y <= boundsCollider.bounds.extents.y * 0.4f)
        {
            magnetTargetPosition.y += boundsCollider.bounds.extents.y / 2f;
        }
        magnetTargetPosition = StartOfRound.Instance.magnetPoint.position - magnetTargetPosition;
        magnetTargetPosition.y = Mathf.Max(1f, magnetStartPosition.y);
        magnetTargetPosition = StartOfRound.Instance.elevatorTransform.InverseTransformPoint(magnetTargetPosition);
        transform.rotation = rotation;
        magnetStartPosition = transform.position;
        CollectItemsInTruck();
        if (StartOfRound.Instance.inShipPhase) return;
        if (GameNetworkManager.Instance.localPlayerController == null) return;
        MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, tempRotation, averageVelocityAtMagnetStart);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void MagnetCarRpc(Vector3 targetPosition, Vector3 targetRotation, Vector3 startPosition, Quaternion startRotation, Vector3 tempRotation, Vector3 avgVel)
    {
        SetVehicleKinematic(setKinematic: true);
        magnetedToShip = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;
        RoundManager.Instance.tempTransform.eulerAngles = tempRotation;
        averageVelocityAtMagnetStart = avgVel;
        magnetStartPosition = startPosition;
        magnetStartRotation = startRotation;
        magnetTargetPosition = targetPosition;
        magnetTargetRotation = Quaternion.Euler(targetRotation);
        CollectItemsInTruck();
    }

    public new void CollectItemsInTruck()
    {
        /*
        Collider[] array = Physics.OverlapSphere(transform.position, 25f, 64, QueryTriggerInteraction.Collide);
        for (int i = 0; i < array.Length; i++)
        {
            GrabbableObject itemInTruck = array[i].GetComponent<GrabbableObject>();
            if (itemInTruck == null ||
                itemInTruck.isHeld ||
                itemInTruck.isHeldByEnemy ||
                itemInTruck.transform.parent != transform)
                continue;

            if (lastDriver == null)
            {
                GameNetworkManager.Instance.localPlayerController?.SetItemInElevator(magnetedToShip, magnetedToShip, itemInTruck);
                continue;
            }
            lastDriver.SetItemInElevator(magnetedToShip, magnetedToShip, itemInTruck);
        }
        */
    }


    // --- WEEDKILLER FUNCTIONALITY ---
    public new void AddEngineOil()
    {
        /*
        int setEngineHealth = Mathf.Min(carHP + 4, baseCarHP);
        AddEngineOilOnLocalClient(setEngineHealth);
        AddEngineOilRpc(setEngineHealth);
        */
    }

    public new void AddEngineOilOnLocalClient(int setCarHP)
    {
        hoodAudio.PlayOneShot(pourOil);
        carHP = setCarHP;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void AddEngineOilRpc(int setHP)
    {
        AddEngineOilOnLocalClient(setHP);
    }

    public new void AddTurboBoost()
    {
        /*
        int setTurboBoosts = Mathf.Min(turboBoosts + 1, 5);
        AddTurboBoostOnLocalClient(setTurboBoosts);
        AddTurboBoostRpc(setTurboBoosts);
        */
    }

    public new void AddTurboBoostOnLocalClient(int setTurboBoosts)
    {
        hoodAudio.PlayOneShot(pourTurbo);
        turboBoosts = setTurboBoosts;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void AddTurboBoostRpc(int setTurboBoosts)
    {
        AddTurboBoostOnLocalClient(setTurboBoosts);
    }


    // --- TURBO BOOST AND JUMP ABILITY ---
    private new void DoTurboBoost(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        //if (truckType == TruckVersionType.V55)
        //    return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled) return;
        if (playerController.isTypingChat ||
            playerController.quickMenuManager.isMenuOpen) return;

        if (!localPlayerInControl || !ignitionStarted ||
            jumpingInCar || keyIsInDriverHand) return;

        Vector2 dir = IngamePlayerSettings.Instance.playerInput.actions.FindAction(PLAYER_MOVEMENT, false).ReadValue<Vector2>();
        UseTurboBoostLocalClient(dir);
        UseTurboBoostRpc();
    }

    public new void UseTurboBoostLocalClient(Vector2 dir = default(Vector2))
    {
        currentDriver?.playerBodyAnimator.SetTrigger(JUMP_WHILE_IN_CAR);
        currentDriver?.movementAudio.PlayOneShot(jumpInCarSFX);
        if (IsOwner)
        {
            if (turboBoosts == 0)
            {
                jumpingInCar = true;
                StartCoroutine(jerkCarUpward(dir));
                return;
            }
            else
            {
                Vector3 boostForce = transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
                mainRigidbody.AddForce(boostForce * turboBoostForce + Vector3.up * turboBoostUpwardForce * 0.6f, ForceMode.Impulse);
            }
        }
        if (turboBoosts > 0)
        {
            turboBoosts = Mathf.Max(0, turboBoosts - 1);
            turboBoostAudio.PlayOneShot(turboBoostSFX);
            engineAudio1.PlayOneShot(turboBoostSFX2);
            turboBoostParticle.Play(true);
            if (Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, turboBoostAudio.transform.position) < 10f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                return;
            }
        }
    }

    private new IEnumerator jerkCarUpward(Vector3 dir)
    {
        if (!IsOwner)
        {
            jumpingInCar = false;
            yield break;
        }
        yield return new WaitForSeconds(0.16f);
        Vector3 jerkForce = transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
        mainRigidbody.AddForce(jerkForce * turboBoostForce * 0.22f + Vector3.up * turboBoostUpwardForce * 0.1f, ForceMode.Impulse);
        mainRigidbody.AddForceAtPosition(Vector3.up * jumpForce, hoodFireAudio.transform.position - Vector3.up * 2f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.15f);
        jumpingInCar = false;
        yield break;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void UseTurboBoostRpc()
    {
        UseTurboBoostLocalClient(default(Vector2));
    }


    // --- KEYBINDS ---
    // UNFINISHED


    // --- HORN ---
    public new void SetHonkingLocalClient(bool honk)
    {
        honkingHorn = honk;
        SetHonkRpc(honk);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHonkRpc(bool honk)
    {
        honkingHorn = honk;
    }


    // --- VEHICLE REMOVAL ---
    public new void OnDisable()
    {
        RemoveRainCollision();
        DisableControl();
        vehicleZone.disablePhysicsRegion = true;
        if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(vehicleZone))
        {
            StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(vehicleZone);
        }
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[i];
            if (playerController.transform.parent == vehicleZone.physicsTransform)
            {
                Transform playerTransform = playerController.isInElevator ? playerController.playersManager.elevatorTransform : playerController.playersManager.playersContainer;
                playerController.transform.SetParent(playerTransform);
                Plugin.LogWarning($"Player {i} setting parent since vehicle was disabled");
            }
        }
        if (localPlayerInControl || localPlayerInPassengerSeat)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
        }
        GrabbableObject[] componentsInChildren = physicsRegion.physicsTransform.GetComponentsInChildren<GrabbableObject>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if (RoundManager.Instance.mapPropsContainer != null)
            {
                componentsInChildren[i].transform.SetParent(RoundManager.Instance.mapPropsContainer.transform, worldPositionStays: true);
            }
            else
            {
                componentsInChildren[i].transform.SetParent(null, worldPositionStays: true);
            }
            if (!componentsInChildren[i].isHeld)
            {
                componentsInChildren[i].FallToGround(false, false, default(Vector3));
            }
        }
    }


    // --- UPDATE ---
    public new void Update()
    {
        if (destroyNextFrame)
        {
            if (IsOwner)
            {
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody1.gameObject);
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody2.gameObject);
                UnityEngine.Object.Destroy(base.ragdollPhysicsBody.gameObject);
                UnityEngine.Object.Destroy(this.playerPhysicsBody.gameObject);
                UnityEngine.Object.Destroy(base.gameObject);
            }
            return;
        }
        if (NetworkObject != null && !NetworkObject.IsSpawned)
        {
            RemoveRainCollision();
            vehicleZone.disablePhysicsRegion = true;
            if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(vehicleZone))
            {
                StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(vehicleZone);
            }
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[i];
                if (playerController.transform.parent == vehicleZone.physicsTransform)
                {
                    Transform playerTransform = playerController.isInElevator ? playerController.playersManager.elevatorTransform : playerController.playersManager.playersContainer;
                    playerController.transform.SetParent(playerTransform);
                    Plugin.LogWarning($"Player {i} setting parent since vehicle was removed");
                }
            }
            if (localPlayerInControl || localPlayerInPassengerSeat)
                GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();

            GrabbableObject[] componentsInChildren = physicsRegion.physicsTransform.GetComponentsInChildren<GrabbableObject>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (RoundManager.Instance.mapPropsContainer != null)
                {
                    componentsInChildren[i].transform.SetParent(RoundManager.Instance.mapPropsContainer.transform, worldPositionStays: true);
                }
                else
                {
                    componentsInChildren[i].transform.SetParent(null, worldPositionStays: true);
                }
                if (!componentsInChildren[i].isHeld)
                {
                    componentsInChildren[i].FallToGround(false, false, default(Vector3));
                }
            }
            destroyNextFrame = true;
            return;
        }
        if (magnetedToShip)
        {
            if (!StartOfRound.Instance.magnetOn)
            {
                if (StartOfRound.Instance.attachedVehicle == this)
                {
                    magnetedToShip = false;
                    StartOfRound.Instance.isObjectAttachedToMagnet = false;
                    CollectItemsInTruck();
                }
                return;
            }
            magnetTime = Mathf.Min(magnetTime + Time.deltaTime, 1f);
            magnetRotationTime = Mathf.Min(magnetTime + Time.deltaTime * 0.75f, 1f);
            if (StartOfRound.Instance.inShipPhase)
            {
                carHP = baseCarHP;
                syncedCarHP = carHP;
            }
            if (!finishedMagneting && magnetTime > 0.7f)
            {
                finishedMagneting = true;
                turbulenceAmount = 2f;
                turbulenceAudio.volume = 0.6f;
                turbulenceAudio.PlayOneShot(maxCollisions[UnityEngine.Random.Range(0, maxCollisions.Length)]);
            }
        }
        else
        {
            finishedMagneting = false;
            if (StartOfRound.Instance.attachedVehicle == this)
            {
                StartOfRound.Instance.attachedVehicle = null;
            }
        }

        SyncCarPhysicsToOtherClients();
        ReactToDamage();

        if (carDestroyed)
        {
            RHDInterior.driverSeatTrigger.interactable = false;
            RHDInterior.passengerSeatTrigger.interactable = false;
            LHDInterior.driverSeatTrigger.interactable = false;
            LHDInterior.passengerSeatTrigger.interactable = false;
            return;
        }

        RHDInterior.driverSeatTrigger.interactable = isInteriorRHD && hasBeenSpawned && Time.realtimeSinceStartup - timeSinceSpringingDriverSeat > 3f;
        RHDInterior.passengerSeatTrigger.interactable = isInteriorRHD && hasBeenSpawned;
        LHDInterior.driverSeatTrigger.interactable = !isInteriorRHD && hasBeenSpawned && Time.realtimeSinceStartup - timeSinceSpringingDriverSeat > 3f;
        LHDInterior.passengerSeatTrigger.interactable = !isInteriorRHD && hasBeenSpawned;

        SetCarEffects(steeringAnimValue);
        UpdateOccupantAnimations();
        if (localPlayerInControl && ignitionStarted)
        {
            GetVehicleInput();
            return;
        }
        if (IsOwner)
        {
            return;
        }    
        drivePedalPressed = syncedDrivePedalPressed;
        brakePedalPressed = syncedBrakePedalPressed;
    }

    private void UpdateOccupantAnimations()
    {
        if (currentDriver == null || currentDriver.playerBodyAnimator == null)
            return;

        if (disableAnimations ||
            keyIgnitionCoroutine != null ||
            !ignitionStarted)
            return;

        currentDriver.playerBodyAnimator.SetFloat(ANIMATION_SPEED, playerSteeringWheelAnimFloat); // player steering animation
        currentDriver.playerBodyAnimator.SetFloat(CAR_MOTION_TIME, gearStickAnimValue); // vehicle gearstick --> player gearstick animation position

        int currentAnimIndex = 1;
        float driverLookInput = currentDriver.ladderCameraHorizontal;
        if (!localPlayerInControl)
        {
            var driverObjData = PlayerControllerBPatches.playerData[currentDriver];
            driverLookInput = driverObjData.syncedCameraHorizontal;
        }
        float lookAngle = currentInterior.cameraLookAngle;
        bool isLookingOver = isInteriorRHD ? driverLookInput < lookAngle : driverLookInput > lookAngle;
        if (isLookingOver)
        {
            if (playerWhoShifted == currentDriver && Time.realtimeSinceStartup - timeAtLastGearShift < 1.7f) currentAnimIndex = 5;
            else currentAnimIndex = 4;
            currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, currentAnimIndex);
        }
        else currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, currentAnimIndex);
    }


    // --- RADIO TIME SYNC ---
    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SyncRadioTimeRpc(float songTime, float syncedTime, float staticTime)
    {
        currentSongTime = songTime;
        syncedSongTime = syncedTime;
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        SetRadioTime();
        SetRadioStaticTime(staticTime);
    }

    public void SetRadioTime()
    {
        if (radioAudio.clip == null || !radioOn) return;
        float setTime = (syncedSongTime + (Time.realtimeSinceStartup - timeLastSyncedRadio)) % radioAudio.clip.length;
        if (Mathf.Abs(setTime - radioAudio.time) > 1f)
        {
            radioAudio.time = setTime;
        }
    }

    public void SetRadioStaticTime(float staticTime)
    {
        float setTime = (staticTime + (Time.realtimeSinceStartup - timeLastSyncedRadio)) % radioInterference.clip.length;
        if (Mathf.Abs(setTime - radioInterference.time) > 1f)
        {
            radioInterference.time = setTime;
        }
    }


    // --- RADIO CHANNEL ---
    public new void ChangeRadioStation()
    {
        if (radioClips.Length == 0)
        {
            Plugin.LogWarning("No music found! are you using CruiserTunes to remove the original tracks?");
            return;
        }
        currentRadioClip = (currentRadioClip + 1) % radioClips.Length;
        switch ((int)Mathf.Round(radioSignalQuality))
        {
            case 0:
                radioSignalQuality = 3f;
                radioSignalDecreaseThreshold = 90f;
                break;
            case 1:
                radioSignalQuality = 2f;
                radioSignalDecreaseThreshold = 70f;
                break;
            case 2:
                radioSignalQuality = 1f;
                radioSignalDecreaseThreshold = 30f;
                break;
            case 3:
                radioSignalQuality = 1f;
                radioSignalDecreaseThreshold = 10f;
                break;
        }
        SetRadioOnLocalClient(on: true, setClip: true);
        float setTime = Mathf.Clamp(currentSongTime % radioAudio.clip.length, 0.01f, radioAudio.clip.length - 0.1f);
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        radioAudio.time = setTime;
        SetRadioStationRpc(currentRadioClip, setTime, radioSignalQuality, radioSignalDecreaseThreshold);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioStationRpc(int radioStation, float setTime, float signalQuality, float signalDecrease)
    {
        if (radioClips.Length == 0)
        {
            Plugin.LogWarning("No music found! are you using CruiserTunes to remove the original tracks?");
            return;
        }
        currentRadioClip = radioStation;
        radioSignalQuality = signalQuality;
        radioSignalDecreaseThreshold = signalDecrease;
        SetRadioOnLocalClient(on: true, setClip: true);
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        radioAudio.time = setTime;
    }


    // --- RADIO TOGGLE --- 
    public new void SwitchRadio()
    {
        if (radioClips.Length == 0)
        {
            Plugin.LogWarning("No music found! are you using CruiserTunes to remove the original tracks?");
            return;
        }
        SetRadioOnLocalClient(on: !radioOn, setClip: false);
        float setTime = Mathf.Clamp(currentSongTime % radioAudio.clip.length, 0.01f, radioAudio.clip.length - 0.1f);
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        radioAudio.time = setTime;
        SetRadioRpc(radioOn, currentRadioClip, setTime, radioSignalQuality, radioSignalDecreaseThreshold);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioRpc(bool setRadioOn, int radioStation, float setTime, float signalQuality, float signalDecrease)
    {
        if (radioClips.Length == 0)
        {
            Plugin.LogWarning("No music found! are you using CruiserTunes to remove the original tracks?");
            return;
        }
        currentRadioClip = radioStation;
        radioSignalQuality = signalQuality;
        radioSignalDecreaseThreshold = signalDecrease;
        SetRadioOnLocalClient(on: setRadioOn, setClip: false);
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        radioAudio.time = setTime;
    }


    // --- RADIO VALUES ---
    public new void SetRadioValues()
    {
        if (NetworkManager.IsServer && radioTurnedOnBefore)
        {
            currentSongTime += Time.deltaTime;
            if (Time.realtimeSinceStartup - timeLastSyncedRadio > 1f)
            {
                timeLastSyncedRadio = Time.realtimeSinceStartup;
                syncedSongTime = radioAudio.time;
                SyncRadioTimeRpc(currentSongTime, syncedSongTime, radioInterference.time);
            }
        }
        if (!radioOn || radioAudio.clip == null)
        {
            if (radioAudio.isPlaying) radioAudio.Stop();
            if (radioInterference.isPlaying) radioInterference.Stop();
            return;
        }
        if (!radioTurnedOnBefore) radioTurnedOnBefore = true;
        if (radioAudio.isPlaying && Time.realtimeSinceStartup > radioPingTimestamp)
        {
            radioPingTimestamp = (Time.realtimeSinceStartup + 1f);
            RoundManager.Instance.PlayAudibleNoise(radioAudio.transform.position, 16f, Mathf.Min((radioAudio.volume + radioInterference.volume) * 0.5f, 0.9f), 0, false, 106217);
        }
        if (IsOwner)
        {
            float random = UnityEngine.Random.Range(0, 100);
            float radioSignal = (3f - radioSignalQuality - 1.5f) * radioSignalTurbulence;
            radioSignalDecreaseThreshold = Mathf.Clamp(radioSignalDecreaseThreshold + Time.deltaTime * radioSignal, 0f, 100f);
            if (random > radioSignalDecreaseThreshold)
            {
                radioSignalQuality = Mathf.Clamp(radioSignalQuality - Time.deltaTime, 0f, 3f);
            }
            else
            {
                radioSignalQuality = Mathf.Clamp(radioSignalQuality + Time.deltaTime, 0f, 3f);
            }
            if (Time.realtimeSinceStartup - changeRadioSignalTime > 0.3f)
            {
                changeRadioSignalTime = Time.realtimeSinceStartup;
                if (radioSignalQuality < 1.2f && UnityEngine.Random.Range(0, 100) < 6)
                {
                    radioSignalQuality = Mathf.Min(radioSignalQuality + 1.5f, 3f);
                    radioSignalDecreaseThreshold = Mathf.Min(radioSignalDecreaseThreshold + 30f, 100f);
                }
                SetRadioSignalQualityRpc((int)Mathf.Round(radioSignalQuality), radioSignalDecreaseThreshold);
            }
        }
        switch ((int)Mathf.Round(radioSignalQuality))
        {
            case 3:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 1f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 0f, 2f * Time.deltaTime);
                break;
            case 2:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 0.85f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 0.4f, 2f * Time.deltaTime);
                break;
            case 1:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 0.6f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 0.8f, 2f * Time.deltaTime);
                break;
            case 0:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 0.4f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 1f, 2f * Time.deltaTime);
                break;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetRadioSignalQualityRpc(int signalQuality, float signalDecrease)
    {
        radioSignalQuality = signalQuality;
        radioSignalDecreaseThreshold = signalDecrease;
    }

    public new void SetRadioOnLocalClient(bool on, bool setClip = true)
    {
        Plugin.LogDebug($"Radio called with setRadioOn? {on}, setClip? {setClip}");
        radioOn = on;
        if (on)
        {
            if (setClip || radioAudio.clip == null)
            {
                if (radioAudio.clip == null) Plugin.LogDebug("Setting station, was null!");
                radioAudio.clip = radioClips[currentRadioClip];
                Plugin.LogDebug($"Set radio clip to {currentRadioClip}, station? {radioAudio.clip.name}");
            }
            radioAudio.Play();
            radioInterference.Play();
            return;
        }
        radioAudio.Stop();
        radioInterference.Stop();
        Plugin.LogDebug("Stop radio playback!");
    }


    // --- WHEEL VISUALS ---
    private void MatchWheelMeshToCollider(MeshRenderer wheelMesh, WheelCollider wheelCollider, float steeringInput = 0f)
    {
        Vector3 position = wheelCollider.transform.position;
        if (Physics.Raycast(position, -wheelCollider.transform.up, out hit, wheelCollider.suspensionDistance + wheelCollider.radius, 2305))
        {
            wheelMesh.transform.position = hit.point + wheelCollider.transform.up * wheelCollider.radius;
        }
        else
        {
            wheelMesh.transform.position = position - wheelCollider.transform.up * wheelCollider.suspensionDistance;
        }
        wheelMesh.transform.localRotation = Quaternion.Euler(wheelsRPM, steeringInput, 0.0f);
    }


    // --- VISUAL EFFECTS ---
    private float GetAnimationSpeed()
    {
        return inBetaMode ? 2f : -2f;
    }

    private new void SetCarEffects(float setSteering)
    {
        // steering
        bool useInput = IsOwner || inBetaMode;
        setSteering = useInput ? setSteering : 0f;

        steeringWheelAnimFloat = Mathf.Clamp(steeringWheelAnimFloat + setSteering * steeringWheelTurnSpeed * Time.deltaTime / 6f, -1f, 1f);
        float playerSteer = Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f) - steeringWheelAnimator.GetFloat(STEERING_WHEEL_SPEED);
        steeringWheelAnimator.SetFloat(STEERING_WHEEL_SPEED, Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f));

        // grab the players current steering animation float
        if (localPlayerInControl && currentDriver != null)
            playerSteeringWheelAnimFloat = currentDriver.playerBodyAnimator.GetFloat(ANIMATION_SPEED) + playerSteer * GetAnimationSpeed();

        // misc
        SetGearstick();
        SetLighting();
        SetAudioEffects();
        SetTyreEffects();
        SetIgnitionKey();

        if (IsOwner)
        {
            SyncEffectsToOtherClients();
            if (!syncedExtremeStress && underExtremeStress && extremeStressAudio.volume > 0.35f)
            {
                syncedExtremeStress = true;
                SyncExtremeStressRpc(underExtremeStress);
            }
            else if (syncedExtremeStress && !underExtremeStress && extremeStressAudio.volume < 0.5f)
            {
                syncedExtremeStress = false;
                SyncExtremeStressRpc(underExtremeStress);
            }
            return;
        }
        playerSteeringWheelAnimFloat = Mathf.MoveTowards(playerSteeringWheelAnimFloat, syncedPlayerSteeringAnim, steeringWheelTurnSpeed * Time.deltaTime / 6f);
        if (inBetaMode)
        {
            return;
        }
        steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, syncedWheelRotation, steeringWheelTurnSpeed * Time.deltaTime / 6f);
        steeringInput = Mathf.MoveTowards(steeringInput, syncedSteeringInput, steeringWheelTurnSpeed * Time.deltaTime);
    }

    // automatic shifter position
    private void SetGearstick()
    {
        switch (gear)
        {
            case CarGearShift.Park:
                {
                    gearStickAnimValue = Mathf.MoveTowards(gearStickAnimValue, 1f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastGearShift));
                    break;
                }
            case CarGearShift.Reverse:
                {
                    gearStickAnimValue = Mathf.MoveTowards(gearStickAnimValue, 0.5f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastGearShift));
                    break;
                }
            case CarGearShift.Drive:
                {
                    gearStickAnimValue = Mathf.MoveTowards(gearStickAnimValue, 0f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastGearShift));
                    break;
                }
        }
        gearStickAnimator.SetFloat("gear", Mathf.Clamp(gearStickAnimValue, 0.01f, 0.99f));
    }

    // manual shifter position
    private void SetGearshift()
    {
        //TBD
    }

    private void SetLighting()
    {
        float wheelSpeed = Mathf.Round(wheelRPM / 5f) * 5f;
        bool setBackLightsOn = ignitionStarted && wheelSpeed <= -5f;
        SetBackLightsOn(setOn: setBackLightsOn);
    }

    private void SetBackLightsOn(bool setOn)
    {
        if (backLightsOn == setOn)
        {
            return;
        }
        backLightsOn = setOn;
        backLightsMesh.material = setOn ? backLightOnMat : headlightsOffMat;
        backLightsContainer.SetActive(setOn);
    }

    /// <summary>
    ///  Available from EnemySoundFixes, licensed under GNU General Public License.
    ///  Source: https://github.com/ButteryStancakes/EnemySoundFixes/blob/master/Patches/CruiserPatches.cs
    /// </summary>
    private new void SetVehicleAudioProperties(AudioSource audio, bool audioActive, float lowest, float highest, float lerpSpeed, bool useVolumeInsteadOfPitch = false, float onVolume = 1f)
    {
        if (audioActive && ((audio == rollingAudio || audio == skiddingAudio) && (magnetedToShip || allWheelsAirborne)))
            audioActive = false;

        if (!audioActive)
        {
            if (useVolumeInsteadOfPitch)
            {
                audio.volume = Mathf.Lerp(audio.volume, 0f, lerpSpeed * Time.deltaTime);
            }
            else
            {
                audio.volume = Mathf.Lerp(audio.volume, 0f, 4f * Time.deltaTime);
                audio.pitch = Mathf.Lerp(audio.pitch, lowest, 4f * Time.deltaTime);
            }
            if (audio.isPlaying)
            {
                if (audio == engineAudio1 || audio == engineAudio2)
                {
                    if (audio.volume == 0f)
                        audio.Stop();
                }
                else
                {
                    if (audio.volume <= 0.001f)
                        audio.Stop();
                }
            }
            return;
        }
        if (!audio.isPlaying)
        {
            audio.Play();
        }
        if (useVolumeInsteadOfPitch)
        {
            audio.volume = Mathf.Max(Mathf.Lerp(audio.volume, highest, lerpSpeed * Time.deltaTime), lowest);
            return;
        }
        audio.volume = Mathf.Lerp(audio.volume, onVolume, 20f * Time.deltaTime);
        audio.pitch = Mathf.Lerp(audio.pitch, highest, lerpSpeed * Time.deltaTime);
    }

    public void SetAudioEffects()
    {
        float highestAudio1 = Mathf.Clamp((EngineRPM / engineIntensityPercentage), 0.65f, 1.15f);
        float highestAudio2 = Mathf.Clamp((EngineRPM / engineIntensityPercentage), 0.7f, 1.5f);
        float wheelSpeed = Mathf.Abs(wheelRPM);
        float highestTyre = Mathf.Clamp(wheelSpeed / (180f * 0.35f), 0f, 1f);
        carEngine2AudioActive = ignitionStarted;
        carRollingAudioActive = !allWheelsAirborne && wheelSpeed > 10f;
        if (!ignitionStarted)
        {
            highestAudio1 = 1f;
        }
        SetVehicleAudioProperties(engineAudio1, carEngine1AudioActive, 0.7f, highestAudio1, 2f, useVolumeInsteadOfPitch: false, 0.7f);
        SetVehicleAudioProperties(engineAudio2, carEngine2AudioActive, 0.7f, highestAudio2, 3f, useVolumeInsteadOfPitch: false, 0.5f);
        SetVehicleAudioProperties(rollingAudio, carRollingAudioActive, 0f, highestTyre, 5f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(extremeStressAudio, underExtremeStress, 0.2f, 1f, 3f, useVolumeInsteadOfPitch: true);
        SetRadioValues();
        float enginePingTime = inBetaMode ? 0.005f : 2f;
        if (engineAudio1.volume > 0.3f && engineAudio1.isPlaying && !inBetaMode && Time.realtimeSinceStartup - timeAtLastEngineAudioPing > enginePingTime)
        {
            timeAtLastEngineAudioPing = Time.realtimeSinceStartup;
            int engineNoiseId = inBetaMode ? 106217 : 2692;
            if (EngineRPM > 130f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 32f, 0.75f, 0, false, engineNoiseId);
            }
            if (EngineRPM > 60f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 25f, 0.6f, 0, false, engineNoiseId);
            }
            else if (!ignitionStarted)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 15f, 0.6f, 0, false, engineNoiseId);
            }
            else
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 11f, 0.5f, 0, false, engineNoiseId);
            }
        }
        //if (gear == CarGearShift.Reverse)
        //{
        //    reverseWhineAudio.pitch = Mathf.Lerp(0f, 1.65f, wheelSpeed / 420f);
        //    reverseWhineAudio.volume = Mathf.Lerp(0f, 1f, wheelSpeed / 310f);

        //    if (!reverseWhineAudio.isPlaying)
        //        reverseWhineAudio.Play();
        //}
        //else
        //{
        //    reverseWhineAudio.pitch = Mathf.Lerp(reverseWhineAudio.pitch, 0f, 16f * Time.deltaTime);
        //    reverseWhineAudio.volume = Mathf.Lerp(reverseWhineAudio.volume, 0f, 16f * Time.deltaTime);
        //}

        //float currentXInput = Mathf.Abs(moveInputVector.x);
        //SetVehicleAudioProperties(steeringWheelAudio, currentXInput > 0.1f, 0f, currentXInput, 5f, true);
        //SetVehicleAudioProperties(steeringWheelAudio, currentXInput > 0.1f, 0f, currentXInput, 5f, true);

        turbulenceAudio.volume = Mathf.Lerp(turbulenceAudio.volume, Mathf.Min(1f, turbulenceAmount), 10f * Time.deltaTime);
        turbulenceAmount = Mathf.Max(turbulenceAmount - Time.deltaTime, 0f);

        if (turbulenceAudio.volume > 0.02f)
        {
            if (!turbulenceAudio.isPlaying)
                turbulenceAudio.Play();
        }
        else if (turbulenceAudio.isPlaying)
            turbulenceAudio.Stop();

        if (honkingHorn)
        {
            if (!inBetaMode) hornAudio.pitch = 1f;
            if (!hornAudio.isPlaying)
            {
                hornAudio.Play();
                if (inBetaMode) hornAudio.pitch = 1f;
            }

            if (Time.realtimeSinceStartup - timeAtLastHornPing > 2f)
            {
                timeAtLastHornPing = Time.realtimeSinceStartup;
                RoundManager.Instance.PlayAudibleNoise(hornAudio.transform.position, 28f, 0.85f, 0, noiseIsInsideClosedShip: false, 106217);
            }
        }
        else
        {
            hornAudio.pitch = Mathf.Max(hornAudio.pitch - Time.deltaTime * 6f, 0.01f);

            if (hornAudio.pitch <= 0.02f)
                hornAudio.Stop();
        }
    }


    // --- MISC EFFECTS ---
    // tyre skid effects
    public void SetTyreEffects()
    {
        if (IsOwner)
        {
            float vehicleSpeed = Vector3.Dot(Vector3.Normalize(mainRigidbody.velocity * 1000f), transform.forward);
            float wheelSpeed = Mathf.Abs(backWheelRPM);
            bool audioActive = vehicleSpeed > -0.6f && vehicleSpeed < 0.4f && (averageVelocity.magnitude > 4f || wheelSpeed > 400f);
            if (backWheelsGrounded)
            {
                bool tyreSlip = psuedoSlipping || forwardSlipping || sidewaySlipping;
                if (tyreSlip)
                {
                    audioActive = true;
                    vehicleSpeed = Mathf.Max(vehicleSpeed, 0.8f);
                    if (averageVelocity.magnitude > 8f && !tireSparks.isPlaying)
                        tireSparks.Play(true);
                }
                else
                {
                    audioActive = false;
                    if (tireSparks.isEmitting)
                        tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            else
            {
                audioActive = false;
                if (tireSparks.isEmitting)
                    tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            SetVehicleAudioProperties(skiddingAudio, audioActive, 0f, vehicleSpeed, 3f, true, 1f);
            if (Mathf.Abs(tyreStress - vehicleSpeed) > 0.02f || wheelSlipping != audioActive)
            {
                tyreStress = vehicleSpeed;
                wheelSlipping = audioActive;
                SetTyreStressRpc(vehicleSpeed, audioActive);
            }
            return;
        }
        if (wheelSlipping && averageVelocity.magnitude > 8f && !tireSparks.isPlaying)
        {
            tireSparks.Play(true);
        }
        else if (!wheelSlipping && tireSparks.isEmitting)
        {
            tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        SetVehicleAudioProperties(skiddingAudio, wheelSlipping, 0f, tyreStress, 3f, true, 1f);
    }


    // what a mess
    public void SetIgnitionKey(bool localUseBodyHands = false)
    {
        if (ignitionAnimator.enabled != !inBetaMode)
            ignitionAnimator.enabled = !inBetaMode;

        if (inBetaMode)
        {
            // old key effects
            if (keyObject.transform.parent != carKeyContainer.transform)
                keyObject.transform.SetParent(carKeyContainer.transform);
            keyObject.transform.localScale = ignitionKeyScale;

            if (currentDriver == null || !keyIsInDriverHand)
            {
                if (keyObject.enabled != keyIsInIgnition)
                    keyObject.enabled = keyIsInIgnition;

                if (carKeyInHand.transform.parent != carKeyContainer.transform)
                    carKeyInHand.transform.SetParent(carKeyContainer.transform, false);
                carKeyInHand.transform.localScale = Vector3.one;

                carKeyInHand.transform.localPosition = Vector3.zero;
                carKeyInHand.transform.localRotation = Quaternion.identity;

                if (ignitionStarted)
                {
                    keyObject.transform.position = ignitionTurnedPosition.position;
                    keyObject.transform.rotation = ignitionTurnedPosition.rotation;
                }
                else
                {
                    keyObject.transform.position = ignitionNotTurnedPosition.position;
                    keyObject.transform.rotation = ignitionNotTurnedPosition.rotation;
                }
                return;
            }

            if (keyIsInDriverHand)
            {
                if (!keyObject.enabled)
                    keyObject.enabled = true;

                Transform keyParent;
                bool useLocalHands = localPlayerInControl && !localUseBodyHands;
                if (useLocalHands)
                {
                    keyParent = currentDriver.localItemHolder;
                }
                else
                {
                    keyParent = currentDriver.serverItemHolder;
                }

                if (carKeyInHand.transform.parent != keyParent.parent)
                    carKeyInHand.transform.SetParent(keyParent.parent, false);
                carKeyInHand.transform.localScale = Vector3.one;

                // -179.855f
                if (useLocalHands) carKeyInHand.transform.SetLocalPositionAndRotation(new(-0.002f, 0.036f, -0.042f), Quaternion.Euler(-3.616f, -2.302f, 0.145f));
                else carKeyInHand.transform.SetLocalPositionAndRotation(new(-0.04170258f, 6.530248e-05f, -0.03752365f), Quaternion.Euler(13.794f, -3.466f, -20.947f));

                keyObject.transform.position = carKeyInHand.transform.position;
                keyObject.transform.rotation = carKeyInHand.transform.rotation;
                keyObject.transform.Rotate(rotationOffset);

                Vector3 vector = positionOffset;
                vector = carKeyInHand.transform.rotation * vector;
                keyObject.transform.position += vector;
            }     
            // end
            return;
        }

        // new key effects
        if (currentDriver == null || keyIsInIgnition)
        {
            if (keyObject.enabled != keyIsInIgnition)
                keyObject.enabled = keyIsInIgnition;

            if (keyObject.transform.parent != carKeyContainer.transform)
                keyObject.transform.SetParent(carKeyContainer.transform);
            keyObject.transform.localScale = ignitionKeyScale;

            if (carKeyInHand.transform.parent != carKeyContainer.transform)
                carKeyInHand.transform.SetParent(carKeyContainer.transform, false);
            carKeyInHand.transform.localScale = Vector3.one;

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            keyObject.transform.position = ignitionKeyPosition.position;
            keyObject.transform.rotation = ignitionKeyPosition.rotation;

            if (currentDriver == null && leftHandServerItemTarget != null)
                leftHandServerItemTarget = null!;
            return;
        }

        if (leftHandServerItemTarget == null)
        {
            leftHandServerItemTarget = currentDriver.bodyParts[2].Find("hand.L").transform;
            return;
        }

        if (keyIsInDriverHand)
        {
            if (!keyObject.enabled)
                keyObject.enabled = true;

            Transform keyParent;
            Vector3 posOffset, rotOffset;

            bool useLocalHands = localPlayerInControl && !localUseBodyHands;
            if (!isInteriorRHD)
            {
                keyParent = useLocalHands
                    ? currentDriver.localItemHolder.parent
                    : currentDriver.serverItemHolder.parent;

                posOffset = useLocalHands ? LHD_Pos_Local : LHD_Pos_Server;
                rotOffset = useLocalHands ? LHD_Rot_Local : LHD_Rot_Server;
            }
            else
            {
                keyParent = useLocalHands
                    ? currentDriver.leftHandItemTarget.transform
                    : leftHandServerItemTarget;

                posOffset = useLocalHands ? RHD_Pos_Local : RHD_Pos_Server;
                rotOffset = useLocalHands ? RHD_Rot_Local : RHD_Rot_Server;
            }

            if (carKeyInHand.transform.parent != keyParent)
                carKeyInHand.transform.SetParent(keyParent, false);
            carKeyInHand.transform.localScale = Vector3.one;

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            if (keyObject.transform.parent != carKeyInHand.transform)
                keyObject.transform.SetParent(carKeyInHand.transform);

            keyObject.transform.localPosition = posOffset;
            keyObject.transform.localRotation = Quaternion.Euler(rotOffset);
        }
        else
        {
            if (keyObject.enabled)
                keyObject.enabled = false;

            if (keyObject.transform.parent != carKeyContainer.transform)
                keyObject.transform.SetParent(carKeyContainer.transform);
            keyObject.transform.localScale = ignitionKeyScale;

            if (carKeyInHand.transform.parent != carKeyContainer.transform)
                carKeyInHand.transform.SetParent(carKeyContainer.transform, false);
            carKeyInHand.transform.localScale = Vector3.one;

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            keyObject.transform.position = ignitionKeyPosition.position;
            keyObject.transform.rotation = ignitionKeyPosition.rotation;
        }
    }


    // --- PHYSICS UPDATE ---
    public new void FixedUpdate()
    {
        SetVehicleToDropship();
        SetVehicleToFixedPosition();
        TryAttachToShipMagnet();

        MovePhysicsBodies();
        CalculateVehicleVelocity();
        //SyncCarPhysicsToOtherClients();

        if (carDestroyed)
        {
            SetPreviousVehiclePosition();
            return;
        }

        ApplySteering();
        ApplyWheelForces();

        SetVFXWheelSpeed();

        MatchWheelMeshToCollider(leftWheelMesh, FrontLeftWheel, tyreSteeringAngle);
        MatchWheelMeshToCollider(rightWheelMesh, FrontRightWheel, tyreSteeringAngle);
        MatchWheelMeshToCollider(backLeftWheelMesh, BackLeftWheel);
        MatchWheelMeshToCollider(backRightWheelMesh, BackRightWheel);

        allWheelsAirborne = !FrontLeftWheel.isGrounded &&
                            !FrontRightWheel.isGrounded &&
                            !BackLeftWheel.isGrounded &&
                            !BackRightWheel.isGrounded;

        backWheelsGrounded = BackLeftWheel.isGrounded &&
                             BackRightWheel.isGrounded;

        if (!IsOwner)
        {
            SetCarPhysicsValuesOnClient();
            SetTorqueForces(useSynced: true);
            CalculateWheelSlip(calculatePhysics: false);
            SetPreviousVehiclePosition();
            return;
        }

        UpdateCarStress();
        UpdateEngineRPMFromWheels();
        SetTorqueForces(useSynced: false);
        SyncCarDrivetrain();
        SyncCarWheelTorque();

        if (mainRigidbody.IsSleeping() || magnetedToShip || allWheelsAirborne)
        {
            CalculateWheelSlip(calculatePhysics: false);
            SetPreviousVehiclePosition();
            return;
        }

        CalculateWheelSlip(calculatePhysics: true);
        SetPreviousVehiclePosition();
    }

    private void SetCarPhysicsValuesOnClient()
    {
        if (ignitionStarted) EngineRPM = syncedEngineRPM;
        else EngineRPM = Mathf.Lerp(EngineRPM, 0f, 3f * Time.fixedDeltaTime);

        frontWheelRPM = syncedFrontWheelRPM;
        backWheelRPM = syncedBackWheelRPM;
        wheelRPM = syncedWheelRPM;
    }

    private void SetPreviousVehiclePosition()
    {
        previousVehiclePosition = mainRigidbody.position;
        previousVehicleRotation = mainRigidbody.rotation;
    }

    private void SetVehicleToDropship()
    {
        if (StartOfRound.Instance.inShipPhase ||
            loadedVehicleFromSave ||
            hasDeliveredVehicle)
            return;

        if (itemShip == null && ScandalsTweaks.Utils.References.itemShip != null)
            itemShip = ScandalsTweaks.Utils.References.itemShip;

        if (itemShip == null)
        {
            inDropshipAnimation = false;
            SetVehicleKinematic(setKinematic: true);
            mainRigidbody.MovePosition(StartOfRound.Instance.notSpawnedPosition.position + Vector3.forward * 30f);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
            return;
        }
        if (itemShip.untetheredVehicle)
        {
            inDropshipAnimation = false;
            mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
            mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
            hasBeenSpawned = true;
            hasDeliveredVehicle = true;
        }
        else if (itemShip.deliveringVehicle)
        {
            inDropshipAnimation = true;
            SetVehicleKinematic(setKinematic: true);
            mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
            mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
        }
    }

    private void SetVehicleKinematic(bool setKinematic)
    {
        if (mainRigidbody.isKinematic == setKinematic)
            return;

        mainRigidbody.isKinematic = setKinematic;
        Plugin.LogDebug($"Set 'mainRigidbody' kinematic to: {setKinematic}");
    }

    private void SetVehicleToFixedPosition()
    {
        // magnet/client sync
        if (magnetedToShip)
        {
            SetVehicleKinematic(setKinematic: true);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
            mainRigidbody.MovePosition(Vector3.Lerp(magnetStartPosition, StartOfRound.Instance.elevatorTransform.position + magnetTargetPosition, magnetPositionCurve.Evaluate(magnetTime)));
            mainRigidbody.MoveRotation(Quaternion.Lerp(magnetStartRotation, magnetTargetRotation, magnetRotationCurve.Evaluate(magnetRotationTime)));
            averageVelocityAtMagnetStart = Vector3.Lerp(averageVelocityAtMagnetStart, Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 4f), 4f * Time.fixedDeltaTime);
            if (!finishedMagneting) magnetStartPosition += Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 5f) * Time.fixedDeltaTime;
            return;
        }

        if (IsOwner || inDropshipAnimation)
            return;

        SetVehicleKinematic(setKinematic: true);
        Mathf.Clamp(syncSpeedMultiplier * Vector3.Distance(transform.position, syncedPosition), 1.3f, 300f);
        Vector3 vector2 = Vector3.Lerp(transform.position, syncedPosition, Time.fixedDeltaTime * syncSpeedMultiplier);
        mainRigidbody.MovePosition(vector2);
        mainRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, syncedRotation, syncRotationSpeed));
        /*
        Vector3 syncVel = syncedPosition + (averageVelocity * Time.fixedDeltaTime);
        Mathf.Clamp(syncSpeedMultiplier * Vector3.Distance(mainRigidbody.position, syncVel), 1.3f, 300f);
        Vector3 position = Vector3.Lerp(mainRigidbody.position, syncVel, Time.fixedDeltaTime * syncSpeedMultiplier);
        mainRigidbody.MovePosition(position);
        mainRigidbody.MoveRotation(Quaternion.Lerp(mainRigidbody.rotation, syncedRotation, syncRotationSpeed));
        */
        truckVelocityLastFrame = mainRigidbody.velocity;
    }

    private void TryAttachToShipMagnet()
    {
        if (magnetedToShip)
            return;

        if (!IsOwner || carDestroyed ||
            StartOfRound.Instance.isObjectAttachedToMagnet ||
            StartOfRound.Instance.attachedVehicle != null ||
            !StartOfRound.Instance.magnetOn ||
            Vector3.Distance(transform.position, StartOfRound.Instance.magnetPoint.position) >= 10f)
            return;

        if (!Physics.Linecast(transform.position, StartOfRound.Instance.magnetPoint.position, 256, QueryTriggerInteraction.Ignore))
        {
            StartMagneting();
            return;
        }
    }

    private void MovePhysicsBodies()
    {
        ragdollPhysicsBody.Move(
          transform.position,
          transform.rotation);
        windwiperPhysicsBody1.Move(
          windwiper1.position,
          windwiper1.rotation);
        windwiperPhysicsBody2.Move(
          windwiper2.position,
          windwiper2.rotation);
        playerPhysicsBody.transform.localPosition = Vector3.zero;
        playerPhysicsBody.transform.localRotation = Quaternion.identity;
    }

    private void CalculateVehicleVelocity()
    {
        if (averageCount > movingAverageLength)
        {
            averageVelocity += (mainRigidbody.velocity - averageVelocity) / (float)(movingAverageLength + 1);
        }
        else
        {
            averageCount++;
            averageVelocity += mainRigidbody.velocity;
            if (averageCount == movingAverageLength)
            {
                averageVelocity /= (float)averageCount;
            }
        }
    }

    private void ApplySteering()
    {
        tyreSteeringAngle = 50f * steeringWheelAnimFloat;
        float steeringFloat;
        if (inBetaMode)
        {
            steeringFloat = 15f * steeringInput;
        }
        else
        {
            if (useSteeringCurve)
            {
                float absFloat = Mathf.Abs(steeringWheelAnimFloat);
                float signFloat = Mathf.Sign(steeringWheelAnimFloat);
                steeringFloat = steeringCurve.Evaluate(absFloat) * 50f * signFloat;
            }
            else steeringFloat = tyreSteeringAngle;
        }
        steeringAngle = steeringFloat;
        FrontLeftWheel.steerAngle = steeringAngle;
        FrontRightWheel.steerAngle = steeringAngle;
    }

    private void ApplyWheelForces()
    {
        // front wheels
        SetTorqueToWheelCollider(FrontLeftWheel, currentMotorTorque, currentBrakeTorque);
        SetTorqueToWheelCollider(FrontRightWheel, currentMotorTorque, currentBrakeTorque);

        // back wheels
        SetTorqueToWheelCollider(BackLeftWheel, currentMotorTorque, currentBrakeTorque);
        SetTorqueToWheelCollider(BackRightWheel, currentMotorTorque, currentBrakeTorque);

        /*
        // instability wheels
        for (int iW = 0; iW < otherWheels.Length; iW++)
        {
            otherWheels[iW].motorTorque = currentMotorTorque;
            otherWheels[iW].brakeTorque = currentBrakeTorque;
        }

        SetWheelRotationVelocity();
        */
    }

    private void SetTorqueToWheelCollider(WheelCollider wheelCollider, float motorForce, float brakeForce)
    {
        wheelCollider.motorTorque = motorForce;
        wheelCollider.brakeTorque = brakeForce;
    }

    /*
    private void SetWheelRotationVelocity()
    {
        // rotation speed-limiter
        FrontLeftWheel.rotationSpeed = Mathf.Clamp(FrontLeftWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
        FrontRightWheel.rotationSpeed = Mathf.Clamp(FrontRightWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
        BackLeftWheel.rotationSpeed = Mathf.Clamp(BackLeftWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
        BackRightWheel.rotationSpeed = Mathf.Clamp(BackRightWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
    }
    */

    private void SetVFXWheelSpeed()
    {
        float wheelSpeed = Mathf.Round(wheelRPM / 4f) * 4f;
        wheelsRPM = Mathf.Repeat(wheelsRPM + (wheelSpeed * 0.5f) * Mathf.Rad2Deg * Time.fixedDeltaTime, 360f);
    }

    private void CalculateWheelSlip(bool calculatePhysics)
    {
        if (!calculatePhysics)
        {
            forwardsSlip = 0f;
            sidewaysSlip = 0f;
            return;
        }
        if (inBetaMode)
        {
            psuedoSlipping = currentMotorTorque > 900f;

            forwardsSlip = 0f;
            sidewaysSlip = 0f;
            return;
        }
        for (int i = 0; i < wheels.Count; i++)
        {
            if (wheels[i].GetGroundHit(out var hit))
            {
                wheelHits[i] = hit;
            }
            else
            {
                wheelHits[i] = default;
            }
        }
        forwardsSlip = (wheelHits[2].forwardSlip + wheelHits[3].forwardSlip) * 0.5f;
        sidewaysSlip = (wheelHits[2].sidewaysSlip + wheelHits[3].sidewaysSlip) * 0.5f;

        forwardSlipping = currentMotorTorque > 900f && Mathf.Abs(forwardsSlip) > 0.2f;
    }

    private void UpdateCarStress()
    {
        if (!ignitionStarted)
        {
            return;
        }
        if (!localPlayerInControl && inBetaMode)
        {
            return;
        }
        float vehicleStress = 0f;
        switch (gear)
        {
            case CarGearShift.Park:
                {
                    if (drivePedalPressed)
                    {
                        vehicleStress += 1.2f;
                        lastStressType += "; Accelerating while in park";
                    }
                    else if (!magnetedToShip && !allWheelsAirborne && Mathf.Abs(wheelRPM) > 150f)
                    {
                        vehicleStress += Mathf.Clamp((Mathf.Abs(wheelRPM) - 100f) / 350f, 0f, 1.3f);
                        lastStressType += "; In park while at high speed";
                    }
                    break;
                }
            case CarGearShift.Reverse:
                {
                    if (brakePedalPressed && drivePedalPressed)
                    {
                        vehicleStress += 2f;
                        lastStressType += "; Accelerating while braking";
                    }
                    else if (wheelRPM > 250f)
                    {
                        vehicleStress += Mathf.Max((wheelRPM - 250f) / 1000f, 0f);
                        lastStressType += "; Reversing while at high speed";
                    }
                    break;
                }
            case CarGearShift.Drive:
                {
                    if (brakePedalPressed && drivePedalPressed)
                    {
                        vehicleStress += 2f;
                        lastStressType += "; Accelerating while braking";
                    }
                    else if (wheelRPM < -250f)
                    {
                        vehicleStress += Mathf.Max((wheelRPM - 250f) / 1000f, 0f);
                        lastStressType += "; Reversing while at high speed";
                    }
                    break;
                }
        }
        SetInternalStress(vehicleStress);
        stressPerSecond = vehicleStress;
    }

    private void UpdateEngineRPMFromWheels()
    {
        if (!localPlayerInControl && inBetaMode)
        {
            if (!ignitionStarted || magnetedToShip)
            {
                EngineRPM = Mathf.Lerp(EngineRPM, 0f, 3f * Time.fixedDeltaTime);
            }
            return;
        }
        frontWheelRPM = (NormaliseFloat(FrontLeftWheel.rpm) + NormaliseFloat(FrontRightWheel.rpm)) / 2f;
        backWheelRPM = (NormaliseFloat(BackLeftWheel.rpm) + NormaliseFloat(BackRightWheel.rpm)) / 2f;
        wheelRPM = (frontWheelRPM + backWheelRPM) / 2f;
        bool calculateEngineSpeed = ignitionStarted && (!magnetedToShip || drivePedalPressed);
        float engineSpeed = inBetaMode ? frontWheelRPM : wheelRPM;
        if (calculateEngineSpeed) EngineRPM = Mathf.Abs(engineSpeed);
        else EngineRPM = Mathf.Lerp(EngineRPM, 0f, 3f * Time.fixedDeltaTime);
    }

    private void SetTorqueForces(bool useSynced)
    {
        if (!ignitionStarted)
        {
            currentMotorTorque = 0f;
            currentBrakeTorque = maxBrakingTorque;
            return;
        }
        if (useSynced)
        {
            currentMotorTorque = syncedMotorTorque;
            currentBrakeTorque = syncedBrakeTorque;
            return;
        }
        if (!localPlayerInControl && inBetaMode)
        {
            return;
        }
        switch (gear)
        {
            case CarGearShift.Park:
                {
                    currentMotorTorque = 0f;
                    currentBrakeTorque = maxBrakingTorque;
                    break;
                }
            case CarGearShift.Reverse:
                {
                    currentMotorTorque = drivePedalPressed ? -EngineTorque : -idleSpeed;
                    currentBrakeTorque = brakePedalPressed ? maxBrakingTorque : 0f;
                    break;
                }
            case CarGearShift.Drive:
                {
                    currentMotorTorque = drivePedalPressed ? Mathf.Clamp(Mathf.MoveTowards(currentMotorTorque, EngineTorque, carAcceleration * Time.fixedDeltaTime), 325f, 1000f) : idleSpeed;
                    currentBrakeTorque = brakePedalPressed ? maxBrakingTorque : 0f;
                    break;
                }
        }
    }


    // --- HELPER FUNCTION ---
    public float NormaliseFloat(float num)
    {
        if (float.IsNaN(num) || float.IsInfinity(num) ||
            float.IsNegativeInfinity(num) || float.IsPositiveInfinity(num))
            return 0f;
        return num;
    }


    // --- MISC SYNC METHODS ---
    public void SyncEffectsToOtherClients()
    {
        if (syncCarEffectsInterval > 0.02f)
        {
            if (syncedWheelRotation != steeringWheelAnimFloat)
            {
                syncCarEffectsInterval = 0f;
                syncedWheelRotation = steeringWheelAnimFloat;
                syncedSteeringInput = steeringInput;
                syncedPlayerSteeringAnim = playerSteeringWheelAnimFloat;
                SyncEffectsRpc(steeringWheelAnimFloat, steeringInput, playerSteeringWheelAnimFloat);
                return;
            }
        }
        else
        {
            syncCarEffectsInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncEffectsRpc(float wheelRotation, float steerInput, float playerSteering)
    {
        syncedWheelRotation = wheelRotation;
        syncedSteeringInput = steerInput;
        syncedPlayerSteeringAnim = playerSteering;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncVehicleInputRpc(bool gasPressed, bool brakePressed)
    {
        syncedDrivePedalPressed = gasPressed;
        syncedBrakePedalPressed = brakePressed;
    }

    private new void SyncCarPhysicsToOtherClients()
    {
        if (!IsOwner || magnetedToShip || inDropshipAnimation)
            return;

        SetVehicleKinematic(setKinematic: false);
        if (syncCarPositionInterval > 0.12f)
        {
            if (Vector3.Distance(syncedPosition, transform.position) > 0.02f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles, steeringAnimValue);
                return;
            }
            if (Vector3.Angle(transform.forward, syncedRotation * Vector3.forward) > 2f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles, steeringAnimValue);
                return;
            }
        }
        else
        {
            syncCarPositionInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarPositionRpc(Vector3 carPosition, Vector3 carRotation, float steeringInput)
    {
        syncedPosition = carPosition;
        syncedRotation = Quaternion.Euler(carRotation);
        steeringAnimValue = steeringInput;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetTyreStressRpc(float wheelStress, bool wheelSkidding)
    {
        tyreStress = wheelStress;
        wheelSlipping = wheelSkidding;
    }

    public void SyncCarDrivetrain()
    {
        float syncThreshold = 0.14f * averageVelocity.magnitude;
        syncThreshold = Mathf.Clamp(syncThreshold, 0.14f, 0.21f);
        if (syncCarDrivetrainInterval >= syncThreshold)
        {
            float fWheelSyncRPM = NormaliseFloat(Mathf.Round(frontWheelRPM));
            float bWheelSyncRPM = NormaliseFloat(Mathf.Round(backWheelRPM));

            if (syncedFrontWheelRPM != fWheelSyncRPM ||
                syncedBackWheelRPM != bWheelSyncRPM)
            {
                syncCarDrivetrainInterval = 0f;

                syncedFrontWheelRPM = fWheelSyncRPM;
                syncedBackWheelRPM = bWheelSyncRPM;

                syncedWheelRPM = wheelRPM;
                syncedEngineRPM = EngineRPM;

                SyncCarDrivetrainRpc(frontWheelRPM, backWheelRPM, wheelRPM);
                return;
            }
        }
        else
        {
            syncCarDrivetrainInterval += Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarDrivetrainRpc(float frontWheelSpeed, float backWheelSpeed, float wheelSpeed)
    {
        syncedFrontWheelRPM = frontWheelSpeed;
        syncedBackWheelRPM = backWheelSpeed;
        syncedWheelRPM = wheelSpeed;
        syncedEngineRPM = Mathf.Abs(wheelSpeed);
    }

    public void SyncCarWheelTorque()
    {
        if (syncWheelTorqueInterval >= 0.14f)
        {
            float fWheelSyncRPM = Mathf.Round(currentMotorTorque);
            float bWheelSyncRPM = Mathf.Round(currentBrakeTorque);

            if (syncedMotorTorque != fWheelSyncRPM ||
                syncedBrakeTorque != bWheelSyncRPM)
            {
                syncWheelTorqueInterval = 0f;
                syncedMotorTorque = currentMotorTorque;
                syncedBrakeTorque = currentBrakeTorque;
                SyncWheelTorqueRpc(currentMotorTorque, currentBrakeTorque);
                return;
            }
        }
        else
        {
            syncWheelTorqueInterval += Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncWheelTorqueRpc(float motorTorque, float brakeTorque)
    {
        syncedMotorTorque = motorTorque;
        syncedBrakeTorque = brakeTorque;
    }


    // --- LATE UPDATE METHOD ---
    public new void LateUpdate()
    {
        bool inOrbit = magnetedToShip &&
            (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipDoorsEnabled);

        hornAudio.mute = inOrbit;
        engineAudio1.mute = inOrbit;
        engineAudio2.mute = inOrbit;
        carKeyAudio.mute = inOrbit;
        ejectorButtonAudio.mute = inOrbit;
        springAudio.mute = inOrbit;
        reverseWhineAudio.mute = inOrbit;
        verticalColumnAudio.mute = inOrbit;
        rollingAudio.mute = inOrbit;
        skiddingAudio.mute = inOrbit;
        turbulenceAudio.mute = inOrbit;
        hoodFireAudio.mute = inOrbit;
        extremeStressAudio.mute = inOrbit;
        pushAudio.mute = inOrbit;
        radioAudio.mute = inOrbit;
        radioInterference.mute = inOrbit;

        if (currentDriver != null && lastDriver != currentDriver && !magnetedToShip)
            lastDriver = currentDriver;

        if (!inBetaMode && honkingHorn && hornAudio.isPlaying && hornAudio.pitch < 1f)
            hornAudio.Stop();
    }


    // --- COLLISION ---
    public new bool CarReactToObstacle(Vector3 vel, Vector3 position, Vector3 impulse, CarObstacleType type, float obstacleSize = 1f, EnemyAI enemyScript = null!, bool dealDamage = true)
    {
        switch (type)
        {
            case CarObstacleType.Object:
                if (carHP < 10)
                {
                    mainRigidbody.AddForceAtPosition(Vector3.up * torqueForce + vel, position, ForceMode.Impulse);
                }
                else
                {
                    mainRigidbody.AddForceAtPosition((Vector3.up * torqueForce + vel) * 0.5f, position, ForceMode.Impulse);
                }
                CarBump(averageVelocity * 0.7f);
                if (dealDamage)
                {
                    DealPermanentDamage(1, position);
                }
                return true;
            case CarObstacleType.Player:
                PlayCollisionAudio(position, 5, Mathf.Clamp(vel.magnitude / 7f, 0.65f, 1f));
                if (vel.magnitude < 4.25f)
                {
                    mainRigidbody.velocity = Vector3.Normalize(-impulse * 100000000f) * 9f;
                    DealPermanentDamage(1);
                    return true;
                }
                mainRigidbody.AddForceAtPosition(Vector3.up * torqueForce, position, ForceMode.VelocityChange);
                return false;
            case CarObstacleType.Enemy:
                {
                    float enemyHitSpeed;
                    if (obstacleSize <= 1f)
                    {
                        enemyHitSpeed = 1f;
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else if (obstacleSize <= 2f)
                    {
                        enemyHitSpeed = 9f;
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else
                    {
                        enemyHitSpeed = 15f;
                        _ = carReactToPlayerHitMultiplier;
                    }
                    vel = Vector3.Scale(vel, new Vector3(1f, 0f, 1f));
                    mainRigidbody.AddForceAtPosition(Vector3.up * torqueForce, position, ForceMode.VelocityChange);
                    bool result = false;
                    if (vel.magnitude < enemyHitSpeed)
                    {
                        if (obstacleSize <= 1f)
                        {
                            mainRigidbody.AddForce(Vector3.Normalize(-impulse * 1E+09f) * 4f, ForceMode.Impulse);
                            if (vel.magnitude > 1f)
                            {
                                enemyScript.KillEnemyOnOwnerClient();
                            }
                        }
                        else
                        {
                            CarBump(averageVelocity);
                            mainRigidbody.velocity = Vector3.Normalize(-impulse * 100000000f) * 9f;
                            PlayerControllerB playerControllerB = currentDriver ?? currentPassenger;
                            if (vel.magnitude > 2f && dealDamage)
                            {
                                enemyScript.HitEnemyOnLocalClient(2, Vector3.zero, playerControllerB, playHitSFX: true, 331);
                            }
                            result = true;
                        }
                        DealPermanentDamage(2, position);
                    }
                    else
                    {
                        mainRigidbody.AddForce(Vector3.Normalize(-impulse * 1E+09f) * (carReactToPlayerHitMultiplier - 220f), ForceMode.Impulse);
                        if (dealDamage)
                        {
                            DealPermanentDamage(1, position);
                        }
                        enemyScript.KillEnemyOnOwnerClient();
                    }
                    PlayCollisionAudio(position, 5, 1f);
                    return result;
                }
            default:
                return false;
        }
    }

    public new void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner)
            return;

        if (magnetedToShip || !hasBeenSpawned)
            return;

        if (collision.collider.gameObject.layer != 8 && canDestroyTrees || 
            (collision.collider.gameObject.TryGetComponent<TerrainObstacleTrigger>(out _) && !canDestroyTrees))
            return;

        float highestImpulse = 0f;
        int contactCount = collision.GetContacts(contacts);
        Vector3 contactPosition = Vector3.zero;

        for (int i = 0; i < contactCount; i++)
        {
            if (contacts[i].impulse.magnitude > highestImpulse)
            {
                highestImpulse = contacts[i].impulse.magnitude;
            }
            contactPosition += contacts[i].point;
        }

        contactPosition /= (float)contactCount;
        highestImpulse /= Time.fixedDeltaTime;

        if (highestImpulse < minimalBumpForce || averageVelocity.magnitude < 4f)
        {
            if (contactCount > 3 && averageVelocity.magnitude > 2.5f)
            {
                SetInternalStress(0.35f);
                lastStressType = "Scraping";
            }
            return;
        }

        float collisionVolume = 0.5f;
        int bumpSeverity = -1;

        if (averageVelocity.magnitude > 27f)
        {
            if (carHP < 3)
            {
                DestroyCarRpc();
                DestroyCar();
                return;
            }
            DealPermanentDamage(carHP - 2);
        }

        if (highestImpulse > maximumBumpForce && averageVelocity.magnitude > 11f)
        {
            bumpSeverity = 2;
            collisionVolume = Mathf.Clamp((highestImpulse - maximumBumpForce) / 20000f, 0.8f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
            DealPermanentDamage(2);
        }
        else if (highestImpulse > mediumBumpForce && averageVelocity.magnitude > 3f)
        {
            bumpSeverity = 1;
            collisionVolume = Mathf.Clamp((highestImpulse - mediumBumpForce) / (maximumBumpForce - mediumBumpForce), 0.67f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.5f, 1f);
            DealPermanentDamage(1);
        }
        else if (averageVelocity.magnitude > 1.5f)
        {
            bumpSeverity = 0;
            collisionVolume = Mathf.Clamp((highestImpulse - mediumBumpForce) / (maximumBumpForce - mediumBumpForce), 0.25f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.25f, 1f);
        }

        if (bumpSeverity != -1)
        {
            PlayCollisionAudio(contactPosition, bumpSeverity, collisionVolume);
            if (highestImpulse > maximumBumpForce + 10000f && averageVelocity.magnitude > 19f)
            {
                int damageType = UnityEngine.Random.Range(0, 2);
                if (damageType == 0)
                {
                    CarCollisionRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), false);
                    DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), false);
                    BreakWindshield();
                    DealPermanentDamage(2);
                }
                else
                {
                    CarCollisionRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), true);
                    DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), true);
                    DealPermanentDamage(2);
                }
            }
            else
            {
                CarBump(Vector3.ClampMagnitude(-collision.relativeVelocity, 40f));
            }
        }
    }

    public bool IsPlayerSeatedInCar()
    {
        return localPlayerInControl || 
               localPlayerInPassengerSeat;
    }

    public void CarBump(Vector3 vel)
    {
        CarBumpRpc(vel);
        CarBumpLocalClient(vel);
    }

    public void CarBumpLocalClient(Vector3 vel)
    {
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (IsPlayerSeatedInCar() && vel.magnitude > 50f)
        {
            playerController.externalForceAutoFade += vel;
            return;
        }
        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, this))
        {
            return;
        }
        vel = Vector3.ClampMagnitude(vel, 30f);
        playerController.externalForceAutoFade += vel;
    }

    [Rpc(SendTo.NotOwner)]
    public void CarBumpRpc(Vector3 vel)
    {
        CarBumpLocalClient(vel);
    }

    [Rpc(SendTo.NotOwner)]
    public void CarCollisionRpc(Vector3 vel, bool onlyLocalDriver)
    {
        DamagePlayerInVehicle(vel, onlyLocalDriver);
        if (!onlyLocalDriver)
        {
            BreakWindshield();
        }
    }

    public void DamagePlayerInVehicle(Vector3 vel, bool onlyLocalDriver)
    {
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (localPlayerInPassengerSeat && onlyLocalDriver)
        {
            return;
        }
        if (IsPlayerSeatedInCar())
        {
            playerController.KillPlayer(vel, spawnBody: true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
            return;
        }
        if (!VehicleUtils.IsPlayerInTruckBounds(playerController, this))
        {
            return;
        }
        if (playerController.health <= 40)
        {
            playerController.KillPlayer(vel, spawnBody: true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
            return;
        }
        playerController.DamagePlayer(30, hasDamageSFX: true, callRPC: true, CauseOfDeath.Inertia, 0, fallDamage: false, vel);
        playerController.externalForceAutoFade += vel;
    }

    private new void BreakWindshield()
    {
        if (windshieldBroken)
        {
            return;
        }
        windshieldBroken = true;
        windshieldPhysicsCollider.enabled = false;
        windshieldObject.SetActive(value: false);
        glassParticle.Play();
        miscAudio.transform.localPosition = windshieldObject.transform.localPosition;
        miscAudio.PlayOneShot(windshieldBreak);
    }

    public new void PlayCollisionAudio(Vector3 setPosition, int audioType, float setVolume)
    {
        if (Time.realtimeSinceStartup - audio1Time > Time.realtimeSinceStartup - audio2Time)
        {
            bool collisionAudioTime = Time.realtimeSinceStartup - audio1Time >= collisionAudio1.clip.length * 0.8f;
            if (audio1Type <= audioType || collisionAudioTime)
            {
                audio1Time = Time.realtimeSinceStartup;
                audio1Type = audioType;
                collisionAudio1.transform.position = setPosition;
                CarCollisionSFXRpc(collisionAudio1.transform.localPosition, 0, audioType, setVolume);
                PlayRandomClipAndPropertiesFromAudio(collisionAudio1, setVolume, collisionAudioTime, audioType);
            }
        }
        else
        {
            bool audioTime = Time.realtimeSinceStartup - audio2Time >= collisionAudio2.clip.length * 0.8f;
            if (audio1Type <= audioType || audioTime)
            {
                audio2Time = Time.realtimeSinceStartup;
                audio2Type = audioType;
                collisionAudio2.transform.position = setPosition;
                CarCollisionSFXRpc(collisionAudio2.transform.localPosition, 1, audioType, setVolume);
                PlayRandomClipAndPropertiesFromAudio(collisionAudio2, setVolume, audioTime, audioType);
            }
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarCollisionSFXRpc(Vector3 audioPosition, int audio, int audioType, float vol)
    {
        AudioSource audioSource;
        if (audio == 0)
        {
            audioSource = collisionAudio1;
        }
        else
        {
            audioSource = collisionAudio2;
        }
        bool audioFinished = audioSource.clip.length - audioSource.time < 0.2f;
        audioSource.transform.localPosition = audioPosition;
        PlayRandomClipAndPropertiesFromAudio(audioSource, vol, audioFinished, audioType);
    }

    private new void PlayRandomClipAndPropertiesFromAudio(AudioSource source, float volume, bool isAudioFinished, int collisionType)
    {
        if (!isAudioFinished)
        {
            source.Stop();
        }
        AudioClip[] selectedClips;
        switch (collisionType)
        {
            case 0:
                selectedClips = minCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 0.4f, 2f);
                break;
            case 1:
                selectedClips = medCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 0.75f, 2f);
                break;
            case 2:
                selectedClips = maxCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 1.4f, 2f);
                break;
            default:
                selectedClips = obstacleCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 0.75f, 2f);
                break;
        }
        AudioClip chosenClip = selectedClips[UnityEngine.Random.Range(0, selectedClips.Length)];
        if (chosenClip == source.clip && UnityEngine.Random.Range(0, 10) <= 5)
        {
            chosenClip = selectedClips[UnityEngine.Random.Range(0, selectedClips.Length)];
        }
        if (isAudioFinished)
        {
            source.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        }
        source.clip = chosenClip;
        source.PlayOneShot(chosenClip, volume);
        int collisionNoiseId = inBetaMode ? 106217 : 2692;
        if (collisionType >= 2)
        {
            RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 18f + volume * 7f, 0.6f, 0, noiseIsInsideClosedShip: false, collisionNoiseId);
        }
        else if (collisionType >= 1)
        {
            RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 12f + volume * 7f, 0.6f, 0, noiseIsInsideClosedShip: false, collisionNoiseId);
        }
        if (collisionType == -1)
        {
            selectedClips = minCollisions;
            chosenClip = selectedClips[UnityEngine.Random.Range(0, selectedClips.Length)];
            source.PlayOneShot(chosenClip);
        }
    }

    public new void SetInternalStress(float carStressIncrease = 0f)
    {
        if (!IsOwner || carDestroyed)
        {
            return;
        }

        if (carStressIncrease <= 0f) carStressChange = Mathf.Clamp(carStressChange - Time.fixedDeltaTime, -0.25f, 0.5f);
        else carStressChange = Mathf.Clamp(carStressChange + Time.fixedDeltaTime * carStressIncrease, 0f, 10f);

        underExtremeStress = (carStressIncrease >= 1f);
        carStress = Mathf.Clamp(carStress + carStressChange, 0f, 100f);

        if (carStress > 7f)
        {
            carStress = 0f;
            DealPermanentDamage(2);
            lastDamageType = "Stress";
        }
    }

    public new void DealPermanentDamage(int damageAmount, Vector3 damagePosition = default(Vector3))
    {
        if (!IsOwner || (magnetedToShip && !(drivePedalPressed && gear == CarGearShift.Park)) || carDestroyed)
        {
            return;
        }

        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP -= damageAmount;
        syncedCarHP = carHP;
        if (carHP <= 0)
        {
            DealDamageRpc(carHP);
            DestroyCarRpc();
            DestroyCar();
        }
        else
        {
            DealDamageRpc(carHP);
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void DealDamageRpc(int carHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP = carHealth;
        syncedCarHP = carHP;
    }


    // --- DESTRUCTION ---
    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void DestroyCarRpc()
    {
        DestroyCar();
    }

    public new void DestroyCar()
    {
        if (carDestroyed)
            return;

        carDestroyed = true;
        UnMagnetCar();
        StopAudiosPlayback();
        RemoveRainCollision();
        CollectItemsInTruck();
        StopParticleVFX();
        SetDestroyedMaterials();
        BreakWindshield();

        RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 20f, 0.8f, 0, noiseIsInsideClosedShip: false, 106217);

        DisableWheelCollider(FrontLeftWheel);
        DisableWheelCollider(FrontRightWheel);
        DisableWheelCollider(BackLeftWheel);
        DisableWheelCollider(BackRightWheel);

        DisableObjectsOnDestroy();
        SetPositionsAndEnableOnDestroy();
        destroyedTruckMeshAlt.SetActive(isInteriorRHD);
        destroyedTruckMesh.SetActive(!isInteriorRHD);

        SetExplosionForce(forceMultiplier: 1560f, explosionPos: hoodFireAudio.transform.position);

        DisableIgnition();
        DisableDrivetrain();

        ResetControl();
        KillOccupants();

        SetInteractions();

        ResetOccupants();

        Landmine.SpawnExplosion(transform.position + transform.forward + Vector3.up * 1.5f, spawnExplosionEffect: true, 6f, 10f, 30, 200f, truckDestroyedExplosion, goThroughCar: true);
    }

    private void UnMagnetCar()
    {
        if (!magnetedToShip || StartOfRound.Instance.attachedVehicle != this)
            return;

        magnetedToShip = false;
        StartOfRound.Instance.attachedVehicle = null;
        StartOfRound.Instance.isObjectAttachedToMagnet = false;
        CollectItemsInTruck();
    }

    private void StopAudiosPlayback()
    {
        underExtremeStress = false;
        engineAudio1.Stop();
        engineAudio2.Stop();
        turbulenceAudio.Stop();
        pushAudio.Stop();
        miscAudio.Stop();
        steeringWheelAudio.Stop();
        gearStickAudio.Stop();
        rollingAudio.Stop();
        radioAudio.Stop();
        radioInterference.Stop();
        extremeStressAudio.Stop();
        carKeyAudio.Stop();
        honkingHorn = false;
        hornAudio.Stop();
        skiddingAudio.Stop();
        turboBoostAudio.Stop();
    }

    private void StopParticleVFX()
    {
        frontTireSparks.Stop();
        tireSparks.Stop();
        turboBoostParticle.Stop();
    }

    private void SetDestroyedMaterials()
    {
        frontLeftDoorMeshLOD.material = destroyedTruckMaterial;
        frontRightDoorMeshLOD.material = destroyedTruckMaterial;
        frontLeftDoorMesh.material = destroyedTruckMaterial;
        frontRightDoorMesh.material = destroyedTruckMaterial;
        steeringWheelMesh.material = destroyedTruckMaterial;
    }

    private void DisableWheelCollider(WheelCollider wheelCollider)
    {
        if (wheelCollider == null || !wheelCollider.enabled)
            return;

        wheelCollider.motorTorque = 0f;
        wheelCollider.brakeTorque = 0f;
        wheelCollider.enabled = false;
    }

    private void DisableObjectsOnDestroy()
    {
        for (int obj = 0; obj < disableOnDestroy.Length; obj++)
        {
            if (!disableOnDestroy[obj].activeSelf)
                continue;
            disableOnDestroy[obj].SetActive(false);
        }
        radarMapIcon.gameObject.SetActive(false);
        mainBodyContainer.SetActive(false);
        hoodDoorContainer.SetActive(false);
        backDoorContainer.SetActive(false);

        headlightsContainer.SetActive(false);
        backLightsContainer.SetActive(false);
        healthMeter.gameObject.SetActive(false);
    }

    private void SetPositionsAndEnableOnDestroy()
    {
        radarMapDestroyedIcon.gameObject.SetActive(true);

        backLightsOn = false;
        backLightsMesh.transform.localPosition = new Vector3(0.033f, 0f, 0.081f);
        backLightsMesh.transform.localRotation = Quaternion.Euler(0f, 3.881f, 0f);
        backLightsMesh.material = headlightsOffMat;

        frontCabinLightContainer.transform.localPosition = new Vector3(0f, -0.239f, 0f);

        frontCabinLightMesh.transform.localPosition = new Vector3(-0.662f, 0.073f, -0.222f);
        frontCabinLightMesh.transform.localRotation = Quaternion.Euler(0f, 11.641f, 0f);

        frontCabinLightMesh.gameObject.SetActive(true);
        frontCabinLightMesh.enabled = true;

        steeringWheelAnimator.transform.localEulerAngles = new Vector3(-141.303f, 0f, 90f);
        if (isInteriorRHD)
        {
            steeringWheelAnimator.transform.localPosition = new Vector3(0.97943f, 0.101f, 2.363f);
            return;
        }
        steeringWheelAnimator.transform.localPosition = new Vector3(-0.97943f, 0.101f, 2.363f);
    }

    private void SetExplosionForce(float forceMultiplier, Vector3 explosionPos)
    {
        mainRigidbody.ResetCenterOfMass();
        mainRigidbody.AddForceAtPosition(Vector3.up * forceMultiplier, explosionPos - Vector3.up, ForceMode.Impulse);
    }

    private void DisableIgnition()
    {
        CancelIgnitionCoroutine();
        ignitionStarted = false;
        if (carExhaustParticle.isEmitting) carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        keyIsInIgnition = false;
        keyIsInDriverHand = false;
    }

    private void DisableDrivetrain()
    {
        EngineRPM = 0f;
        frontWheelRPM = 0f;
        backWheelRPM = 0f;
        wheelRPM = 0f;
        wheelsRPM = 0f;
    }

    private void ResetControl()
    {
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
        moveInputVector = Vector2.zero;
    }

    private void KillOccupants()
    {
        if (!localPlayerInControl && !localPlayerInPassengerSeat)
            return;
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        playerController.KillPlayer(Vector3.up * 27f + 20f * UnityEngine.Random.insideUnitSphere, spawnBody: true, CauseOfDeath.Blast, 6, Vector3.up * 1.5f, false);
    }

    private void SetInteractions()
    {
        InteractTrigger[] interactTriggers = gameObject.GetComponentsInChildren<InteractTrigger>();
        for (int i = 0; i < interactTriggers.Length; i++)
        {
            interactTriggers[i].interactable = false;
            interactTriggers[i].CancelAnimationExternally();
        }
        driverSideDoorTrigger.interactable = true;
        passengerSideDoorTrigger.interactable = true;
        pushTruckTrigger.interactable = false;
    }

    private void ResetOccupants()
    {
        currentDriver = null!;
        currentPassenger = null!;
    }


    // --- REMOVAL MISC ---
    public void RemoveRainCollision()
    {
        var particleTriggers = new[]
        {
            ScandalsTweaks.Utils.References.rainParticles,
            ScandalsTweaks.Utils.References.rainHitParticles,
            ScandalsTweaks.Utils.References.stormyRainParticles,
            ScandalsTweaks.Utils.References.stormyRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneSandParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainHitParticles
        };

        foreach (var particle in particleTriggers)
        {
            if (particle == null)
            {
                Plugin.LogDebug("Weather particle (or trigger) is null!");
                continue;
            }

            var trigger = particle.trigger;
            for (int j = trigger.colliderCount - 1; j >= 0; j--)
            {
                var collider = (Collider)trigger.GetCollider(j);
                if (weatherEffectBlockers.Contains(collider))
                {
                    trigger.RemoveCollider(j);
                }
            }
        }
    }


    // --- MISC DAMAGE ---
    private new void ReactToDamage()
    {
        healthMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            healthMeter.localScale.z,
            Mathf.Clamp((float)carHP / (float)baseCarHP, 0.01f, 1f),
            6f * Time.deltaTime));
        turboMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            turboMeter.localScale.z,
            Mathf.Clamp((float)turboBoosts / 5f, 0.01f, 1f),
            6f * Time.deltaTime));

        if (!IsOwner)
            return;

        if (carHP < 7 && Time.realtimeSinceStartup - timeAtLastDamage > 16f)
        {
            timeAtLastDamage = Time.realtimeSinceStartup;
            carHP++;
            syncedCarHP = carHP;
            SyncCarHealthRpc(carHP);
        }
        if (carHP < 3)
        {
            if (!isHoodOnFire)
                SetHoodFireAndSync(setOnFire: true);
        }
        else if (isHoodOnFire)
        {
            SetHoodFireAndSync(setOnFire: false);
        }
    }


    // --- DAMAGE/HEALTH SYNC ---
    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    private void SyncCarHealthRpc(int carHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        syncedCarHP = carHealth;
        carHP = syncedCarHP;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncExtremeStressRpc(bool underStress)
    {
        if (carDestroyed)
        {
            underExtremeStress = false;
        }
        else
        {
            underExtremeStress = underStress;
        }
    }


    // --- HOOD FIRE VFX ---
    private void SetHoodFireAndSync(bool setOnFire)
    {
        SetHoodOnFireLocalClient(setOnFire);
        SetHoodOnFireRpc(setOnFire);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetHoodOnFireRpc(bool onFire)
    {
        SetHoodOnFireLocalClient(onFire);
    }

    private void SetHoodOnFireLocalClient(bool setOnFire)
    {
        isHoodOnFire = setOnFire;
        if (setOnFire)
        {
            hoodFireAudio.Play();
            hoodFireParticle.Play();
            if (!carHoodOpen && !carDestroyed) SetHoodOpenLocalClient(setOpen: true);
            return;
        }
        hoodFireAudio.Stop();
        hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
    }


    // --- HOOD INTERACTION ---
    public new void ToggleHoodOpenLocalClient()
    {
        carHoodOpen = !carHoodOpen;
        carHoodAnimator.SetBool("hoodOpen", carHoodOpen);
        SetHoodOpenRpc(open: carHoodOpen);
    }

    // used for when the hood is 'on fire'
    public new void SetHoodOpenLocalClient(bool setOpen)
    {
        if (carHoodOpen && carHoodOpen == setOpen)
            return;

        carHoodOpen = setOpen;
        carHoodAnimator.SetBool("hoodOpen", setOpen);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHoodOpenRpc(bool open)
    {
        carHoodOpen = open;
        carHoodAnimator.SetBool("hoodOpen", carHoodOpen);
    }


    // --- PUSH METHODS ---
    public new void PushTruckWithArms()
    {
        if (magnetedToShip)
            return;

        if (GameNetworkManager.Instance.localPlayerController.overridePhysicsParent != null)
            return;

        if (!Physics.Raycast(
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position,
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward,
            out hit,
            10f,
            1073742656,
            QueryTriggerInteraction.Ignore))
            return;

        Vector3 point = hit.point;
        Vector3 forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;

        tempPushTransform.position = point;
        Vector3 hitPoint = tempPushTransform.localPosition;

        float turbulence = Mathf.Min(turbulenceAmount + 0.5f, 2f);

        if (IsOwner)
        {
            PlayPushAudio(hitPoint, turbulence);
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(forward * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, tempPushTransform.position - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
            PushTruckFromOwnerRpc(hitPoint, turbulence);
            return;
        }
        PushTruckRpc(hitPoint, forward, turbulence);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushTruckRpc(Vector3 pushPosition, Vector3 dir, float turbulence)
    {
        PlayPushAudio(pushPosition, turbulence);
        if (IsOwner)
        {
            tempPushTransform.localPosition = pushPosition;
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(dir * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, tempPushTransform.position - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushTruckFromOwnerRpc(Vector3 pushPosition, float turbulence)
    {
        PlayPushAudio(pushPosition, turbulence);
    }

    public void PlayPushAudio(Vector3 pos, float turbulence)
    {
        pushAudio.transform.localPosition = pos;
        pushAudio.Play();
        turbulenceAmount = turbulence;
    }


    // --- INTERIOR BUTTON ANIMATIONS ---
    public void UseButtonOnLocalClient(string triggerString)
    {
        verticalColumnAnimator.SetTrigger(triggerString);
    }


    public void ToggleWipersOnLocalClient()
    {
        verticalColumnAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickWiperButton");
        ToggleWipersRpc();
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ToggleWipersRpc()
    {
        verticalColumnAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickWiperButton");
    }


    public void OpenCabinWindowOnLocalClient()
    {
        verticalColumnAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickCabinButton");
        OpenCabinWindowRpc();
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OpenCabinWindowRpc()
    {
        verticalColumnAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickCabinButton");
    }


    // --- HEADLAMPS ---
    public new void ToggleHeadlightsLocalClient()
    {
        headlightsContainer.SetActive(!headlightsContainer.activeSelf);
        verticalColumnAudio.PlayOneShot(headlightsToggleSFX);
        SetHeadlightMaterial(headlightsContainer.activeSelf);
        UseButtonOnLocalClient("clickLightButton");
        ToggleHeadlightsRpc(headlightsContainer.activeSelf);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ToggleHeadlightsRpc(bool setLightsOn)
    {
        headlightsContainer.SetActive(setLightsOn);
        verticalColumnAudio.PlayOneShot(headlightsToggleSFX);
        SetHeadlightMaterial(setLightsOn);
        UseButtonOnLocalClient("clickLightButton");
    }

    public new void SetHeadlightMaterial(bool on)
    {
        Material[] bodyMat = mainBodyMesh.sharedMaterials;
        bodyMat[1] = (on ? headlightsOnMat : headlightsOffMat);
        mainBodyMesh.sharedMaterials = bodyMat;

        bodyMat = lod1Mesh.sharedMaterials;
        bodyMat[1] = (on ? headlightsOnMat : headlightsOffMat);
        lod1Mesh.sharedMaterials = bodyMat;

        bodyMat = lod2Mesh.sharedMaterials;
        bodyMat[1] = (on ? headlightsOnMat : headlightsOffMat);
        lod2Mesh.sharedMaterials = bodyMat;
    }


    // --- EJECTOR SEAT ---
    public new void SpringDriverSeatLocalClient()
    {
        if (Time.realtimeSinceStartup - timeSinceSpringingDriverSeat < 3f)
            return;

        timeSinceSpringingDriverSeat = Time.realtimeSinceStartup;
        SpringDriverSeatRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SpringDriverSeatRpc()
    {
        timeSinceSpringingDriverSeat = Time.realtimeSinceStartup;
        ejectorButtonAudio.PlayOneShot(dashboardButton);
        driverSeatSpringAnimator.SetTrigger("spring");
        ejectorButtonAnimator.SetTrigger("press");
        springAudio.Play();
        RoundManager.Instance.PlayAudibleNoise(springAudio.transform.position, 30f, 1f, 0, noiseIsInsideClosedShip: false, 106217); // 2692

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        if (!player.isPlayerControlled || 
            player.isPlayerDead)
            return;

        if (localPlayerInControl ||
            Vector3.Distance(player.transform.position, springAudio.transform.position) < 0.9f) //|| Vector3.Distance(player.transform.localPosition, springAudio.transform.localPosition) < 1f
        {
            player.externalForceAutoFade += (transform.up * springForce);
        }
    }
}