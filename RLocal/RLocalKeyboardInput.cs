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
        //RLocalInputMap map;

        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;

        public RLocalKeyboardInput()
        {
            this.map = BuildDefaultMap();
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
            int button;
            if (!map.TryGetValue(update.Key.ToString(), out button)) return null;

            var buttonState = new RLocalButtonState();

            buttonState.rawInput = update.Key.ToString();
            buttonState.value = update.IsPressed ? 128 : 0;
            buttonState.button = button;

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
    }
}
