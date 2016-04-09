using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace DesktopDup
{
    /// <summary>
    ///   Screen capture of the desktop using DXGI OutputDuplication.
    /// </summary>
    internal static class RLocalMain
    {
        //[STAThread]
        private static void Main()
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            Application.Run(new RLocal());
        }
    }
}
