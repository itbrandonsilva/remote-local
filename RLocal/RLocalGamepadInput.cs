using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;

namespace RLocal
{
    public class RLocalGamepadInput : RLocalInput
    {
        private DirectInput directInput;
        private Joystick gamepad;

        public RLocalGamepadInput()
        {
            directInput = new DirectInput();
            var joystickGuid = Guid.Empty;
        }

        public static List<RLocalGamepadDescription> GetAvailableGamepads()
        {
            var directInput = new DirectInput();
            var gamepads = new List<RLocalGamepadDescription>();

            var deviceTypes = new DeviceType[] { DeviceType.Gamepad, DeviceType.Joystick, DeviceType.FirstPerson };
            Array.ForEach(deviceTypes, (type) =>
            {
                var devices = directInput.GetDevices(type, DeviceEnumerationFlags.AllDevices);
                foreach (var deviceInstance in devices)
                {
                    var gamepad = new RLocalGamepadDescription();
                    gamepad.name = deviceInstance.ProductName;
                    gamepad.guid = deviceInstance.InstanceGuid;
                    gamepads.Add(gamepad);
                }
            });

            return gamepads;
        }

        public void AssignGamepad(RLocalGamepadDescription gamepadDesc)
        {
            gamepad = new Joystick(directInput, gamepadDesc.guid);
            gamepad.Properties.BufferSize = 128;
            gamepad.Properties.DeadZone = 250;
            gamepad.Acquire();
        }

        public override List<RLocalButtonState> PollInputs(bool raw = false)
        {
            List<RLocalButtonState> buttonStates = new List<RLocalButtonState>();
            JoystickUpdate[] data = gamepad.GetBufferedData();
            foreach (JoystickUpdate update in data)
            {
                RLocalButtonState buttonState = KeyStateToStructure(update, raw);
                buttonStates.Add(buttonState);
            }
            return buttonStates;
        }

        public RLocalButtonState KeyStateToStructure(JoystickUpdate update, bool raw)
        {
            var buttonState = new RLocalButtonState();
            buttonState.value = update.Value;
            buttonState.rawInput = update.Offset.ToString();

            buttonState.button = RLocalInput.MapButtonToId[buttonState.rawInput];

            //if (!map.TryGetValue(buttonName, out buttonState.button)) return null;

            return buttonState;
        }

        public override RLocalButtonState PollSingleDownInput()
        {
            return new RLocalButtonState();
        }

        public void PrintGamepads(List<RLocalGamepadDescription> devices)
        {
            foreach (var deviceInstance in devices)
            {
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("DEVICE NAME: " + deviceInstance.name);
                Console.WriteLine("DEVICE GUID: " + deviceInstance.guid);
            }
        }
    }
}
