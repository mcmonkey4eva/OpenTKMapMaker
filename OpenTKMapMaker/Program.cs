using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker
{
    static class Program
    {
        public static string GameName = "OpenTK Map Maker";

        [DllImport("kernel32.dll",
        EntryPoint = "AllocConsole",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        public static FileHandler Files;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AllocConsole();
            SysConsole.Init();
            Files = new FileHandler();
            Files.Init();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PrimaryEditor());
        }
    }
}
