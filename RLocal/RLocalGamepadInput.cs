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
            this.map = BuildDefaultMap();
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
            gamepad.Acquire();
        }

        public override List<RLocalButtonState> PollInputs(bool raw = false)
        {
            List<RLocalButtonState> buttonStates = new List<RLocalButtonState>();
            JoystickUpdate[] data = gamepad.GetBufferedData();
            foreach (JoystickUpdate update in data)
            {
                Console.WriteLine(update.Offset + " " + update.Value);
                RLocalButtonState buttonState = KeyStateToStructure(update, raw);
                if (buttonState != null)
                {
                    buttonStates.Add(buttonState);
                }
            }
            return buttonStates;
        }

        public RLocalButtonState KeyStateToStructure(JoystickUpdate update, bool raw)
        {
            var buttonState = new RLocalButtonState();
            buttonState.value = update.Value;

            string buttonName = update.Offset.ToString();
            buttonState.rawInput = buttonName;

            if (!map.TryGetValue(buttonName, out buttonState.button)) return null;

            return buttonState;
        }

        public override RLocalButtonState PollSingleDownInput()
        {
            return new RLocalButtonState();
        }

        public override RLocalInputMap BuildDefaultMap()
        {
            RLocalInputMap map = new RLocalInputMap();

            map.Add("Buttons0", 1);                     // X
            map.Add("Buttons1", 2);                     // A
            map.Add("Buttons2", 3);                     // B
            map.Add("Buttons3", 4);                     // Y
            map.Add("Buttons4", 5);                     // LB
            map.Add("Buttons5", 6);                     // RB 
            map.Add("Buttons6", 7);                     // LT
            map.Add("Buttons7", 8);                     // RT
            map.Add("Buttons8", 9);                     // Back
            map.Add("Buttons9", 10);                    // Start

            map.Add("PointOfViewControllers0", 1000);   // Directions

            return map;
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
