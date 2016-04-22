using System;
using System.IO;
using System.Collections.Generic;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;

namespace RLocal
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

    public enum RLocalInputDeviceType
    {
        KEYBOARD,
        GAMEPAD
    }

    public struct RLocalInputDescription
    {
        public RLocalInputDeviceType type;
        public string name;
        public RLocalInput device;
        public RLocalKeyboardInput keyboardDevice;
        public RLocalGamepadDescription gamepadDescription;
    }

    public class RLocalInputManager
    {
        List<RLocalInputDescription> devices;
        RLocalKeyboardInput keyboard;
        RLocalGamepadInput gamepad;

        public List<RLocalInputDescription> DiscoverInputDevices()
        {
            var devices = new List<RLocalInputDescription>();

            var keyboard = new RLocalInputDescription();
            var keyboardInput = new RLocalKeyboardInput();
            keyboard.type = RLocalInputDeviceType.KEYBOARD;
            keyboard.name = "Keyboard";
            keyboard.keyboardDevice = keyboardInput;
            devices.Add(keyboard);

            var gamepads = RLocalGamepadInput.GetAvailableGamepads();
            gamepads.ForEach(gamepad =>
            {
                var inputDescription = new RLocalInputDescription();

                inputDescription.name = gamepad.name;
                inputDescription.type = RLocalInputDeviceType.GAMEPAD;
                inputDescription.gamepadDescription = gamepad;

                devices.Add(inputDescription);
            });

            this.devices = devices;
            return devices;
        }

        public void AssignDevice(int index)
        {
            var input = devices[index];
            switch (input.type)
            {
                case RLocalInputDeviceType.KEYBOARD:
                    AssignKeyboard(input.keyboardDevice);
                    break;
                case RLocalInputDeviceType.GAMEPAD:
                    AssignGamepad(input.gamepadDescription);
                    break;
            }
        }

        public void AssignKeyboard(RLocalKeyboardInput keyboard)
        {
            this.keyboard = keyboard;
            keyboard.Acquire();
        }

        public void AssignGamepad(RLocalGamepadDescription gamepadDescription)
        {
            gamepad = new RLocalGamepadInput();
            gamepad.AssignGamepad(gamepadDescription);
        }

        public List<RLocalButtonState> PollInputs()
        {
            if (keyboard != null) return keyboard.PollInputs();
            if (gamepad != null) return gamepad.PollInputs();
            else
            {
                throw new Exception("PollInputs() called on RLocalInputManager when no device has been assigned.");
            }
        }

        public RLocalButtonState PollSingleDownInput()
        {
            if (keyboard != null) return keyboard.PollSingleDownInput();
            if (gamepad != null) return gamepad.PollSingleDownInput();
            else
            {
                throw new Exception("PollSingleDownInput() called on RLocalInputManager when no device has been assigned.");
            }
        }
    }
}
