using System;
using System.IO;
using System.Collections.Generic;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;

namespace DesktopDup
{
    public abstract class RLocalInput
    {
        public RLocalInputMap map;

        public void ApplyMap(RLocalInputMap map)
        {
            this.map = map;
        }

        public void ApplyDefaultMap()
        {
            RLocalInputMap map = BuildDefaultMap();
            ApplyMap(map);
        }

        public abstract RLocalInputMap BuildDefaultMap();
        public abstract List<RLocalButtonState> PollInputs(bool raw = false);
        public abstract RLocalButtonState PollSingleDownInput();
    }

    public class RLocalGamepadInput : RLocalInput
    {
        private DirectInput directInput;
        private Joystick gamepad;

        public RLocalGamepadInput()
        {
            directInput = new DirectInput();
            var joystickGuid = Guid.Empty;
            ApplyDefaultMap();
        }

        public List<RLocalGamepadDescription> GetAvailableGamepads()
        {
            var devices = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
            var gamepads = new List<RLocalGamepadDescription>();

            foreach (var deviceInstance in devices)
            {
                var gamepad = new RLocalGamepadDescription();
                gamepad.name = deviceInstance.ProductName;
                gamepad.guid = deviceInstance.InstanceGuid;
                gamepads.Add(gamepad);
            }

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

    /*public class RLocalKeyboardInput : RLocalInput
    {
        private Keyboard keyboard = null;
        RLocalInputMap map;

        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;

        public RLocalKeyboardInput()
        {
            var directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            keyboard.Properties.BufferSize = 128;
            keyboard.Acquire();
        }

        ~RLocalKeyboardInput()
        {
            if (keyboard != null) keyboard.Dispose();
        }

        public override List<RLocalButtonState> PollInputs(bool raw = false)
        {
            List<RLocalButtonState> buttonStates = new List<RLocalButtonState>();
            KeyboardUpdate[] data = keyboard.GetBufferedData();
            foreach (KeyboardUpdate update in data)
            {
                RLocalButtonState buttonState = KeyStateToStructure(update, raw);
                if (buttonState != null)
                {
                    buttonStates.Add(buttonState);
                }
            }
            return buttonStates;
        }

        public RLocalButtonState KeyStateToStructure(KeyboardUpdate update, bool raw)
        {
            RLocalButtonState buttonState;

            if (raw)
            {
                buttonState = new RLocalButtonState();
                buttonState.rawInput = update.Key.ToString();
                buttonState.isPressed = update.IsPressed;
                return buttonState;
            }
            else
            {
                int button;
                bool pressed = update.IsPressed;

                if (!map.TryGetValue(update.Key.ToString(), out button)) return null;

                if (button >= 1000)
                {
                    switch (button)
                    {
                        case 1000:
                            up = pressed; break;
                        case 1002:
                            right = pressed; break;
                        case 1004:
                            down = pressed; break;
                        case 1006:
                            left = pressed; break;
                    }

                    if (up || right || down || left) pressed = true;
                    else pressed = false;

                    if (up && right) button = 1001;
                    else if (right && down) button = 1003;
                    else if (down && left) button = 1005;
                    else if (left && up) button = 1007;
                    else
                    {
                        button = up ? 1000 : right ? 1002 : down ? 1004 : left ? 1006 : button;
                    }
                }

                buttonState = new RLocalButtonState();
                buttonState.button = button;
                buttonState.isPressed = pressed;
            }

            return buttonState;
        }

        public override RLocalButtonState PollSingleDownInput()
        {
            List<RLocalButtonState> buttonStates = null;
            int count = 0;
            while (count == 0)
            {
                buttonStates = PollInputs();
                count = buttonStates.Count;
            }
            return buttonStates[0];
        }

        public override RLocalInputMap BuildDefaultMap()
        {
            RLocalInputMap map = new RLocalInputMap();

            map.Add("J", 1);         // X
            map.Add("K", 2);         // A
            map.Add("L", 3);         // B
            map.Add("I", 4);         // Y
            map.Add("U", 5);         // LB
            map.Add("O", 6);         // RB 
            map.Add("D7", 7);        // LT
            map.Add("D9", 8);        // RT
            map.Add("Backslash", 9); // Back
            map.Add("Return", 10);   // Start
            map.Add("V", 10);        // Start
            map.Add("W", 1000);      // Up, Right, Down, Left
            map.Add("D", 1002);
            map.Add("S", 1004);
            map.Add("A", 1006);

            return map;
        }

        public void KeyStateToBytes(KeyboardUpdate update)
        {
            Console.WriteLine(update);
            Console.WriteLine(update.Key);
            Console.WriteLine(update.IsPressed);
            Console.WriteLine(update.IsReleased);
            Console.WriteLine();
        }
    }*/

    public class RLocalButtonState
    {
        public int button;
        public string rawInput = "null";
        public int value;

        public const int packetSize = sizeof(int) * 4;

        public byte[] ToPacket()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(9);
            writer.Write(sizeof(int) + sizeof(int));
            writer.Write(button);
            writer.Write(value);

            BinaryReader reader = new BinaryReader(writer.BaseStream);
            reader.BaseStream.Position = 0;
            return reader.ReadBytes(packetSize);
        }

        public static RLocalButtonState FromPacket(byte[] packet)
        {
            RLocalButtonState buttonState = new RLocalButtonState();

            buttonState.button = BitConverter.ToInt32(packet, 8);
            buttonState.value = BitConverter.ToInt32(packet, 12);

            return buttonState;
        }

        public void Print()
        {
            Console.WriteLine("Button: " + button + " - " + value + "  (Raw: " + rawInput + ")");
        }
    }

    public class RLocalInputMap : Dictionary<string, int>
    {

    }

    public struct RLocalGamepadDescription
    {
        public string name;
        public Guid guid;
    }
}
