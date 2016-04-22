using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

using vJoyInterfaceWrap;

namespace RLocal
{
    class RLocalVJoy
    {
        private vJoy vjoy;

        public RLocalVJoy()
        {
            vjoy = new vJoy();
            ValidateHost();

            //vJoy.JoystickState joystickState = new vJoy.JoystickState();
        }

        public void AcquireDevice(uint id)
        {
            bool res = vjoy.AcquireVJD(id);
            vjoy.ResetVJD(id);
            Console.WriteLine("Res: " + res);
        }

        public void SetButtonState(uint id, int button, int value)
        {
            if (button < 1000)
            {
                bool res = vjoy.SetBtn(value > 0, id, (uint)button);
            }
            else
            {
                /*int angle = -1;
                if (pressed)
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
                }*/
                vjoy.SetContPov(value, id, 1);
            }
            //vjoy.SetDiscPov(pressed, id, 1);
            //vjoy.SetDiscPov(pressed, id, 2);
            //vjoy.SetDiscPov(pressed, id, 3);
            //vjoy.SetDiscPov(pressed, id, 4);
        }

        public void ValidateHost()
        {
            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!vjoy.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else {
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
                    vjoy.GetvJoyManufacturerString(),
                    vjoy.GetvJoyProductString(),
                    vjoy.GetvJoySerialNumberString());
            }

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = vjoy.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);
        }
    }
}
