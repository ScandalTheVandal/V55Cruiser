using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering.HighDefinition;
using v55Cruiser;
using v55Cruiser.Behaviour;
using v55Cruiser.Patches;
using v55Cruiser.Utils;

public class v55VehicleController : VehicleController
{
    [Header("Variety")]
    public TruckVersionType truckType;

    public v55InteriorType LHDInterior = null!;
    public v55InteriorType RHDInterior = null!;
    public v55InteriorType currentInterior = null!;

    public Transform v55HealthMeter = null!;
    public Transform v56HealthMeter = null!;

    public GameObject v55EngineBay = null!;
    public GameObject v56EngineBay = null!;

    public int interiorType;
    public bool isInteriorRHD;

    [Header("Vehicle Physics")]
    public List<WheelCollider> wheels = null!;
    public AnimationCurve steeringWheelCurve = null!;
    public v55VehicleCollisionTrigger collisionTrigger = null!;
    public Rigidbody playerPhysicsBody = null!;
    public Vector3 previousVehiclePosition;

    private WheelHit[] wheelHits = new WheelHit[4];
    public int syncedCarHP;
    public float sidewaysSlip;
    public float forwardsSlip;
    public float wheelTorque;
    public float wheelBrakeTorque;
    public bool hasDeliveredVehicle;
    public float maxBrakingPower = 2000f;
    public float syncedPlayerSteeringAnim;
    public bool syncedDrivePedalPressed;
    public bool syncedBrakePedalPressed;
    public float wheelRPM;

    [Header("Drivetrain")]
    public float diffRatio;
    public float forwardWheelSpeed;
    public float reverseWheelSpeed;

    [Header("Multiplayer")]
    public Collider vehicleBounds = null!;
    public Collider storageCompartment = null!;
    public PlayerControllerB playerWhoShifted = null!;

    public Vector3 playerPositionOffset;
    public Vector3 seatNodePositionOffset;
    public bool liftGateOpen;
    public float syncedWheelRotation;
    public float syncedEngineRPM;
    public float syncedWheelRPM;
    public float syncedMotorTorque;
    public float syncedBrakeTorque;
    public float tyreStress;
    public bool wheelSlipping;
    public float syncCarEffectsInterval;
    public float syncWheelTorqueInterval;
    public float syncCarDrivetrainInterval;
    public float syncCarWheelSpeedInterval;

    [Header("Effects")]
    public GameObject[] disableOnDestroy = null!;
    public InteractTrigger pushTruckTrigger = null!;
    public Collider[] weatherEffectBlockers = null!;

    public MeshRenderer leftBrakeMesh = null!;
    public MeshRenderer rightBrakeMesh = null!;
    public MeshRenderer backLeftBrakeMesh = null!;
    public MeshRenderer backRightBrakeMesh = null!;

    public AnimationCurve engineAudio1Curve = null!;
    public AnimationCurve engineAudio2Curve = null!;

    public GameObject leftWheelSteeringAxis = null!;
    public GameObject leftWheelAxis = null!;
    public GameObject rightWheelSteeringAxis = null!;
    public GameObject rightWheelAxis = null!;
    public GameObject backLeftWheelAxis = null!;
    public GameObject backRightWheelAxis = null!;
    public GameObject destroyedTruckMesh_1 = null!;
    public GameObject windshieldMesh = null!;
    public GameObject carKeyInHand = null!;

    public MeshRenderer radarMapIcon = null!;
    public MeshRenderer radarMapDestroyedIcon = null!;

    // ignition key stuff
    private Transform currentDriver_LHand_transform = null!;
    private Vector3 LHD_Pos_Local = new Vector3(0.0489f, 0.1371f, -0.1566f);
    private Vector3 LHD_Pos_Server = new Vector3(0.0366f, 0.1023f, -0.1088f);
    private Vector3 LHD_Rot_Local = new Vector3(-3.446f, 3.193f, 172.642f);
    private Vector3 LHD_Rot_Server = new Vector3(-191.643f, 174.051f, -7.768005f);

    private Vector3 RHD_Pos_Local = new Vector3(-0.01708288f, 0.1665026f, -0.1157278f);
    private Vector3 RHD_Pos_Server = new Vector3(-0.0194879f, 0.1340649f, -0.1071167f);
    private Vector3 RHD_Rot_Local = new Vector3(21.592f, -11.63f, -158.578f);
    private Vector3 RHD_Rot_Server = new Vector3(-9.158f, -18.16f, -162.445f);

    public bool correctedPosition;
    public bool twistingKey;
    public bool isCabLightOn;

    public float playerSteeringWheelAnimFloat;
    public float ignitionRotSpeed = 45f;

    [Header("Audio")]
    public AudioSource[] allVehicleAudios = null!;

    public AudioSource reverseWhine = null!;
    public AudioClip dashboardButton = null!;
    public AudioClip revEngineStart1 = null!;

    [Header("Radio")]
    public float timeLastSyncedRadio;
    public float radioPingTimestamp;

    [Header("Materials")]
    public Material greyLightOffMat = null!;
    public Material redLightOffMat = null!;


    // --- INIT ---
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (StartOfRound.Instance.inShipPhase ||
            !IsServer)
            return;

        int interiorVariant = new System.Random(StartOfRound.Instance.randomMapSeed).Next(0, 2);
        SetInteriorTypeRpc(interiorVariant);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetInteriorTypeRpc(int carInteriorType)
    {
        SetInteriorType(carInteriorType);
    }

    public void SetInteriorType(int carInteriorType)
    {
        interiorType = carInteriorType;
        v55InteriorType type;

        switch (interiorType)
        {
            case 0:
                windwiperPhysicsBody1.transform.localScale = new Vector3(0.98f, 0.98f, 0.98f);
                windwiperPhysicsBody2.transform.localScale = new Vector3(0.98f, 0.98f, 0.98f);
                currentInterior = LHDInterior;
                isInteriorRHD = false;
                RHDInterior.gameObject.SetActive(false);
                break;
            case 1:
                windwiperPhysicsBody1.transform.localScale = new Vector3 (-0.98f, 0.98f, 0.98f);
                windwiperPhysicsBody2.transform.localScale = new Vector3(-0.98f, 0.98f, 0.98f);
                currentInterior = RHDInterior;
                isInteriorRHD = true;
                LHDInterior.gameObject.SetActive(false);
                break;
        }
        // assign variables n' shit
        type = currentInterior;
        gearStickAnimator = type.gearStickAnimator;
        steeringWheelAnimator = type.steeringWheelAnimator;

        startKeyIgnitionTrigger = type.startKeyIgnitionTrigger;
        removeKeyIgnitionTrigger = type.removeKeyIgnitionTrigger;

        ignitionNotTurnedPosition = type.ignitionNotTurnedPosition;
        ignitionTurnedPosition = type.ignitionTurnedPosition;

        driverSeatTrigger = type.driverSeatTrigger;
        passengerSeatTrigger = type.passengerSeatTrigger;

        driverSeatSpringAnimator = type.driverSeatSpringAnimator;
        springAudio = type.springAudio;

        if (StartOfRound.Instance.inShipPhase)
            hasBeenSpawned = true;
    }


    public new void Awake()
    {
        // allow mods like ButteryFixes, YesFox and WeedKillerFixes that
        // all cache VehicleController will be compatible with our vehicle
        FixAllAudios(allVehicleAudios);
        ragdollPhysicsBody.interpolation = RigidbodyInterpolation.Interpolate;
        windwiperPhysicsBody1.interpolation = RigidbodyInterpolation.Interpolate;
        windwiperPhysicsBody2.interpolation = RigidbodyInterpolation.Interpolate;
        playerPhysicsBody.interpolation = RigidbodyInterpolation.Interpolate;
        base.Awake();
        playerPhysicsBody.transform.SetParent(RoundManager.Instance.VehiclesContainer);
        References.truckController = this; // optimisation
        wheels = new List<WheelCollider> {
            FrontLeftWheel,
            FrontRightWheel,
            BackLeftWheel,
            BackRightWheel };

        physicsRegion.priority = 1;
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;

        seatNodePositionOffset = Vector3.zero;
        playerPositionOffset = Vector3.zero;

        // set interior offsets
        LHDInterior.driverSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;
        LHDInterior.passengerSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;
        RHDInterior.driverSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;
        RHDInterior.passengerSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;

        backDoorOpen = true; // hacky shit
        forwardWheelSpeed = 5000f;
        reverseWheelSpeed = -3500f;
        SetTruckStats();
    }

    public void FixAllAudios(AudioSource[] audiosArray)
    {
        // apply the actual SFX sound mixer so TZP effects and whatnot will work (subtle, but nice to have)
        foreach (AudioSource audio in audiosArray)
        {
            audio.outputAudioMixerGroup = References.diageticSFXGroup;
        }
    }

    private void SetTruckStats()
    {
        // drivetrain
        gear = CarGearShift.Park;
        MaxEngineRPM = 300f;
        MinEngineRPM = 100f;
        engineIntensityPercentage = 180f;
        EngineTorque = 1100f;
        carAcceleration = 350f;
        idleSpeed = 15f;

        // physics
        mainRigidbody.automaticCenterOfMass = false;
        mainRigidbody.centerOfMass = new Vector3(0f, -0.655f, -1.1173f);
        mainRigidbody.automaticInertiaTensor = false;

        carMaxSpeed = 60f;
        mainRigidbody.maxLinearVelocity = carMaxSpeed;
        mainRigidbody.maxAngularVelocity = 4f;
        brakeSpeed = 2000f;
        pushForceMultiplier = 27f;
        pushVerticalOffsetAmount = 1f;
        steeringWheelTurnSpeed = 4f;
        torqueForce = 2.5f;

        SetWheelFriction();

        JointSpring suspensionSpring = new JointSpring
        {
            spring = 40000f,
            damper = 750f,
            targetPosition = 0.88f,
        };

        FrontLeftWheel.wheelDampingRate = 0.7f;
        FrontRightWheel.wheelDampingRate = 0.7f;
        BackRightWheel.wheelDampingRate = 0.7f;
        BackLeftWheel.wheelDampingRate = 0.7f;

        FrontLeftWheel.suspensionSpring = suspensionSpring;
        FrontRightWheel.suspensionSpring = suspensionSpring;
        BackRightWheel.suspensionSpring = suspensionSpring;
        BackLeftWheel.suspensionSpring = suspensionSpring;

        FrontLeftWheel.suspensionDistance = 0.4f;
        FrontRightWheel.suspensionDistance = 0.4f;
        BackRightWheel.suspensionDistance = 0.4f;
        BackLeftWheel.suspensionDistance = 0.4f;

        FrontLeftWheel.mass = 25f;
        FrontRightWheel.mass = 25f;
        BackLeftWheel.mass = 25f;
        BackRightWheel.mass = 25f;

        FrontLeftWheel.sprungMass = 54.4f;
        FrontRightWheel.sprungMass = 54.4f;
        BackLeftWheel.sprungMass = 147.4f;
        BackRightWheel.sprungMass = 147.4f;

        // boost ability
        turboBoostForce = 3000f;
        turboBoostUpwardForce = 7200f;
        jumpForce = 600f;

        // health
        baseCarHP = 30;
        carHP = baseCarHP;
        syncedCarHP = carHP;
        carFragility = 1f;

        truckType = TruckVersionType.V55;

        v55EngineBay.SetActive(truckType == TruckVersionType.V55);
        v56EngineBay.SetActive(truckType != TruckVersionType.V55);

        v55HealthMeter.gameObject.SetActive(truckType == TruckVersionType.V55);
        v56HealthMeter.gameObject.SetActive(truckType != TruckVersionType.V55);
        turboMeter.gameObject.SetActive(truckType != TruckVersionType.V55);

        if (truckType == TruckVersionType.V55)
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
        FrontRightWheel.forwardFriction = wheelFrictionCurve;
        FrontLeftWheel.forwardFriction = wheelFrictionCurve;
        wheelFrictionCurve.stiffness = 0.75f;
        BackRightWheel.forwardFriction = wheelFrictionCurve;
        BackLeftWheel.forwardFriction = wheelFrictionCurve;
        wheelFrictionCurve.stiffness = 0.8f;
        wheelFrictionCurve.asymptoteValue = 0.75f;
        wheelFrictionCurve.extremumSlip = 0.7f;
        FrontRightWheel.sidewaysFriction = wheelFrictionCurve;
        FrontLeftWheel.sidewaysFriction = wheelFrictionCurve;
        BackRightWheel.sidewaysFriction = wheelFrictionCurve;
        BackLeftWheel.sidewaysFriction = wheelFrictionCurve;

        //WheelFrictionCurve forwardFrictionCurve = new WheelFrictionCurve
        //{
        //    extremumSlip = 0.6f, //1
        //    extremumValue = 1f, //0.2 //0.3
        //    asymptoteSlip = 0.8f, //0.8
        //    asymptoteValue = 0.5f, //0.4
        //    stiffness = 1f, //2.7
        //};
        //FrontRightWheel.forwardFriction = forwardFrictionCurve;
        //FrontLeftWheel.forwardFriction = forwardFrictionCurve;
        //BackRightWheel.forwardFriction = forwardFrictionCurve;
        //BackLeftWheel.forwardFriction = forwardFrictionCurve;
        //WheelFrictionCurve sidewaysFrictionCurve = new WheelFrictionCurve
        //{
        //    extremumSlip = 0.7f,
        //    extremumValue = 1f,
        //    asymptoteSlip = 0.8f,
        //    asymptoteValue = 0.75f,
        //    stiffness = 0.8f,
        //};
        //FrontRightWheel.sidewaysFriction = sidewaysFrictionCurve;
        //FrontLeftWheel.sidewaysFriction = sidewaysFrictionCurve;
        //BackRightWheel.sidewaysFriction = sidewaysFrictionCurve;
        //BackLeftWheel.sidewaysFriction = sidewaysFrictionCurve;
    }

    public new void Start()
    {
        SetCarRainCollisions();
        FrontLeftWheel.brakeTorque = maxBrakingPower;
        FrontRightWheel.brakeTorque = maxBrakingPower;
        BackLeftWheel.brakeTorque = maxBrakingPower;
        BackRightWheel.brakeTorque = maxBrakingPower;

        currentRadioClip = new System.Random(StartOfRound.Instance.randomMapSeed).Next(0, radioClips.Length);
        radioAudio.clip = radioClips[currentRadioClip];
        decals = new DecalProjector[24];

        if (!StartOfRound.Instance.inShipPhase)
            return;

        magnetedToShip = true;
        loadedVehicleFromSave = true;
        hasDeliveredVehicle = true;
        inDropshipAnimation = false;

        transform.position = StartOfRound.Instance.magnetPoint.position + StartOfRound.Instance.magnetPoint.forward * 7f;
        transform.rotation = Quaternion.Euler(new(0f, 90f, 0f));

        StartMagneting();
    }

    public void SetCarRainCollisions()
    {
        var particleTriggers = new[]
        {
           ScandalsTweaks.Utils.References.rainParticles.trigger,
           ScandalsTweaks.Utils.References.rainHitParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainHitParticles.trigger
        };

        if (particleTriggers == null)
        {
            Plugin.Logger.LogError("rain particles are null! this will cause issues!");
            return;
        }
        for (int i = 0; i < particleTriggers.Length; i++)
        {
            for (int j = 0; j < weatherEffectBlockers.Length; j++)
            {
                int index = particleTriggers[i].colliderCount + j;
                particleTriggers[i].SetCollider(index, weatherEffectBlockers[j]);
            }
        }
    }


    // --- SYNC DATA ---
    public void SendClientSyncData()
    {
        if (interiorType == -1)
        {
            interiorType = 0;
        }
        SyncClientDataRpc(interiorType);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SyncClientDataRpc(int carInteriorType)
    {
        SetInteriorType(carInteriorType);
    }


    // --- STORAGE DOOR ---
    public new void SetBackDoorOpen(bool open)
    {
        RoundManager.Instance.PlayAudibleNoise(backDoorContainer.transform.position, 21f, 0.9f, 0, noiseIsInsideClosedShip: false, 2692);
        liftGateOpen = open;
    }


    // --- CAB LIGHTING ---
    private new void SetFrontCabinLightOn(bool setOn)
    {
        isCabLightOn = setOn;
        Material cabinLightMat = setOn ? headlightsOnMat : greyLightOffMat;
        frontCabinLightContainer.SetActive(setOn);
        frontCabinLightMesh.material = cabinLightMat;
    }


    // --- TRY IGNITION METHOD ---
    public new void StartTryCarIgnition()
    {
        if (!localPlayerInControl ||
            ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: true));
        TryIgnitionRpc(keyIsInIgnition);
    }

    private new IEnumerator TryIgnition(bool isLocalDriver)
    {
        if (keyIsInIgnition)
        {
            //if (isLocalDriver)
            //{
            //    if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 3)
            //        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 2);
            //    else
            //        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 12);
            //}
            if (currentDriver?.playerBodyAnimator.GetInteger("SA_CarAnim") == 3)
                currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 2);
            else
                currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 12);

            yield return new WaitForSeconds(isInteriorRHD ? 0.047f : 0.02f);
            currentInterior.carKeySounds.PlayOneShot(twistKey);
            RoundManager.Instance.PlayAudibleNoise(currentInterior.carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
            SetKeyIgnitionValues(twisting: true, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.1467f);
        }
        else
        {
            //if (isLocalDriver)
            //    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 2);
            currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 2);

            SetKeyIgnitionValues(twisting: false, keyInHand: true, keyInSlot: false);
            yield return new WaitForSeconds(0.6f);
            currentInterior.carKeySounds.PlayOneShot(insertKey);
            RoundManager.Instance.PlayAudibleNoise(currentInterior.carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
            SetKeyIgnitionValues(twisting: false, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.2f + (isInteriorRHD ? 0.097f : 0f));
            currentInterior.carKeySounds.PlayOneShot(twistKey);
            RoundManager.Instance.PlayAudibleNoise(currentInterior.carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
            SetKeyIgnitionValues(twisting: true, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.185f);
        }
        SetKeyIgnitionValues(twisting: true, keyInHand: true, keyInSlot: true);
        if (!isLocalDriver) yield break;
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        engineAudio1.Stop();
        engineAudio1.clip = revEngineStart1;
        engineAudio1.volume = 0.7f;
        if (engineAudio1.clip == revEngineStart1)
            engineAudio1.PlayOneShot(engineRev);
        carEngine1AudioActive = true;
        if (engineAudio1.clip == revEngineStart1)
            engineAudio1.pitch = 1f;
        TryStartIgnitionRpc();

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 1.1f));
        if ((float)UnityEngine.Random.Range(0, 100) < chanceToStartIgnition)
        {
            currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
            SetKeyIgnitionValues(twisting: false, keyInHand: false, keyInSlot: true);
            SetIgnition(started: true, cabLightOn: true);
            SetFrontCabinLightOn(setOn: keyIsInIgnition);
            CancelIgnitionAnimation(ignitionOn: true);
            StartIgnitionRpc();
        }
        else
        {
            chanceToStartIgnition += 15f;
            chanceToStartIgnition = Mathf.Clamp(chanceToStartIgnition, 0f, 99f);
        }
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void TryIgnitionRpc(bool setKeyInSlot)
    {
        if (ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        SetKeyIgnitionValues(twisting: false, keyInHand: false, keyInSlot: setKeyInSlot);
        SetFrontCabinLightOn(setKeyInSlot);
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: false));
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void TryStartIgnitionRpc()
    {
        SetKeyIgnitionValues(twisting: true, keyInHand: true, keyInSlot: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);

        engineAudio1.Stop();
        engineAudio1.clip = revEngineStart1;
        engineAudio1.volume = 0.7f;
        if (engineAudio1.clip == revEngineStart1)
            engineAudio1.PlayOneShot(engineRev);
        carEngine1AudioActive = true;
        if (engineAudio1.clip == revEngineStart1)
            engineAudio1.pitch = 1f;
    }


    // --- CANCEL IGNITION METHOD ---
    public new void CancelTryCarIgnition()
    {
        if (!localPlayerInControl ||
            ignitionStarted)
            return;

        // hopefully fix a bug where the wrong animation can play?
        if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 2 && keyIsInIgnition)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 12)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);

        CancelIgnitionAnimation(ignitionOn: false);
        CancelTryIgnitionRpc(keyIsInIgnition, isCabLightOn);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void CancelTryIgnitionRpc(bool setKeyInSlot, bool preIgnition)
    {
        if (currentDriver?.playerBodyAnimator.GetInteger("SA_CarAnim") == 2 && keyIsInIgnition)
            currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else if (currentDriver?.playerBodyAnimator.GetInteger("SA_CarAnim") == 12)
            currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else
            currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 0);

        // account for netlag when the key is first inserted
        if (setKeyInSlot == true && (keyIsInIgnition != setKeyInSlot))
        {
            currentInterior.carKeySounds.PlayOneShot(insertKey);
            RoundManager.Instance.PlayAudibleNoise(currentInterior.carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
        }
        SetKeyIgnitionValues(twisting: false, keyInHand: false, keyInSlot: setKeyInSlot);
        CancelIgnitionAnimation(ignitionOn: false);
        if (setKeyInSlot == true && (isCabLightOn != preIgnition))
            SetFrontCabinLightOn(setOn: preIgnition);
    }


    // --- START IGNITION METHOD ---
    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void StartIgnitionRpc()
    {
        currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
        SetKeyIgnitionValues(twisting: false, keyInHand: false, keyInSlot: true);
        SetIgnition(started: true, cabLightOn: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        CancelIgnitionAnimation(ignitionOn: true);
    }

    public void SetIgnition(bool started, bool cabLightOn)
    {
        SetFrontCabinLightOn(cabLightOn);
        carEngine1AudioActive = started;
        if (started)
        {
            startKeyIgnitionTrigger.SetActive(false);
            removeKeyIgnitionTrigger.SetActive(true);

            if (started == ignitionStarted)
                return;

            ignitionStarted = true;
            carExhaustParticle.Play();
            engineAudio1.Stop();
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

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
        chanceToStartIgnition = 20f;
        RemoveKeyFromIgnitionRpc();
        //GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 6);
    }

    private new IEnumerator RemoveKey()
    {
        currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 6);
        yield return new WaitForSeconds(0.26f);
        SetKeyIgnitionValues(twisting: false, keyInHand: true, keyInSlot: false);
        currentInterior.carKeySounds.PlayOneShot(removeKey);
        RoundManager.Instance.PlayAudibleNoise(currentInterior.carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
        SetIgnition(started: false, cabLightOn: false);
        yield return new WaitForSeconds(0.73f);
        SetKeyIgnitionValues(twisting: false, keyInHand: false, keyInSlot: false);
        keyIgnitionCoroutine = null;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void RemoveKeyFromIgnitionRpc()
    {
        if (!ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
    }


    // --- MISC IGNITION STUFF ---
    public void CancelIgnitionAnimation(bool ignitionOn)
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
            keyIgnitionCoroutine = null;
        }
        carEngine1AudioActive = ignitionOn;
        keyIsInDriverHand = false;
        twistingKey = false;
    }

    public void SetKeyIgnitionValues(bool twisting, bool keyInHand, bool keyInSlot)
    {
        twistingKey = twisting;
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


    // --- DRIVER OCCUPANT METHODS ---
    public void SetDriverInCar()
    {
        if (!hasBeenSpawned || carDestroyed) return;
        if (GameNetworkManager.Instance.localPlayerController.inAnimationWithEnemy || 
            GameNetworkManager.Instance.localPlayerController.inVehicleAnimation) return;
        if (currentDriver != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetDriverInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetDriverInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentDriver != null ||
            (currentDriver != null && currentDriver != playerController))
        {
            CancelSetPlayerInVehicleClientRpc(playerId);
            return;
        }
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[playerId].actualClientId);
        SetDriverInCarOwnerRpc();
        SetDriverInCarClientsRpc(playerId);
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    public void SetDriverInCarOwnerRpc()
    {
        PlayerUtils.disableAnimationSync = true;
        SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
        InteractTriggerPatches.specialInteractCoroutine =
            StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                trigger: driverSeatTrigger,
                playerController: GameNetworkManager.Instance.localPlayerController,
                controller: this,
                isRightHandDrive: isInteriorRHD,
                isPassenger: false));

        ActivateControl();
        InteractTrigger doorTrigger = isInteriorRHD ? passengerSideDoorTrigger : driverSideDoorTrigger;
        AnimatedObjectTrigger door = isInteriorRHD ? passengerSideDoor : driverSideDoor;
        SetTriggerHoverTip(doorTrigger, "Exit : [LMB]");
        startKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        removeKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
        playerSteeringWheelAnimFloat = 0.5f;
        syncedPlayerSteeringAnim = 0.5f;
        if (keyIsInIgnition) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (ignitionStarted) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
        if (door.boolValue) door.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetDriverInCarClientsRpc(int playerId)
    {
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, currentDriver);
        startKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        removeKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        PlayerUtils.ReplaceClientPlayerAnimator(playerId, driverSeatTrigger);

        currentDriver.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
        playerSteeringWheelAnimFloat = 0.5f;
        syncedPlayerSteeringAnim = 0.5f;
        if (keyIsInIgnition) currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (ignitionStarted) currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
    }

    public void OnDriverExit()
    {
        PlayerUtils.disableAnimationSync = false;
        localPlayerInControl = false;
        DisableVehicleCollisionForAllPlayers();
        InteractTrigger doorTrigger = isInteriorRHD ? passengerSideDoorTrigger : driverSideDoorTrigger;
        SetTriggerHoverTip(doorTrigger, "Use door : [LMB]");
        startKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        removeKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        PlayerUtils.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentDriver != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        DisableControl();
        CancelIgnitionAnimation(ignitionOn: ignitionStarted);
        chanceToStartIgnition = 20f;
        SetIgnition(started: ignitionStarted, cabLightOn: isCabLightOn);
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
        OnDriverExitServerRpc(
            transform.position,
            transform.rotation,
            drivePedalPressed,
            brakePedalPressed);
        OnDriverExitRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            syncedPosition,
            syncedRotation,
            ignitionStarted,
            keyIsInIgnition,
            drivePedalPressed,
            brakePedalPressed,
            isCabLightOn);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void OnDriverExitServerRpc(Vector3 carLocation, Quaternion carRotation, bool gasFloored, bool brakeFloored)
    {
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = gasFloored;
        brakePedalPressed = brakeFloored;
        currentDriver = null;
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnDriverExitRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot, bool gasFloored, bool brakeFloored, bool preIgnition)
    {
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = gasFloored;
        brakePedalPressed = brakeFloored;
        currentDriver = null;
        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setIgnitionState;
        startKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        removeKeyIgnitionTrigger.GetComponent<InteractTrigger>().isBeingHeldByPlayer = false;
        PlayerUtils.ReturnClientPlayerAnimator(playerId, driverSeatTrigger);
        CancelIgnitionAnimation(ignitionOn: ignitionStarted);
        SetIgnition(started: ignitionStarted, cabLightOn: preIgnition);
        if (localPlayerInPassengerSeat)
            SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
    }


    // --- PASSENGER OCCUPANT METHODS ---
    public void SetPassengerInCar()
    {
        if (!hasBeenSpawned || carDestroyed) return;
        if (GameNetworkManager.Instance.localPlayerController.inAnimationWithEnemy || 
            GameNetworkManager.Instance.localPlayerController.inVehicleAnimation) return;
        if (currentPassenger != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetPassengerInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetPassengerInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentPassenger != null ||
            (currentPassenger != null && currentPassenger != playerController))
        {
            CancelSetPlayerInVehicleClientRpc(playerId);
            return;
        }
        SetVehicleCollisionForPlayer(setEnabled: false, playerController);
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
            SetPassengerIntoPassengerSeat();
        currentPassenger = playerController;
        SetPassengerInCarRpc(playerId);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SetPassengerInCarRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, playerController);
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId) SetPassengerIntoPassengerSeat();
        currentPassenger = playerController;
        currentPassenger.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
    }

    public void SetPassengerIntoPassengerSeat()
    {
        InteractTriggerPatches.specialInteractCoroutine =
            StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                trigger: passengerSeatTrigger,
                playerController: GameNetworkManager.Instance.localPlayerController,
                controller: this,
                isRightHandDrive: isInteriorRHD,
                isPassenger: true));

        localPlayerInPassengerSeat = true;
        InteractTrigger doorTrigger = isInteriorRHD ? driverSideDoorTrigger : passengerSideDoorTrigger;
        AnimatedObjectTrigger door = isInteriorRHD ? driverSideDoor : passengerSideDoor;
        SetTriggerHoverTip(doorTrigger, "Exit : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
        if (door.boolValue) door.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
    }

    public new void OnPassengerExit()
    {
        SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        localPlayerInPassengerSeat = false;
        InteractTrigger doorTrigger = isInteriorRHD ? driverSideDoorTrigger : passengerSideDoorTrigger;
        SetTriggerHoverTip(doorTrigger, "Use door : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        PlayerUtils.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentPassenger != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        currentPassenger = null!;
        OnPassengerExitRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnPassengerExitRpc(int playerId, Vector3 exitPoint)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == GameNetworkManager.Instance.localPlayerController)
            return;
        playerController.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentPassenger = null!;
        if (!base.IsOwner)
        {
            SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        }
    }


    // --- CANCEL OCCUPANT METHOD ---
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void CancelSetPlayerInVehicleClientRpc(int playerId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId != playerId)
            return;

        HUDManager.Instance.DisplayTip("Kicked from vehicle",
            "You have been forcefully kicked to prevent a softlock!");
    }


    // --- OCCUPANT EXITING METHODS ---
    public void ExitFrontLeftSideSeat()
    {
        if (!localPlayerInControl && !isInteriorRHD) return;
        if (!localPlayerInPassengerSeat && isInteriorRHD) return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (!driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        int exitPoint = CanExitCar(passengerSide: false);
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

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        int exitPoint = CanExitCar(passengerSide: true);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[1].position);
    }

    private new int CanExitCar(bool passengerSide)
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


    // --- PLAYER-VEHICLE COLLISION ---
    public new void EnableVehicleCollisionForAllPlayers()
    {
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            if (StartOfRound.Instance.allPlayerScripts[i] != currentPassenger)
            {
                if (StartOfRound.Instance.allPlayerScripts[i] != GameNetworkManager.Instance.localPlayerController)
                {
                    // local clients
                    StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = (1 << 12) | (1 << 30);
                    StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = (1 << 12) | (1 << 30);
                    return;
                }
                StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = 0;
                StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = 0;
            }
        }
    }

    public new void DisableVehicleCollisionForAllPlayers()
    {
        // 1073741824
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            if (!localPlayerInControl && !localPlayerInPassengerSeat &&
                StartOfRound.Instance.allPlayerScripts[i] == GameNetworkManager.Instance.localPlayerController)
            {
                StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = 0;
                StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = 0;
            }
            else
            {
                StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = (1 << 12) | (1 << 30);
                StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = (1 << 12) | (1 << 30);
            }
        }
    }

    public new void SetVehicleCollisionForPlayer(bool setEnabled, PlayerControllerB player)
    {
        if (!setEnabled)
        {
            player.thisController.excludeLayers = (1 << 12) | (1 << 30);
            player.playerRigidbody.excludeLayers = (1 << 12) | (1 << 30);
            return;
        }
        player.thisController.excludeLayers = 0;
        player.playerRigidbody.excludeLayers = 0;
    }


    // --- PLAYER INPUT TO VEHICLE INPUT & VEHICLE CONTROL METHODS ---
    private new void GetVehicleInput()
    {
        if (GameNetworkManager.Instance.localPlayerController == null)
            return;

        if (GameNetworkManager.Instance.localPlayerController.isTypingChat ||
            GameNetworkManager.Instance.localPlayerController.quickMenuManager.isMenuOpen)
            return;

        // TO-DO: re-work the pedal sync
        if (syncedDrivePedalPressed != drivePedalPressed ||
            syncedBrakePedalPressed != brakePedalPressed)
        {
            syncedDrivePedalPressed = drivePedalPressed;
            syncedBrakePedalPressed = brakePedalPressed;
            SyncPedalInputsRpc(drivePedalPressed, brakePedalPressed);
        }

        if (!ignitionStarted)
        {
            moveInputVector = Vector2.zero;
            steeringAnimValue = 0f;
            drivePedalPressed = false;
            brakePedalPressed = false;
            return;
        }

        moveInputVector = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        steeringAnimValue = moveInputVector.x;
        drivePedalPressed = UserVehicleControls.VehicleControlsInstance.GasPedalKey.IsPressed();
        brakePedalPressed = UserVehicleControls.VehicleControlsInstance.BrakePedalKey.IsPressed();
    }

    private new void ActivateControl()
    {
        //InputActionAsset inputActionAsset = IngamePlayerSettings.Instance.playerInput.actions;
        UserVehicleControls.VehicleControlsInstance.TurboKey.performed += DoTurboBoost;

        localPlayerInControl = true;
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = GameNetworkManager.Instance.localPlayerController;
    }

    private new void DisableControl()
    {
        //InputActionAsset inputActionAsset = IngamePlayerSettings.Instance.playerInput.actions;
        UserVehicleControls.VehicleControlsInstance.TurboKey.performed -= DoTurboBoost;

        localPlayerInControl = false;
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
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
        RoundManager.Instance.PlayAudibleNoise(gearStickAudio.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        ShiftToGearRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, setGear);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ShiftToGearRpc(int playerId, int setGear)
    {
        timeAtLastGearShift = Time.realtimeSinceStartup;
        playerWhoShifted = StartOfRound.Instance.allPlayerScripts[playerId];
        gear = (CarGearShift)setGear;
        gearStickAudio.PlayOneShot(gearStickAudios[setGear - 1]);
        RoundManager.Instance.PlayAudibleNoise(gearStickAudio.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
    }


    // --- AUTOPILOT MAGNET ---
    public new void StartMagneting()
    {
        mainRigidbody.isKinematic = true;
        mainRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        magnetedToShip = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;
        averageVelocityAtMagnetStart = averageVelocity;
        RoundManager.Instance.tempTransform.eulerAngles = new Vector3(0f, mainRigidbody.rotation.eulerAngles.y, 0f);
        Vector3 tempRotation = RoundManager.Instance.tempTransform.eulerAngles;

        float magnetAngle = Vector3.Angle(RoundManager.Instance.tempTransform.forward, -StartOfRound.Instance.magnetPoint.forward);
        Vector3 eulerAngles = mainRigidbody.rotation.eulerAngles;
        if (magnetAngle < 47f || magnetAngle > 133f)
        {
            if (eulerAngles.y < 0f)
            {
                eulerAngles.y -= 46f - magnetAngle;
            }
            else
            {
                eulerAngles.y += 46f - magnetAngle;
            }
        }
        eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
        eulerAngles.z = Mathf.Round(eulerAngles.z / 90f) * 90f;
        eulerAngles.x += UnityEngine.Random.Range(-5f, 5f);
        magnetTargetRotation = Quaternion.Euler(eulerAngles);
        magnetStartRotation = mainRigidbody.rotation;
        Quaternion rotation = mainRigidbody.rotation;
        mainRigidbody.rotation = magnetTargetRotation;
        magnetTargetPosition = boundsCollider.ClosestPoint(StartOfRound.Instance.magnetPoint.position) - mainRigidbody.position;
        if (magnetTargetPosition.y >= boundsCollider.bounds.extents.y)
        {
            magnetTargetPosition.y -= boundsCollider.bounds.extents.y / 2f;
        }
        else if (magnetTargetPosition.y <= boundsCollider.bounds.extents.y * 0.4f)
        {
            magnetTargetPosition.y += boundsCollider.bounds.extents.y / 2f;
        }
        magnetTargetPosition = StartOfRound.Instance.magnetPoint.position - magnetTargetPosition;
        magnetTargetPosition.z = Mathf.Min(-20.4f, magnetTargetPosition.z);
        magnetTargetPosition.y = Mathf.Max(2.5f, magnetStartPosition.y);
        magnetTargetPosition = StartOfRound.Instance.elevatorTransform.InverseTransformPoint(magnetTargetPosition);
        mainRigidbody.rotation = rotation;
        magnetStartPosition = mainRigidbody.position;

        CollectItemsInTruck();
        if (StartOfRound.Instance.inShipPhase) return;
        if (GameNetworkManager.Instance.localPlayerController == null) return;
        if (!base.IsOwner) return;
        MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, tempRotation, averageVelocityAtMagnetStart);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void MagnetCarRpc(Vector3 targetPosition, Vector3 targetRotation, Vector3 startPosition, Quaternion startRotation, Vector3 tempRotation, Vector3 avgVel)
    {
        mainRigidbody.isKinematic = true;
        mainRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        magnetedToShip = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        averageVelocityAtMagnetStart = avgVel;
        RoundManager.Instance.tempTransform.eulerAngles = tempRotation;

        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;

        magnetStartPosition = startPosition;
        magnetStartRotation = startRotation;

        magnetTargetPosition = targetPosition;
        magnetTargetRotation = Quaternion.Euler(targetRotation);
        CollectItemsInTruck();
    }

    public new void CollectItemsInTruck()
    {
        Collider[] array = Physics.OverlapSphere(transform.position, 25f, 64, QueryTriggerInteraction.Collide);
        for (int i = 0; i < array.Length; i++)
        {
            GrabbableObject scrapItem = array[i].GetComponent<GrabbableObject>();
            if (scrapItem != null &&
                !scrapItem.isHeld &&
                !scrapItem.isHeldByEnemy &&
                array[i].transform.parent == transform)
            {
                if (References.lastDriver != null)
                {
                    References.lastDriver.SetItemInElevator(magnetedToShip, magnetedToShip, scrapItem);
                }
                else if (References.lastDriver == null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    GameNetworkManager.Instance.localPlayerController.SetItemInElevator(magnetedToShip, magnetedToShip, scrapItem);
                }
            }
        }
    }


    // --- WEEDKILLER FUNCTIONALITY ---
    public new void AddEngineOil()
    {
        if (truckType == TruckVersionType.V55)
            return;
        int setEngineHealth = Mathf.Min(carHP + 4, baseCarHP);
        AddEngineOilOnLocalClient(setEngineHealth);
        AddEngineOilRpc(setEngineHealth);
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
        if (truckType == TruckVersionType.V55)
            return;
        int setTurboBoosts = Mathf.Min(turboBoosts + 1, 5);
        AddTurboBoostOnLocalClient(setTurboBoosts);
        AddTurboBoostRpc(setTurboBoosts);
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
        if (truckType == TruckVersionType.V55)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (playerController == null) return;
        if (playerController.isPlayerDead) return;
        if (!playerController.isPlayerControlled) return;
        if (playerController.isTypingChat ||
            playerController.quickMenuManager.isMenuOpen) return;

        if (!localPlayerInControl || !ignitionStarted ||
            jumpingInCar || keyIsInDriverHand) return;

        Vector2 dir = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();
        UseTurboBoostLocalClient(dir);
        UseTurboBoostRpc();
    }

    public new void UseTurboBoostLocalClient(Vector2 dir = default(Vector2))
    {
        currentDriver?.playerBodyAnimator.SetTrigger("SA_JumpInCar");
        currentDriver?.movementAudio.PlayOneShot(jumpInCarSFX);
        if (base.IsOwner)
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
        if (!base.IsOwner)
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
        RemoveCarRainCollision();
        DisableControl();
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
        physicsRegion.disablePhysicsRegion = true;
        if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(physicsRegion))
        {
            StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(physicsRegion);
        }
    }


    // --- VEHICLE ZONE NETWORKING ---
    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SyncPlayerZoneRpc(int playerId, bool onTruck, bool inStorage)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
        if (player == null ||
            player.isPlayerDead ||
            !player.isPlayerControlled)
            return;

        var data = PlayerControllerBPatches.GetData(player);
        data.isPlayerOnTruck = onTruck;
        data.isPlayerInStorage = inStorage;
    }


    // --- UPDATE ---
    public new void Update()
    {
        if (destroyNextFrame)
        {
            if (base.IsOwner)
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
            RemoveCarRainCollision();
            physicsRegion.disablePhysicsRegion = true;
            if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(physicsRegion))
                StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(physicsRegion);

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
                magnetedToShip = false;
                StartOfRound.Instance.isObjectAttachedToMagnet = false;
                CollectItemsInTruck();
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
            if (base.IsOwner && !carDestroyed && !StartOfRound.Instance.isObjectAttachedToMagnet && StartOfRound.Instance.magnetOn && Vector3.Distance(transform.position, StartOfRound.Instance.magnetPoint.position) < 10f && !Physics.Linecast(transform.position, StartOfRound.Instance.magnetPoint.position, 256, QueryTriggerInteraction.Ignore))
            {
                StartMagneting();
                return;
            }
            if (base.IsOwner)
            {
                if (enabledCollisionForAllPlayers)
                {
                    enabledCollisionForAllPlayers = false;
                    DisableVehicleCollisionForAllPlayers();
                }
                if (!inDropshipAnimation) SyncCarPositionToOtherClients();
            }
            else
            {
                if (!enabledCollisionForAllPlayers)
                {
                    enabledCollisionForAllPlayers = true;
                    EnableVehicleCollisionForAllPlayers();
                }
            }
        }

        ReactToDamage();

        if (carDestroyed)
        {
            RHDInterior.driverSeatTrigger.interactable = false;
            RHDInterior.passengerSeatTrigger.interactable = false;
            LHDInterior.driverSeatTrigger.interactable = false;
            LHDInterior.passengerSeatTrigger.interactable = false;
            return;
        }

        RHDInterior.driverSeatTrigger.interactable = isInteriorRHD && hasBeenSpawned && (Time.realtimeSinceStartup - timeSinceSpringingDriverSeat > 3f);
        RHDInterior.passengerSeatTrigger.interactable = isInteriorRHD && hasBeenSpawned;
        LHDInterior.driverSeatTrigger.interactable = !isInteriorRHD && hasBeenSpawned && (Time.realtimeSinceStartup - timeSinceSpringingDriverSeat > 3f);
        LHDInterior.passengerSeatTrigger.interactable = !isInteriorRHD && hasBeenSpawned;

        if (PlayerUtils.seatedInTruck) SyncPlayerLookInput();
        if (currentDriver != null)
        {
            currentDriver.playerBodyAnimator.SetFloat("animationSpeed", playerSteeringWheelAnimFloat); // player steering animation
            currentDriver.playerBodyAnimator.SetFloat("SA_CarMotionTime", gearStickAnimValue); // vehicle gearstick --> player gearstick animation position
            if (ignitionStarted && keyIgnitionCoroutine == null)
            {
                var data = PlayerControllerBPatches.GetData(currentDriver);
                int currentAnimIndex = 1;

                bool isLookingOver = (isInteriorRHD ? 
                    data.vehicleCameraHorizontal < currentInterior.cameraLookAngle : 
                    data.vehicleCameraHorizontal > currentInterior.cameraLookAngle);
                if (isLookingOver)
                {
                    if ((Time.realtimeSinceStartup - timeAtLastGearShift < 1.7f) && playerWhoShifted == currentDriver) currentAnimIndex = 5;
                    else currentAnimIndex = 4;
                    currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", currentAnimIndex);
                }
                else currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", currentAnimIndex);
            }
        }
        SetCarEffects(steeringAnimValue);
        if (!ignitionStarted) EngineRPM = Mathf.Lerp(EngineRPM, 0f, 3f * Time.deltaTime);
        if (base.IsOwner)
        {
            if (localPlayerInControl && currentDriver != null) GetVehicleInput();
            float vehicleStress = 0f;
            bool engineOn = ignitionStarted;
            bool gasPressed = drivePedalPressed && engineOn;

            if (engineOn && gear != CarGearShift.Park &&
                brakePedalPressed && drivePedalPressed)
            {
                vehicleStress += 2f;
                lastStressType += "; Accelerating while braking";
            }
            if (gear == CarGearShift.Park)
            {
                if (gasPressed)
                {
                    vehicleStress += 1.2f;
                    lastStressType += "; Accelerating while in park";
                }
                if (engineOn &&
                    BackLeftWheel.isGrounded && BackRightWheel.isGrounded &&
                    averageVelocity.magnitude > 18f)
                {
                    vehicleStress += Mathf.Clamp(((averageVelocity.magnitude * 165f) - 200f) / 150f, 0f, 4f);
                    lastStressType += "; In park while at high speed";
                }
            }
            SetInternalStress(vehicleStress);
            stressPerSecond = vehicleStress;

            SyncCarDrivetrainToOtherClients();
            SyncCarWheelTorqueToOtherClients();
            return;
        }
        moveInputVector = Vector2.zero;
    }


    // --- CUSTOM PLAYER ANIMATION NETWORKING ---
    public void SyncPlayerLookInput()
    {
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer == null ||
            localPlayer.isPlayerDead ||
            !localPlayer.isPlayerControlled)
            return;

        var data = PlayerControllerBPatches.GetData(localPlayer);

        if (data.vehicleCameraHorizontal != localPlayer.ladderCameraHorizontal)
            data.vehicleCameraHorizontal = localPlayer.ladderCameraHorizontal;

        if (data.syncLookInputInterval >= 0.14f)
        {
            if (data.lastVehicleCameraHorizontal != localPlayer.ladderCameraHorizontal)
            {
                data.syncLookInputInterval = 0f;
                data.lastVehicleCameraHorizontal = localPlayer.ladderCameraHorizontal;
                SyncPlayerLookInputRpc((int)localPlayer.playerClientId, localPlayer.ladderCameraHorizontal);
                return;
            }
        }
        else
        {
            data.syncLookInputInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SyncPlayerLookInputRpc(int playerId, float lookInput)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
        if (player == null || 
            player.isPlayerDead || 
            !player.isPlayerControlled)
            return;

        var data = PlayerControllerBPatches.GetData(player);
        data.vehicleCameraHorizontal = lookInput;
    }


    public void UpdatePlayerVehicleAnimations()
    {
        PlayerControllerB thisPlayer = GameNetworkManager.Instance.localPlayerController;
        if (thisPlayer == null ||
            thisPlayer.isPlayerDead ||
            !thisPlayer.isPlayerControlled)
            return;

        int currentAnimIndex = thisPlayer.playerBodyAnimator.GetInteger("SA_CarAnim");
        var data = PlayerControllerBPatches.GetData(thisPlayer);

        if (data.currentCarAnimation != currentAnimIndex)
        {
            data.currentCarAnimation = currentAnimIndex;
            SyncPlayerVehicleAnimationsRpc((int)thisPlayer.playerClientId, currentAnimIndex, false, null!);
        }
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SyncPlayerVehicleAnimationsRpc(int playerId, int animIndex, bool isTrigger, string triggerName)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
        if (player == null)
        {
            Plugin.Logger.LogWarning($"no driver found for {base.gameObject.name}, aborting");
            return;
        }
        if (player.isPlayerDead ||
            !player.isPlayerControlled)
            return;

        if (isTrigger)
        {
            player.playerBodyAnimator.SetTrigger(triggerName);
            return;
        }
        player.playerBodyAnimator.SetInteger("SA_CarAnim", animIndex);
    }


    // --- RADIO TIME SYNC ---
    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SyncRadioTimeRpc(float songTime)
    {
        currentSongTime = songTime;
        SetRadioTime();
    }

    public void SetRadioTime()
    {
        //if (radioAudio.clip == null) return;
        //radioAudio.time = Mathf.Clamp(currentSongTime % radioAudio.clip.length, 0.01f, radioAudio.clip.length - 0.1f);
        if (radioAudio.clip == null) return;
        float songTime = currentSongTime % radioAudio.clip.length;
        if (songTime < 0) songTime += radioAudio.clip.length;
        radioAudio.time = songTime;
    }


    // --- RADIO CHANNEL ---
    public new void ChangeRadioStation()
    {
        if (!radioOn)
        {
            radioOn = true;
            if (!radioAudio.isPlaying) radioAudio.Play();
            if (!radioInterference.isPlaying) radioInterference.Play();
        }

        currentRadioClip = (currentRadioClip + 1) % radioClips.Length;
        radioAudio.clip = radioClips[currentRadioClip];
        currentSongTime = 0f;
        SetRadioTime();
        if (!radioAudio.isPlaying) radioAudio.Play();

        int quality = (int)Mathf.Round(radioSignalQuality);
        switch (quality)
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
        SetRadioStationRpc(currentRadioClip, quality);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioStationRpc(int radioStation, int signalQuality)
    {
        if (!radioOn)
        {
            radioOn = true;
            if (!radioAudio.isPlaying) radioAudio.Play();
            if (!radioInterference.isPlaying) radioInterference.Play();
        }
        currentRadioClip = radioStation;
        radioSignalQuality = signalQuality;
        radioAudio.clip = radioClips[currentRadioClip];
        currentSongTime = 0f;
        SetRadioTime();
        if (!radioAudio.isPlaying) radioAudio.Play();
    }


    // --- RADIO TOGGLE ---
    public new void SwitchRadio()
    {
        radioOn = !radioOn;
        if (radioAudio.clip == null)
            radioAudio.clip = radioClips[currentRadioClip];
        if (radioOn)
        {
            if (!radioAudio.isPlaying) radioAudio.Play();
            if (!radioInterference.isPlaying) radioInterference.Play();
        }
        else
        {
            if (radioAudio.isPlaying) radioAudio.Stop();
            if (radioInterference.isPlaying) radioInterference.Stop();
        }
        SetRadioRpc(radioOn, currentRadioClip);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioRpc(bool on, int radioStation)
    {
        if (radioOn == on) return;
        radioOn = on;
        currentRadioClip = radioStation;
        if (radioAudio.clip == null)
            radioAudio.clip = radioClips[currentRadioClip];
        radioAudio.clip = radioClips[currentRadioClip];
        SetRadioTime();

        if (radioOn)
        {
            if (!radioAudio.isPlaying) radioAudio.Play();
            if (!radioInterference.isPlaying) radioInterference.Play();
        }
        else
        {
            if (radioAudio.isPlaying) radioAudio.Stop();
            if (radioInterference.isPlaying) radioInterference.Stop();
        }
    }


    // --- RADIO VALUES ---
    public new void SetRadioValues()
    {
        if (!radioOn)
        {
            if (radioAudio.isPlaying) radioAudio.Stop();
            if (radioInterference.isPlaying) radioInterference.Stop();
            currentSongTime = 0f;
            return;
        }
        if (IsHost)
        {
            currentSongTime += Time.deltaTime;
            if (Time.realtimeSinceStartup - timeLastSyncedRadio > 1f)
            {
                timeLastSyncedRadio = Time.realtimeSinceStartup;

                SyncRadioTimeRpc(currentSongTime);
            }
        }
        if (!radioAudio.isPlaying) radioAudio.Play();
        if (!radioInterference.isPlaying) radioInterference.Play();
        if (IsServer && radioAudio.isPlaying &&
            Time.realtimeSinceStartup > radioPingTimestamp)
        {
            radioPingTimestamp = (Time.realtimeSinceStartup + 1f);
            RoundManager.Instance.PlayAudibleNoise(radioAudio.transform.position, 16f, Mathf.Min((radioAudio.volume + radioInterference.volume) * 0.5f, 0.9f), 0, false, 2692);
        }
        if (base.IsOwner)
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
                SetRadioSignalQualityRpc((int)Mathf.Round(radioSignalQuality));
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
    public void SetRadioSignalQualityRpc(int signalQuality)
    {
        radioSignalQuality = signalQuality;
    }


    // --- WHEEL VISUALS ---
    private new void MatchWheelMeshToCollider(MeshRenderer wheelMesh, WheelCollider wheelCollider)
    {
        wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        wheelMesh.transform.rotation = rotation;
        wheelMesh.transform.position = position;
    }


    // --- VISUAL EFFECTS ---
    private new void SetCarEffects(float setSteering)
    {
        setSteering = base.IsOwner ? setSteering : 0f;
        steeringWheelAnimFloat = Mathf.Clamp(steeringWheelAnimFloat + setSteering * steeringWheelTurnSpeed * Time.deltaTime / 6f, -1f, 1f);
        float playerSteer = Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f) - steeringWheelAnimator.GetFloat("steeringWheelTurnSpeed");
        steeringWheelAnimator.SetFloat("steeringWheelTurnSpeed", Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f));

        if (localPlayerInControl && currentDriver != null)
            playerSteeringWheelAnimFloat = currentDriver.playerBodyAnimator.GetFloat("animationSpeed") + playerSteer * 2f;

        SetCarAutomaticShifter();
        SetCarLightingEffects();
        SetCarAudioEffects();
        CalculateTyreSlip();
        SetCarKeyEffects();

        MatchWheelMeshToCollider(leftWheelMesh, FrontLeftWheel);
        MatchWheelMeshToCollider(rightWheelMesh, FrontRightWheel);
        MatchWheelMeshToCollider(backLeftWheelMesh, BackLeftWheel);
        MatchWheelMeshToCollider(backRightWheelMesh, BackRightWheel);

        if (base.IsOwner)
        {
            SyncCarEffectsToOtherClients();
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
        if (ignitionStarted) EngineRPM = Mathf.Lerp(EngineRPM, syncedEngineRPM, 3f * Time.deltaTime);
        wheelRPM = Mathf.Lerp(wheelRPM, syncedWheelRPM, Time.deltaTime * 4f);
        steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, syncedWheelRotation, steeringWheelTurnSpeed * Time.deltaTime / 6f);
        playerSteeringWheelAnimFloat = Mathf.MoveTowards(playerSteeringWheelAnimFloat, syncedPlayerSteeringAnim, steeringWheelTurnSpeed * Time.deltaTime / 6f);
    }

    // automatic shifter position
    public void SetCarAutomaticShifter()
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
    public void SetCarManualShifter()
    {
        //TBD
    }

    public void SetCarLightingEffects()
    {
        if (gear == CarGearShift.Reverse && keyIsInIgnition && !backLightsOn)
        {
            backLightsOn = true;
            backLightsMesh.material = backLightOnMat;
            backLightsContainer.SetActive(true);
        }
        else if ((backLightsOn && !keyIsInIgnition) || (backLightsOn && gear != CarGearShift.Reverse))
        {
            backLightsOn = false;
            backLightsMesh.material = greyLightOffMat;
            backLightsContainer.SetActive(false);
        }
    }

    /// <summary>
    ///  Available from EnemySoundFixes, licensed under GNU General Public License.
    ///  Source: https://github.com/ButteryStancakes/EnemySoundFixes/blob/master/Patches/CruiserPatches.cs
    /// </summary>
    private new void SetVehicleAudioProperties(AudioSource audio, bool audioActive, float lowest, float highest, float lerpSpeed, bool useVolumeInsteadOfPitch = false, float onVolume = 1f)
    {
        if (audioActive && ((audio == extremeStressAudio && magnetedToShip) || ((audio == rollingAudio || audio == skiddingAudio) && (magnetedToShip ||
            (!FrontLeftWheel.isGrounded && !FrontRightWheel.isGrounded && !BackLeftWheel.isGrounded && !BackRightWheel.isGrounded)))))
            audioActive = false;

        if (audioActive)
        {
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
            return;
        }
        if (useVolumeInsteadOfPitch)
        {
            audio.volume = Mathf.Lerp(audio.volume, 0f, lerpSpeed * Time.deltaTime);
        }
        else
        {
            audio.volume = Mathf.Lerp(audio.volume, 0f, 4f * Time.deltaTime);
            audio.pitch = Mathf.Lerp(audio.pitch, lowest, 4f * Time.deltaTime);
        }
        if (audio.isPlaying && audio.volume <= 0.001f)
        {
            audio.Stop();
        }
    }

    public void SetCarAudioEffects()
    {
        float highestAudio1 = Mathf.Clamp((EngineRPM / engineIntensityPercentage), 0.65f, 1.15f);
        float highestAudio2 = Mathf.Clamp((EngineRPM / engineIntensityPercentage), 0.7f, 1.5f);
        float wheelSpeed = Mathf.Abs(wheelRPM);
        float highestTyre = Mathf.Clamp(wheelSpeed / (180f * 0.35f), 0f, 1f);
        carEngine2AudioActive = ignitionStarted;
        carRollingAudioActive = (FrontLeftWheel.isGrounded || FrontRightWheel.isGrounded || BackLeftWheel.isGrounded || BackRightWheel.isGrounded) && wheelSpeed > 10f;
        if (!ignitionStarted)
        {
            highestAudio1 = 1f;
        }
        SetVehicleAudioProperties(engineAudio1, carEngine1AudioActive, 0.7f, highestAudio1, 2f, useVolumeInsteadOfPitch: false, 0.7f);
        SetVehicleAudioProperties(engineAudio2, carEngine2AudioActive, 0.7f, highestAudio2, 3f, useVolumeInsteadOfPitch: false, 0.5f);
        SetVehicleAudioProperties(rollingAudio, carRollingAudioActive, 0f, highestTyre, 5f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(extremeStressAudio, underExtremeStress, 0.2f, 1f, 3f, useVolumeInsteadOfPitch: true);
        SetRadioValues();
        if (engineAudio1.volume > 0.3f && engineAudio1.isPlaying && Time.realtimeSinceStartup - timeAtLastEngineAudioPing > 2f)
        {
            timeAtLastEngineAudioPing = Time.realtimeSinceStartup;
            if (EngineRPM > 1300f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 32f, 0.75f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            if (EngineRPM > 600f && EngineRPM < 1300f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 25f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else if (!ignitionStarted)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 15f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 11f, 0.5f, 0, noiseIsInsideClosedShip: false, 2692);
            }
        }
        //if (gear == CarGearShift.Reverse)
        //{
        //    reverseWhine.pitch = Mathf.Lerp(0f, 1.65f, wheelSpeed / 420f);
        //    reverseWhine.volume = Mathf.Lerp(0f, 1f, wheelSpeed / 310f);

        //    if (!reverseWhine.isPlaying)
        //        reverseWhine.Play();
        //}
        //else
        //{
        //    reverseWhine.pitch = Mathf.Lerp(reverseWhine.pitch, 0f, 16f * Time.deltaTime);
        //    reverseWhine.volume = Mathf.Lerp(reverseWhine.volume, 0f, 16f * Time.deltaTime);
        //}

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
            hornAudio.pitch = 1f;

            if (!hornAudio.isPlaying)
                hornAudio.Play();

            if (Time.realtimeSinceStartup - timeAtLastHornPing > 2f)
            {
                timeAtLastHornPing = Time.realtimeSinceStartup;
                RoundManager.Instance.PlayAudibleNoise(hornAudio.transform.position, 28f, 0.85f, 0, noiseIsInsideClosedShip: false, 106217);
            }
        }
        else
        {
            hornAudio.pitch = Mathf.Max(hornAudio.pitch - Time.deltaTime * 6f, 0.01f);

            if (hornAudio.pitch < 0.02f)
                hornAudio.Stop();
        }
    }


    // --- MISC EFFECTS ---
    // tyre skid effects
    public void CalculateTyreSlip()
    {
        if (base.IsOwner)
        {
            float vehicleSpeed = Vector3.Dot(Vector3.Normalize(mainRigidbody.velocity * 1000f), transform.forward);
            float wheelSpeed = Mathf.Abs(wheelRPM);
            bool audioActive = vehicleSpeed > -0.6f && vehicleSpeed < 0.4f && (averageVelocity.magnitude > 4f || (wheelSpeed > 65f));
            bool wheelsGrounded = BackLeftWheel.isGrounded && BackRightWheel.isGrounded;

            if (wheelsGrounded)
            {
                bool forwardSlipping = (wheelTorque > 900f) &&
                    Mathf.Abs(forwardsSlip) > 0.2f;

                if (forwardSlipping)
                {
                    vehicleSpeed = Mathf.Max(vehicleSpeed, 0.8f);
                    audioActive = true;

                    if (averageVelocity.magnitude > 8f && !tireSparks.isPlaying)
                        tireSparks.Play(true);
                }
                else
                {
                    audioActive = false;
                    tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            else
            {
                audioActive = false;
                tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            SetVehicleAudioProperties(skiddingAudio, audioActive, 0f, vehicleSpeed, 3f, true, 1f);

            if (Mathf.Abs(tyreStress - vehicleSpeed) > 0.04f || wheelSlipping != audioActive)
                SetTyreStressRpc(vehicleSpeed, audioActive);

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

    // what a mess, but it works, and it's
    // better than whatever i had before
    public void SetCarKeyEffects()
    {
        Transform ignBarrelRot = (ignitionStarted || twistingKey)
            ? currentInterior.ignitionBarrelTurnedPosition
            : currentInterior.ignitionBarrelNotTurnedPosition;

        currentInterior.ignitionBarrel.transform.localPosition = ignBarrelRot.localPosition;
        currentInterior.ignitionBarrel.transform.localRotation = Quaternion.Lerp(
            currentInterior.ignitionBarrel.transform.localRotation,
            ignBarrelRot.localRotation,
            Time.deltaTime * ignitionRotSpeed
        );

        if (keyIsInIgnition)
        {
            if (!keyObject.enabled)
                keyObject.enabled = true;

            if (keyObject.transform.parent != currentInterior.carKeyContainer.transform)
                keyObject.transform.SetParent(currentInterior.carKeyContainer.transform);
            keyObject.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            if (carKeyInHand.transform.parent != currentInterior.carKeyContainer.transform)
                carKeyInHand.transform.SetParent(currentInterior.carKeyContainer.transform, false);
            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            Transform ignKeyRot = (ignitionStarted || twistingKey)
                ? ignitionTurnedPosition
                : ignitionNotTurnedPosition;

            if (!correctedPosition)
            {
                correctedPosition = true;
                keyObject.transform.localPosition = ignKeyRot.localPosition;
                keyObject.transform.localRotation = ignKeyRot.localRotation;
            }

            keyObject.transform.localPosition = ignKeyRot.localPosition;
            keyObject.transform.localRotation = Quaternion.Lerp(
                keyObject.transform.localRotation,
                ignKeyRot.localRotation,
                Time.deltaTime * ignitionRotSpeed
            );
        }
        else
        {
            if (!keyIsInDriverHand && keyObject.enabled)
                keyObject.enabled = false;
            correctedPosition = false;
        }

        if (currentDriver == null)
        {
            if (currentDriver_LHand_transform != null)
                currentDriver_LHand_transform = null!;
            return;
        }
        if (currentDriver_LHand_transform == null)
            currentDriver_LHand_transform = currentDriver.bodyParts[2].Find("hand.L").transform;
        if (keyIsInDriverHand && !keyIsInIgnition)
        {
            if (!keyObject.enabled)
                keyObject.enabled = true;

            Transform handParent;
            Vector3 posOffset, rotOffset;

            if (!isInteriorRHD)
            {
                handParent = localPlayerInControl
                    ? currentDriver.localItemHolder.parent
                    : currentDriver.serverItemHolder.parent;

                posOffset = localPlayerInControl ? LHD_Pos_Local : LHD_Pos_Server;
                rotOffset = localPlayerInControl ? LHD_Rot_Local : LHD_Rot_Server;
            }
            else
            {
                handParent = localPlayerInControl
                    ? currentDriver.leftHandItemTarget.transform
                    : currentDriver_LHand_transform;

                posOffset = localPlayerInControl ? RHD_Pos_Local : RHD_Pos_Server;
                rotOffset = localPlayerInControl ? RHD_Rot_Local : RHD_Rot_Server;
            }
            if (carKeyInHand.transform.parent != handParent.transform)
                carKeyInHand.transform.SetParent(handParent, false);

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;
            carKeyInHand.transform.localScale = Vector3.one;

            if (keyObject.transform.parent != carKeyInHand.transform)
                keyObject.transform.SetParent(carKeyInHand.transform);

            keyObject.transform.localPosition = posOffset;
            keyObject.transform.localRotation = Quaternion.Euler(rotOffset);
        }
    }


    // --- PHYSICS UPDATE ---
    public new void FixedUpdate()
    {
        bool allWheelsAirborne = !FrontLeftWheel.isGrounded &&
                                 !FrontRightWheel.isGrounded &&
                                 !BackLeftWheel.isGrounded &&
                                 !BackRightWheel.isGrounded;

        if (!carDestroyed)
        {
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
        }

        if (!StartOfRound.Instance.inShipPhase && !loadedVehicleFromSave && !hasDeliveredVehicle)
        {
            if (itemShip == null && References.itemShip != null)
                itemShip = References.itemShip;

            if (itemShip != null)
            {
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

                    mainRigidbody.isKinematic = true;
                    mainRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                    mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
                    mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);

                    averageVelocity = Vector3.zero;

                    syncedPosition = mainRigidbody.position;
                    syncedRotation = mainRigidbody.rotation;
                }
            }
            else if (itemShip == null)
            {
                inDropshipAnimation = false;

                mainRigidbody.isKinematic = true;
                mainRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                mainRigidbody.MovePosition(StartOfRound.Instance.notSpawnedPosition.position + Vector3.forward * 30f);

                syncedPosition = mainRigidbody.position;
                syncedRotation = mainRigidbody.rotation;
            }
        }
        if (magnetedToShip)
        {
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
            mainRigidbody.MovePosition(Vector3.Lerp(magnetStartPosition, StartOfRound.Instance.elevatorTransform.position + magnetTargetPosition, magnetPositionCurve.Evaluate(magnetTime)));
            mainRigidbody.MoveRotation(Quaternion.Lerp(magnetStartRotation, magnetTargetRotation, magnetRotationCurve.Evaluate(magnetRotationTime)));
            averageVelocityAtMagnetStart = Vector3.Lerp(averageVelocityAtMagnetStart, Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 4f), 4f * Time.fixedDeltaTime);

            if (!finishedMagneting)
                magnetStartPosition += Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 5f) * Time.fixedDeltaTime;
        }
        else
        {
            if (!base.IsOwner && !inDropshipAnimation)
            {
                mainRigidbody.isKinematic = true;
                mainRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                Vector3 syncVel = syncedPosition + (averageVelocity * Time.fixedDeltaTime);
                Mathf.Clamp(syncSpeedMultiplier * Vector3.Distance(mainRigidbody.position, syncVel), 1.3f, 300f);
                Vector3 position = Vector3.Lerp(mainRigidbody.position, syncVel, Time.fixedDeltaTime * syncSpeedMultiplier);
                mainRigidbody.MovePosition(position);
                mainRigidbody.MoveRotation(Quaternion.Lerp(mainRigidbody.rotation, syncedRotation, syncRotationSpeed));
            }
        }

        if (base.IsOwner) averageVelocity += (mainRigidbody.velocity - averageVelocity) / (movingAverageLength + 1);
        else averageVelocity += (((mainRigidbody.position - previousVehiclePosition) / Time.fixedDeltaTime) - averageVelocity) / (movingAverageLength + 1);

        ragdollPhysicsBody.Move(
            transform.position,
            transform.rotation);
        playerPhysicsBody.Move(
            transform.position,
            transform.rotation);
        windwiperPhysicsBody1.Move(
            windwiper1.position,
            windwiper1.rotation);
        windwiperPhysicsBody2.Move(
            windwiper2.position,
            windwiper2.rotation);

        if (carDestroyed)
            return;

        FrontLeftWheel.steerAngle = 50f * steeringWheelAnimFloat;
        FrontRightWheel.steerAngle = 50f * steeringWheelAnimFloat;

        foreach (WheelCollider drivenWheel in wheels)
        {
            drivenWheel.motorTorque = wheelTorque;
            drivenWheel.brakeTorque = wheelBrakeTorque;
            drivenWheel.rotationSpeed = Mathf.Clamp(drivenWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
        }

        if (!base.IsOwner)
        {
            wheelTorque = syncedMotorTorque;
            wheelBrakeTorque = syncedBrakeTorque;
            previousVehiclePosition = mainRigidbody.position;
            return;
        }

        bool engineOn = ignitionStarted;
        bool gasPressed = drivePedalPressed && engineOn;
        bool atIdle = !drivePedalPressed && engineOn;

        if (engineOn && gear != CarGearShift.Park)
        {
            if (brakePedalPressed)
            {
                wheelBrakeTorque = 2000f;
            }
            else
            {
                wheelBrakeTorque = 0f;
            }
        }
        switch (gear)
        {
            case CarGearShift.Park:
                {
                    wheelBrakeTorque = 2000f;
                    wheelTorque = 0f;
                    break;
                }
            case CarGearShift.Reverse:
                {
                    if (gasPressed) wheelTorque = 0f - EngineTorque;
                    else if (atIdle) wheelTorque = idleSpeed * -1f;
                    break;
                }
            case CarGearShift.Drive:
                {
                    if (gasPressed) wheelTorque = Mathf.Clamp(Mathf.MoveTowards(wheelTorque, EngineTorque, carAcceleration * Time.fixedDeltaTime), 325f, 1000f);
                    else if (atIdle) wheelTorque = idleSpeed * 1f;
                    break;
                }
        }
        if (!ignitionStarted && gear != CarGearShift.Park)
        {
            wheelTorque = 0f;
            wheelBrakeTorque = 2000f;
        }

        wheelRPM = Mathf.Abs((FrontLeftWheel.rpm + FrontRightWheel.rpm + 
                              BackLeftWheel.rpm + BackRightWheel.rpm) 
                              / 4f);
        EngineRPM = wheelRPM;
        previousVehiclePosition = mainRigidbody.position;
        if (mainRigidbody.IsSleeping() || magnetedToShip || allWheelsAirborne)
        {
            forwardsSlip = 0f;
            sidewaysSlip = 0f;
            return;
        }
        forwardsSlip = (wheelHits[2].forwardSlip + wheelHits[3].forwardSlip) * 0.5f;
        sidewaysSlip = (wheelHits[2].sidewaysSlip + wheelHits[3].sidewaysSlip) * 0.5f;
    }


    // --- MISC SYNC METHODS ---
    public void SyncCarEffectsToOtherClients()
    {
        if (syncCarEffectsInterval > 0.02f)
        {
            if (syncedWheelRotation != steeringWheelAnimFloat)
            {
                syncCarEffectsInterval = 0f;
                syncedWheelRotation = steeringWheelAnimFloat;
                syncedPlayerSteeringAnim = playerSteeringWheelAnimFloat;
                SyncCarEffectsRpc(steeringWheelAnimFloat, playerSteeringWheelAnimFloat);
                return;
            }
        }
        else
        {
            syncCarEffectsInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarEffectsRpc(float wheelRotation, float playerSteering)
    {
        syncedWheelRotation = wheelRotation;
        syncedPlayerSteeringAnim = playerSteering;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncPedalInputsRpc(bool gasPressed, bool brakePressed)
    {
        drivePedalPressed = gasPressed;
        brakePedalPressed = brakePressed;
    }

    private void SyncCarPositionToOtherClients()
    {
        mainRigidbody.isKinematic = false;
        mainRigidbody.interpolation = RigidbodyInterpolation.None;

        //float syncThreshold = 0.12f * (averageVelocity.magnitude / 200f);
        //syncThreshold = Mathf.Clamp(syncThreshold, 0.06f, 0.125f);
        float syncThreshold = 0.12f;
        if (syncCarPositionInterval >= syncThreshold)
        {
            if (Vector3.Distance(this.syncedPosition, base.transform.position) > 0.02f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = base.transform.position;
                syncedRotation = base.transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles);
                return;
            }
            if (Vector3.Angle(base.transform.forward, this.syncedRotation * Vector3.forward) > 2f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = base.transform.position;
                syncedRotation = base.transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles);
                return;
            }
        }
        else
        {
            syncCarPositionInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarPositionRpc(Vector3 carPosition, Vector3 carRotation)
    {
        syncedPosition = carPosition;
        syncedRotation = Quaternion.Euler(carRotation);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetTyreStressRpc(float wheelStress, bool wheelSkidding)
    {
        tyreStress = wheelStress;
        wheelSlipping = wheelSkidding;
    }

    //public void SyncCarWheelSpeedToOtherClients()
    //{
    //    float syncThreshold = 0.12f * averageVelocity.magnitude;
    //    syncThreshold = Mathf.Clamp(syncThreshold, 0.12f, 0.35f);
    //    if (syncCarWheelSpeedInterval >= syncThreshold)
    //    {
    //        float wheelSyncRPM = Mathf.Floor(wheelRPM / 5f) * 5f;
    //        if (syncedWheelRPM != wheelSyncRPM)
    //        {
    //            syncCarWheelSpeedInterval = 0f;
    //            syncedWheelRPM = wheelSyncRPM;
    //            SyncCarWheelSpeedRpc(wheelRPM);
    //            return;
    //        }
    //    }
    //    else
    //    {
    //        syncCarWheelSpeedInterval += Time.deltaTime;
    //    }
    //}

    //[Rpc(SendTo.NotOwner, RequireOwnership = false)]
    //public void SyncCarWheelSpeedRpc(float wheelSpeed)
    //{
    //    syncedWheelRPM = wheelSpeed;
    //}

    //public void SyncCarEngineSpeedToOtherClients()
    //{
    //    if (!ignitionStarted)
    //        return;

    //    float syncThreshold = 0.1f * averageVelocity.magnitude;
    //    syncThreshold = Mathf.Clamp(syncThreshold, 0.1f, 0.35f);
    //    if (syncCarDrivetrainInterval >= syncThreshold)
    //    {
    //        float engineSpeedToSync = Mathf.Floor(Mathf.Round(EngineRPM / 10f) * 10f);
    //        if (syncedEngineRPM != engineSpeedToSync)
    //        {
    //            syncCarDrivetrainInterval = 0f;
    //            syncedEngineRPM = engineSpeedToSync;
    //            SyncCarEngineSpeedRpc(EngineRPM);
    //            return;
    //        }
    //    }
    //    else
    //    {
    //        syncCarDrivetrainInterval += Time.deltaTime;
    //    }
    //}

    //[Rpc(SendTo.NotOwner, RequireOwnership = false)]
    //public void SyncCarEngineSpeedRpc(float engineSpeed)
    //{
    //    syncedEngineRPM = engineSpeed;
    //}

    public void SyncCarDrivetrainToOtherClients()
    {
        float syncThreshold = 0.14f * averageVelocity.magnitude;
        syncThreshold = Mathf.Clamp(syncThreshold, 0.14f, 0.34f);
        if (syncCarDrivetrainInterval >= syncThreshold)
        {
            float wheelSyncRPM = Mathf.Floor(wheelRPM / 5f) * 5f;
            if (syncedWheelRPM != wheelSyncRPM)
            {
                syncCarDrivetrainInterval = 0f;
                syncedWheelRPM = wheelSyncRPM;
                syncedEngineRPM = wheelSyncRPM;
                SyncCarDrivetrainRpc(wheelRPM);
                return;
            }
        }
        else
        {
            syncCarDrivetrainInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarDrivetrainRpc(float wheelSpeed)
    {
        syncedWheelRPM = wheelSpeed;
        syncedEngineRPM = wheelSpeed;
    }

    public void SyncCarWheelTorqueToOtherClients()
    {
        float syncThreshold = 0.04f * averageVelocity.magnitude;
        syncThreshold = Mathf.Clamp(syncThreshold, 0.04f, 0.3f);
        if (syncWheelTorqueInterval >= syncThreshold)
        {
            float motorTorqueSync = Mathf.Floor(wheelTorque / 10f) * 10f;
            float brakeTorqueSync = Mathf.Floor(wheelBrakeTorque / 10f) * 10f;

            if (syncedMotorTorque != motorTorqueSync || 
                syncedBrakeTorque != brakeTorqueSync)
            {
                syncWheelTorqueInterval = 0f;
                syncedMotorTorque = motorTorqueSync;
                syncedBrakeTorque = brakeTorqueSync;
                SyncWheelTorqueRpc(wheelTorque, wheelBrakeTorque);
                return;
            }
        }
        else
        {
            syncWheelTorqueInterval += Time.deltaTime;
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
        currentInterior.carKeySounds.mute = inOrbit;
        rollingAudio.mute = inOrbit;
        skiddingAudio.mute = inOrbit;
        turbulenceAudio.mute = inOrbit;
        hoodFireAudio.mute = inOrbit;
        extremeStressAudio.mute = inOrbit;
        pushAudio.mute = inOrbit;
        radioAudio.mute = inOrbit;
        radioInterference.mute = inOrbit;

        if (currentDriver != null && References.lastDriver != currentDriver && !magnetedToShip)
            References.lastDriver = currentDriver;

        if (honkingHorn && hornAudio.isPlaying && hornAudio.pitch < 1f)
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
                CarBumpRpc(averageVelocity * 0.7f);
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
                        enemyHitSpeed = 1f; // 9f
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else if (obstacleSize <= 2f)
                    {
                        enemyHitSpeed = 9f; // 16f
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else
                    {
                        enemyHitSpeed = 15f; // 21f
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
                            CarBumpRpc(averageVelocity);
                            mainRigidbody.velocity = Vector3.Normalize(-impulse * 100000000f) * 9f;
                            PlayerControllerB playerControllerB = currentDriver != null ? currentDriver : currentPassenger;
                            if (vel.magnitude > 2f && dealDamage)
                            {
                                enemyScript.HitEnemyOnLocalClient(2, Vector3.zero, playerControllerB, playHitSFX: true, 331);
                            }
                            result = true;
                            if (truckType == TruckVersionType.V70 && obstacleSize > 2f)
                            {
                                DealPermanentDamage(1, position);
                            }
                        }
                        if (truckType != TruckVersionType.V70) DealPermanentDamage(2, position);
                    }
                    else
                    {
                        mainRigidbody.AddForce(Vector3.Normalize(-impulse * 1E+09f) * (carReactToPlayerHitMultiplier - 220f), ForceMode.Impulse);
                        if (dealDamage)
                        {
                            DealPermanentDamage(1, position);
                        }
                        if (truckType == TruckVersionType.V70 || enemyScript is GiantKiwiAI)
                        {
                            PlayerControllerB playerWhoHit = currentDriver != null ? currentDriver : currentPassenger;
                            enemyScript.HitEnemyOnLocalClient(12, Vector3.zero, playerWhoHit, false, -1);
                        }
                        else
                        {
                            enemyScript.KillEnemyOnOwnerClient();
                        }
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
        if (!base.IsOwner)
            return;

        if (magnetedToShip || !hasBeenSpawned)
            return;

        if (collision.collider.gameObject.layer != 8)
            return;

        float highestImpulse = 0f;
        int contactCount = collision.GetContacts(contacts);
        Vector3 averageContactPoint = Vector3.zero;

        for (int i = 0; i < contactCount; i++)
        {
            if (contacts[i].impulse.magnitude > highestImpulse)
            {
                highestImpulse = contacts[i].impulse.magnitude;
            }
            averageContactPoint += contacts[i].point;
        }

        averageContactPoint /= (float)contactCount;
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
                DestroyCar();
                DestroyCarRpc();
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
            PlayCollisionAudio(averageContactPoint, bumpSeverity, collisionVolume);
            if (highestImpulse > maximumBumpForce + 10000f && averageVelocity.magnitude > 19f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f));
                BreakWindshield();
                CarCollisionRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f));
                DealPermanentDamage(2);
            }
            else
            {
                CarBumpRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 40f));
            }
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarBumpRpc(Vector3 vel)
    {
        if ((localPlayerInControl || localPlayerInPassengerSeat) && vel.magnitude >= 50f)
        {
            GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
            return;
        }
        if (!VehicleUtils.IsPlayerInVehicleBounds())
            return;


        if (PlayerUtils.isPlayerInStorage) vel = Vector3.ClampMagnitude(vel, 30f);
        GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarCollisionRpc(Vector3 vel)
    {
        DamagePlayerInVehicle(vel);
        BreakWindshield();
    }

    private void DamagePlayerInVehicle(Vector3 vel)
    {
        if (localPlayerInPassengerSeat || localPlayerInControl)
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, spawnBody: true, CauseOfDeath.Inertia, 0, transform.up * 0.77f);
            return;
        }
        if (!VehicleUtils.IsPlayerInVehicleBounds())
            return;
        if (GameNetworkManager.Instance.localPlayerController.health <= 40)
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, spawnBody: true, CauseOfDeath.Inertia, 0, transform.up * 0.77f);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.DamagePlayer(30, hasDamageSFX: true, callRPC: true, CauseOfDeath.Inertia, 0, fallDamage: false, vel);
        GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
    }

    private new void BreakWindshield()
    {
        if (windshieldBroken)
            return;

        windshieldBroken = true;
        windshieldPhysicsCollider.enabled = false;
        windshieldMesh.SetActive(value: false);

        glassParticle.Play();
        miscAudio.PlayOneShot(windshieldBreak);
    }

    public new void PlayCollisionAudio(Vector3 setPosition, int audioType, float setVolume)
    {
        if (Time.realtimeSinceStartup - audio1Time > Time.realtimeSinceStartup - audio2Time)
        {
            bool audioTime = Time.realtimeSinceStartup - audio1Time >= collisionAudio1.clip.length * 0.8f;
            if (audio1Type <= audioType || audioTime)
            {
                audio1Time = Time.realtimeSinceStartup;
                audio1Type = audioType;
                collisionAudio1.transform.position = setPosition;
                PlayRandomClipAndPropertiesFromAudio(collisionAudio1, setVolume, audioTime, audioType);
                CarCollisionSFXRpc(collisionAudio1.transform.localPosition, 0, audioType, setVolume);
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
                PlayRandomClipAndPropertiesFromAudio(collisionAudio2, setVolume, audioTime, audioType);
                CarCollisionSFXRpc(collisionAudio2.transform.localPosition, 1, audioType, setVolume);
            }
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarCollisionSFXRpc(Vector3 audioPosition, int audio, int audioType, float vol)
    {
        AudioSource audioSource = ((audio != 0) ? collisionAudio2 : collisionAudio1);
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

        if (ignitionStarted)
        {
            if (collisionType >= 2)
            {
                RoundManager.Instance.PlayAudibleNoise(
                    engineAudio1.transform.position,
                    18f + volume * 7f,
                    0.6f,
                    0,
                    noiseIsInsideClosedShip: false,
                    2692
                );
            }
            else if (collisionType >= 1)
            {
                RoundManager.Instance.PlayAudibleNoise(
                    engineAudio1.transform.position,
                    12f + volume * 7f,
                    0.6f,
                    0,
                    noiseIsInsideClosedShip: false,
                    2692
                );
            }
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
        if (StartOfRound.Instance.inShipPhase)
            return;

        if (carStressIncrease <= 0f) carStressChange = Mathf.Clamp(carStressChange - Time.deltaTime, -0.25f, 0.5f);
        else carStressChange = Mathf.Clamp(carStressChange + Time.deltaTime * carStressIncrease, 0f, 10f);

        underExtremeStress = carStressIncrease >= 1f;
        carStress = Mathf.Clamp(carStress + carStressChange, 0f, 100f);

        if (carStress < 7f)
            return;

        carStress = 0f;
        DealPermanentDamage(2);
        lastDamageType = "Stress";
    }

    public new void DealPermanentDamage(int damageAmount, Vector3 damagePosition = default(Vector3))
    {
        if (StartOfRound.Instance.inShipPhase || magnetedToShip ||
            carDestroyed || !base.IsOwner)
            return;

        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP -= damageAmount;
        syncedCarHP = carHP;
        if (carHP <= 0)
        {
            DestroyCar();
            DestroyCarRpc();
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
    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void DestroyCarRpc()
    {
        if (carDestroyed)
            return;

        DestroyCar();
    }

    public new void DestroyCar()
    {
        if (carDestroyed)
            return;

        carDestroyed = true;
        magnetedToShip = false;
        StartOfRound.Instance.isObjectAttachedToMagnet = false;
        RemoveCarRainCollision();
        CollectItemsInTruck();
        underExtremeStress = false;
        hoodPoppedUp = true;
        keyObject.enabled = false;
        engineAudio1.Stop();
        engineAudio2.Stop();
        turbulenceAudio.Stop();
        rollingAudio.Stop();
        radioAudio.Stop();
        radioInterference.Stop();
        extremeStressAudio.Stop();
        currentInterior.carKeySounds.Stop();
        honkingHorn = false;
        hornAudio.Stop();
        tireSparks.Stop();
        skiddingAudio.Stop();
        turboBoostAudio.Stop();
        turboBoostParticle.Stop();
        RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 20f, 0.8f, 0, noiseIsInsideClosedShip: false, 2692);
        FrontLeftWheel.motorTorque = 0f;
        FrontRightWheel.motorTorque = 0f;
        BackLeftWheel.motorTorque = 0f;
        BackRightWheel.motorTorque = 0f;
        FrontLeftWheel.brakeTorque = 0f;
        FrontRightWheel.brakeTorque = 0f;
        BackLeftWheel.brakeTorque = 0f;
        BackRightWheel.brakeTorque = 0f;
        FrontLeftWheel.enabled = false;
        FrontRightWheel.enabled = false;
        BackLeftWheel.enabled = false;
        BackRightWheel.enabled = false;
        leftWheelMesh.enabled = false;
        rightWheelMesh.enabled = false;
        backLeftWheelMesh.enabled = false;
        backRightWheelMesh.enabled = false;
        carHoodAnimator.gameObject.SetActive(false);
        backDoorContainer.SetActive(value: false);
        backDoorOpen = true;
        headlightsContainer.SetActive(value: false);
        backLightsContainer.SetActive(value: false);
        BreakWindshield();
        if (isInteriorRHD)
        {
            destroyedTruckMesh_1.SetActive(value: true);
            destroyedTruckMesh.SetActive(value: false);
        }
        else
        {
            destroyedTruckMesh_1.SetActive(value: false);
            destroyedTruckMesh.SetActive(value: true);
        }
        radarMapIcon.enabled = false;
        radarMapDestroyedIcon.enabled = true;
        mainBodyMesh.gameObject.SetActive(value: false);
        for (int disableDestroy = 0; disableDestroy < disableOnDestroy.Length; disableDestroy++)
        {
            disableOnDestroy[disableDestroy].SetActive(false);
        }
        mainRigidbody.AddForceAtPosition(Vector3.up * 1560f, hoodFireAudio.transform.position - Vector3.up, ForceMode.Impulse);
        SetIgnition(started: false, cabLightOn: false);
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        keyIsInDriverHand = false;
        keyIsInIgnition = false;
        keyIgnitionCoroutine = null;
        EngineRPM = 0f;
        wheelRPM = 0f;

        if (localPlayerInControl || localPlayerInPassengerSeat)
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.up * 27f + 20f * UnityEngine.Random.insideUnitSphere, spawnBody: true, CauseOfDeath.Blast, 6, Vector3.up * 1.5f);
        }
        InteractTrigger[] componentsInChildren2 = gameObject.GetComponentsInChildren<InteractTrigger>();
        for (int k = 0; k < componentsInChildren2.Length; k++)
        {
            componentsInChildren2[k].interactable = false;
            componentsInChildren2[k].CancelAnimationExternally();
        }

        driverSeatTrigger.interactable = false;
        passengerSeatTrigger.interactable = false;

        currentDriver = null!;
        currentPassenger = null!;

        Landmine.SpawnExplosion(transform.position + transform.forward + Vector3.up * 1.5f, spawnExplosionEffect: true, 6f, 10f, 30, 200f, truckDestroyedExplosion, goThroughCar: true);
        mainRigidbody.AddExplosionForce(800f * 50f, transform.position, 12f, 3f * 6f, ForceMode.Impulse);
        pushTruckTrigger.interactable = true;
    }


    // --- REMOVAL MISC ---
    public void RemoveCarRainCollision()
    {
        var particleTriggers = new[]
        {
           ScandalsTweaks.Utils.References.rainParticles.trigger,
           ScandalsTweaks.Utils.References.rainHitParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainHitParticles.trigger
        };

        foreach (var trigger in particleTriggers)
        {
            for (int i = trigger.colliderCount - 1; i >= 0; i--)
            {
                var collider = (Collider)trigger.GetCollider(i);
                if (weatherEffectBlockers.Contains(collider))
                {
                    trigger.RemoveCollider(i);
                }
            }
        }
    }


    // --- IDK ---
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

        if (!base.IsOwner)
            return;
        if (carHP < 7 && Time.realtimeSinceStartup - timeAtLastDamage > 16f)
        {
            timeAtLastDamage = Time.realtimeSinceStartup;
            carHP++;
            syncedCarHP = carHP;
            SyncCarHealthRpc(carHP);
        }
        if (carDestroyed)
        {
            if (carHP < 3)
            {
                if (!isHoodOnFire)
                {
                    isHoodOnFire = true;
                    hoodFireAudio.Play();
                    hoodFireParticle.Play();
                    SetHoodOnFireRpc(isHoodOnFire);
                }
            }
            else if (isHoodOnFire && carHP >= 3)
            {
                isHoodOnFire = false;
                hoodFireAudio.Stop();
                hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
                SetHoodOnFireRpc(isHoodOnFire);
            }
            return;
        }
        if (carHP < 3)
        {
            if (!isHoodOnFire)
            {
                if (!hoodPoppedUp)
                {
                    hoodPoppedUp = true;
                    SetHoodOpenLocalClient(setOpen: true);
                }
                isHoodOnFire = true;
                hoodFireAudio.Play();
                hoodFireParticle.Play();
                SetHoodOnFireRpc(isHoodOnFire);
            }
        }
        else if (isHoodOnFire)
        {
            isHoodOnFire = false;
            hoodPoppedUp = false;
            hoodFireAudio.Stop();
            hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            SetHoodOnFireRpc(isHoodOnFire);
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    private void SyncCarHealthRpc(int carHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        syncedCarHP = carHealth;
        carHP = syncedCarHP;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetHoodOnFireRpc(bool onFire)
    {
        isHoodOnFire = onFire;
        if (isHoodOnFire)
        {
            hoodFireAudio.Play();
            hoodFireParticle.Play();
            return;
        }
        hoodFireAudio.Stop();
        hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
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


    // --- PUSH METHODS ---
    public new void PushTruckWithArms()
    {
        if (magnetedToShip)
            return;

        if (!Physics.Raycast(
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position,
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward,
            out hit,
            10f,
            1073742656,
            QueryTriggerInteraction.Ignore))
            return;

        if (GameNetworkManager.Instance.localPlayerController.overridePhysicsParent != null)
            return;
        if (VehicleUtils.IsPlayerInVehicleBounds())
            return;

        Vector3 point = hit.point;
        Vector3 forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;
        int clip = UnityEngine.Random.Range(0, minCollisions.Length);

        if (base.IsOwner)
        {
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(forward * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, point - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
            PushTruckFromOwnerRpc(point, clip);
            return;
        }
        PushTruckRpc(point, forward, clip);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushTruckRpc(Vector3 pushPosition, Vector3 dir, int clip)
    {
        pushAudio.clip = minCollisions[clip];
        pushAudio.transform.position = pushPosition;
        pushAudio.Play();
        turbulenceAmount = Mathf.Min(turbulenceAmount + 0.5f, 2f);
        if (base.IsOwner)
        {
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(dir * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, pushPosition - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushTruckFromOwnerRpc(Vector3 pos, int clip)
    {
        pushAudio.clip = minCollisions[clip];
        pushAudio.transform.position = pos;
        pushAudio.Play();
        turbulenceAmount = Mathf.Min(turbulenceAmount + 0.5f, 2f);
    }


    // --- HOOD THINGAMJIG ---
    public new void ToggleHoodOpenLocalClient()
    {
        RoundManager.Instance.PlayAudibleNoise(carHoodAnimator.gameObject.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        carHoodOpen = !carHoodOpen;
        if (!carHoodOpen) hoodPoppedUp = false;
        carHoodAnimator.SetBool("hoodOpen", carHoodOpen);
        SetHoodOpenRpc(carHoodOpen);
    }

    public new void SetHoodOpenLocalClient(bool setOpen)
    {
        if (carHoodOpen == setOpen)
            return;

        RoundManager.Instance.PlayAudibleNoise(carHoodAnimator.gameObject.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        carHoodOpen = setOpen;
        carHoodAnimator.SetBool("hoodOpen", setOpen);
        SetHoodOpenRpc(open: true);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHoodOpenRpc(bool open)
    {
        if (carHoodOpen == open)
            return;

        RoundManager.Instance.PlayAudibleNoise(carHoodAnimator.gameObject.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        carHoodOpen = open;
        if (!carHoodOpen) hoodPoppedUp = false;
        carHoodAnimator.SetBool("hoodOpen", open);
    }


    // --- INTERIOR BUTTON ANIMATIONS ---
    public void UseButtonOnLocalClient(string triggerString)
    {
        if (currentInterior?.verticalColumnAnimator == null)
            return;

        currentInterior?.verticalColumnAnimator.SetTrigger(triggerString);
    }


    public void ToggleWipersOnLocalClient()
    {
        currentInterior.wiperToggleAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickWiperButton");
        ToggleWipersRpc();
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ToggleWipersRpc()
    {
        currentInterior.wiperToggleAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickWiperButton");
    }


    public void OpenCabinWindowOnLocalClient()
    {
        currentInterior.cabinWindowToggleAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickCabinButton");
        OpenCabinWindowRpc();
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OpenCabinWindowRpc()
    {
        currentInterior.cabinWindowToggleAudio.PlayOneShot(dashboardButton);
        UseButtonOnLocalClient("clickCabinButton");
    }


    // --- HEADLAMPS ---
    public new void ToggleHeadlightsLocalClient()
    {
        headlightsContainer.SetActive(!headlightsContainer.activeSelf);
        currentInterior.headlightToggleAudio.PlayOneShot(headlightsToggleSFX);
        SetHeadlightMaterial(headlightsContainer.activeSelf);
        UseButtonOnLocalClient("clickLightButton");
        ToggleHeadlightsRpc(headlightsContainer.activeSelf);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ToggleHeadlightsRpc(bool setLightsOn)
    {
        headlightsContainer.SetActive(setLightsOn);
        currentInterior.headlightToggleAudio.PlayOneShot(headlightsToggleSFX);
        SetHeadlightMaterial(setLightsOn);
        UseButtonOnLocalClient("clickLightButton");
    }

    private new void SetHeadlightMaterial(bool on)
    {
        Material headlightMat = on ? headlightsOnMat : headlightsOffMat;
        Material[] truckBodyMats = mainBodyMesh.sharedMaterials;
        truckBodyMats[1] = headlightMat;

        mainBodyMesh.sharedMaterials = truckBodyMats;
        lod1Mesh.sharedMaterials = truckBodyMats;
        lod2Mesh.sharedMaterials = truckBodyMats;
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
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

        timeSinceSpringingDriverSeat = Time.realtimeSinceStartup;
        springAudio.Play();
        currentInterior.ejectorButtonAudio.PlayOneShot(dashboardButton);
        driverSeatSpringAnimator.SetTrigger("spring");
        currentInterior.ejectorButtonAnimator.SetTrigger("press");
        RoundManager.Instance.PlayAudibleNoise(springAudio.transform.position, 30f, 1f, 0, noiseIsInsideClosedShip: false, 2692);

        if (player == null)
            return;

        if (!player.isPlayerControlled)
            return;

        if (player.isPlayerDead)
            return;

        if (localPlayerInControl ||
            Vector3.Distance(player.transform.position, springAudio.transform.position) < 0.9f) //|| Vector3.Distance(player.transform.localPosition, springAudio.transform.localPosition) < 1f
        {
            player.externalForceAutoFade += (transform.up * springForce);
        }
    }
}