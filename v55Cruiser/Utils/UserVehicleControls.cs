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

            [InputAction(KeyboardControl.S, Name = "Brake Pedal", GamepadControl = GamepadControl.LeftTrigger)]
            public InputAction BrakePedalKey { get; set; } = null!;

            [InputAction(KeyboardControl.D, Name = "Steer Right", GamepadControl = GamepadControl.RightStick)]
            public InputAction SteerRightKey { get; set; } = null!;

            //[InputAction(KeyboardControl.None, Name = "Jump/Boost (V56+)", GamepadControl = GamepadControl.ButtonNorth)]
            //public InputAction TurboKey { get; set; } = null!;
        }
    }
}
