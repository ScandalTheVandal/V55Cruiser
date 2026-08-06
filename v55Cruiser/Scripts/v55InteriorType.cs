using UnityEngine;

namespace v55Cruiser.Scripts;

public class v55InteriorType : MonoBehaviour
{
    public v55SeatAnimator driverSeat = null!;
    public v55SeatAnimator passengerSeat = null!;

    public InteractTrigger driverSeatTrigger = null!;
    public InteractTrigger passengerSeatTrigger = null!;

    public Animator gearStickAnimator = null!;

    public Transform windWiper1 = null!;
    public Transform windWiper2 = null!;

    public Animator driverSeatSpringAnimator = null!;

    public float cameraLookAngle;
}