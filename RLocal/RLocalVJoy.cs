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

        public static readonly Dictionary<string, HID_USAGES> MapButtonToAxis = new Dictionary<string, HID_USAGES>
        {
            { "X",          HID_USAGES.HID_USAGE_X },
            { "Y",          HID_USAGES.HID_USAGE_Y },
            { "Z",          HID_USAGES.HID_USAGE_Z },
            { "RotationX",  HID_USAGES.HID_USAGE_RX },
            { "RotationY",  HID_USAGES.HID_USAGE_RY },
            { "RotationZ",  HID_USAGES.HID_USAGE_RZ },
        };

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

        public void SetButtonState(uint id, string button, int value)
        {
            if (button.Contains("Buttons"))
            {
                vjoy.SetBtn(value > 0, id, (uint)RLocalInput.MapButtonToId[button]+1);
            }
            else if (button.Contains("PointOfViewControllers"))
            {
                vjoy.SetContPov(value, id, 1);
            } else {
                var axis = MapButtonToAxis[button];
                vjoy.SetAxis(value/2, id, axis);
            }
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
