using System;
using System.Runtime.InteropServices;

namespace RLocal
{
    class RLocalConsoleManager
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        bool hidden = true;

        public RLocalConsoleManager()
        {

        }

        public void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SW_SHOW);
            }
        }

        public void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        public void ToggleConsoleWindow()
        {
            hidden = !hidden;
            if (hidden)
            {
                HideConsoleWindow();
            }
            else
            {
                ShowConsoleWindow();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
