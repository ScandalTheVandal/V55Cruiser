using UnityEngine.InputSystem;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;

namespace v55Cruiser.Utils
{
    internal class UserVehicleControls
    {
        internal static VehicleControls VehicleControlsInstance = null!;

        internal class VehicleControls : LcInputActions
        {
            [InputAction(KeyboardControl.W, Name = "Gas Pedal", GamepadControl = GamepadControl.RightTrigger)]
            public InputAction GasPedalKey { get; set; } = null!;

            [InputAction(KeyboardControl.A, Name = "Steer Left", GamepadControl = GamepadControl.LeftStick)]
            public InputAction SteerLeftKey { get; set; } = null!;

            [InputAction(KeyboardControl.S, Name = "Brake", GamepadControl = GamepadControl.LeftTrigger)]
            public InputAction BrakePedalKey { get; set; } = null!;

            [InputAction(KeyboardControl.D, Name = "Steer Right", GamepadControl = GamepadControl.RightStick)]
            public InputAction SteerRightKey { get; set; } = null!;

            [InputAction(KeyboardControl.None, Name = "Jump/Boost (V56+)", GamepadControl = GamepadControl.ButtonNorth)]
            public InputAction TurboKey { get; set; } = null!;

            [InputAction(MouseControl.ScrollUp, Name = "Shift Gear Forward", GamepadControl = GamepadControl.LeftShoulder)]
            public InputAction GearShiftForwardKey { get; set; } = null!;

            [InputAction(MouseControl.ScrollDown, Name = "Shift Gear Backward", GamepadControl = GamepadControl.RightShoulder)]
            public InputAction GearShiftBackwardKey { get; set; } = null!;

            //[InputAction(KeyboardControl.None, Name = "Center Steering Wheel")]
            //public InputAction WheelCenterKey { get; set; } = null!;

            //[InputAction(KeyboardControl.L, Name = "Headlights")]
            //public InputAction ToggleHeadlightsKey { get; set; } = null!;

            //[InputAction(KeyboardControl.H, Name = "Horn")]
            //public InputAction ActivateHornKey { get; set; } = null!;

            //[InputAction(KeyboardControl.None, Name = "Wipers")]
            //public InputAction ToggleWipersKey { get; set; } = null!;

            //[InputAction(KeyboardControl.None, Name = "Magnet")]
            //public InputAction ToggleMagnetKey { get; set; } = null!;
        }
    }
}
