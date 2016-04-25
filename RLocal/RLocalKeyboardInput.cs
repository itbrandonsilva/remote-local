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
    public class RLocalKeyboardInput : RLocalInput
    {
        private Keyboard keyboard = null;
        public RLocalInputMap map;

        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;

        bool hasBeenPolled = false;

        public RLocalKeyboardInput()
        {
            this.map = BuildDefaultMap();
        }

        public void ApplyMap(RLocalInputMap map)
        {
            this.map = map;
        }

        public void ApplyDefaultMap()
        {
            RLocalInputMap map = BuildDefaultMap();
            ApplyMap(map);
        }

        public void Acquire()
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

        public List<RLocalButtonState> GenerateNeutralAxisInputs()
        {
            var buttonStates = new List<RLocalButtonState>();

            var buttons = new string[] { "X", "Y", "RotationX", "RotationY" };
            for (int i = 0; i < buttons.Length; ++i) {
                var buttonState = new RLocalButtonState();
                buttonState.button = RLocalInput.MapButtonToId[buttons[i]];
                buttonState.value = ushort.MaxValue / 2;
                buttonStates.Add(buttonState);
            }

            return buttonStates;
        }

        public override List<RLocalButtonState> PollInputs(bool raw = false)
        {
            List<RLocalButtonState> buttonStates = new List<RLocalButtonState>();

            if (!hasBeenPolled)
            {
                hasBeenPolled = true;
                buttonStates.AddRange(GenerateNeutralAxisInputs());
            }

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
            int button;
            if (!map.TryGetValue(update.Key.ToString(), out button)) return null;

            var buttonState = new RLocalButtonState();
            buttonState.button = button;
            buttonState.rawInput = update.Key.ToString();

            if (button == 1300) {
                buttonState.button = RLocalInput.MapButtonToId["Z"];
                buttonState.value = update.IsPressed ? ushort.MaxValue : 0;
                return buttonState;
            }
            else if (button == 1301)
            {
                buttonState.button = RLocalInput.MapButtonToId["RotationZ"];
                buttonState.value = update.IsPressed ? ushort.MaxValue : 0;
                return buttonState;
            }
            else
            {
                buttonState.value = update.IsPressed ? 128 : 0;
            }

            if (button >= 1000)
            {
                int angle = -1;
                switch (button)
                {
                    case 1000:
                        up = update.IsPressed; break;
                    case 1002:
                        right = update.IsPressed; break;
                    case 1004:
                        down = update.IsPressed; break;
                    case 1006:
                        left = update.IsPressed; break;
                }

                bool isPressed = (up || right || down || left);

                if (isPressed)
                {
                    if (up && right) button = 1001;
                    else if (right && down) button = 1003;
                    else if (down && left) button = 1005;
                    else if (left && up) button = 1007;
                    else
                    {
                        button = up ? 1000 : right ? 1002 : down ? 1004 : left ? 1006 : button;
                    }

                    if (isPressed)
                    {
                        switch (button)
                        {
                            case 1000: angle = 0; break;
                            case 1001: angle = 4500; break;
                            case 1002: angle = 9000; break;
                            case 1003: angle = 13500; break;
                            case 1004: angle = 18000; break;
                            case 1005: angle = 22500; break;
                            case 1006: angle = 27000; break;
                            case 1007: angle = 31500; break;
                        }
                    }
                }

                buttonState.button = 1100;
                buttonState.value = angle;
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

        public RLocalInputMap BuildDefaultMap()
        {
            RLocalInputMap map = new RLocalInputMap();

            map.Add("J", 0);         // X
            map.Add("K", 1);         // A
            map.Add("L", 2);         // B
            map.Add("I", 3);         // Y
            map.Add("U", 4);         // LB
            map.Add("O", 5);         // RB 
            map.Add("Backslash", 6); // Back
            map.Add("Return", 7);   // Start
            map.Add("V", 7);        // Start
            map.Add("W", 1000);      // Up, Right, Down, Left
            map.Add("D", 1002);
            map.Add("S", 1004);
            map.Add("A", 1006);

            map.Add("D7", 1300);        // LT
            map.Add("D9", 1301);        // RT

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
    }
}
