using UnityEngine;
using UnityEngine.Scripting.APIUpdating;


namespace v55Cruiser.Behaviour;
[MovedFrom("v55Cruiser.MonoBehaviours.Vehicles.v55Cruiser")]

public class v55InteriorType : MonoBehaviour
{
    public Animator gearStickAnimator = null!;
    public Animator steeringWheelAnimator = null!;

    public Animator verticalColumnAnimator = null!;

    public GameObject carKeyContainer = null!;
    public GameObject ignitionBarrel = null!;

    public GameObject startKeyIgnitionTrigger = null!;
    public GameObject removeKeyIgnitionTrigger = null!;

    public AudioSource wiperToggleAudio = null!;
    public AudioSource cabinWindowToggleAudio = null!;
    public AudioSource headlightToggleAudio = null!;
    public AudioSource carKeySounds = null!;

    public Transform ignitionBarrelNotTurnedPosition = null!;
    public Transform ignitionBarrelTurnedPosition = null!;

    public Transform ignitionNotTurnedPosition = null!;
    public Transform ignitionTurnedPosition = null!;

    public InteractTrigger driverSeatTrigger = null!;
    public InteractTrigger passengerSeatTrigger = null!;

    public Transform windWiper1 = null!;
    public Transform windWiper2 = null!;

    public Animator driverSeatSpringAnimator = null!;
    public Animator ejectorButtonAnimator = null!;
    public AudioSource springAudio = null!;
    public AudioSource ejectorButtonAudio = null!;

    //52 for left-hand-drive
    public float cameraLookAngle;
}