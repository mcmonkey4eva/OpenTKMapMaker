using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace OpenTKMapMaker.Utility
{
    public class SysConsole
    {
        volatile static string Waiting = "";

        static Object ConsoleLock;

        static Object WriteLock;

        static Thread ConsoleOutputThread;

        /// <summary>
        /// Prepares the system console.
        /// </summary>
        public static void Init()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Preparing console...");
            Console.WriteLine("Starting system...");
            ConsoleLock = new Object();
            WriteLock = new Object();
            ConsoleOutputThread = new Thread(new ThreadStart(ConsoleLoop));
            //Program.ThreadsToClose.Add(ConsoleOutputThread);
            ConsoleOutputThread.Start();
            Output(OutputType.INIT, "Console prepared...");
            Output(OutputType.INIT, "Test colors: ^r^7Text Colors: ^0^h^1^^n1 ^!^^n! ^2^^n2 ^@^^n@ ^3^^n3 ^#^^n# ^4^^n4 ^$^^n$ ^5^^n5 ^%^^n% ^6^^n6 ^-^^n- ^7^^n7 ^&^^n& ^8^^n8 ^*^^** ^9^^n9 ^(^^n( ^&^h^0^^n0^h ^)^^n) ^a^^na ^A^^nA\n" +
                            "^r^7Text styles: ^b^^nb is bold,^r ^i^^ni is italic,^r ^u^^nu is underline,^r ^s^^ns is strike-through,^r ^O^^nO is overline,^r ^7^h^0^^nh is highlight,^r^7 ^j^^nj is jello (AKA jiggle),^r " +
                            "^7^h^2^e^0^^ne is emphasis,^r^7 ^t^^nt is transparent,^r ^T^^nT is more transparent,^r ^o^^no is opaque,^r ^R^^nR is random,^r ^p^^np is pseudo-random,^r ^^nk is obfuscated (^kobfu^r),^r " +
                            "^^nS is ^SSuperScript^r, ^^nl is ^lSubScript (AKA Lower-Text)^r, ^h^8^d^^nd is Drop-Shadow,^r^7 ^f^^nf is flip,^r ^^nr is regular text, ^^nq is a ^qquote^q, and ^^nn is nothing (escape-symbol).");
        }

        static void ConsoleLoop()
        {
            while (true)
            {
                string twaiting;
                lock (ConsoleLock)
                {
                    twaiting = Waiting;
                    Waiting = "";
                }
                if (twaiting.Length > 0)
                {
                    // TODO: Log file control! Option to change file name or disable entirely...
                    // Also options to put a value like logs/%yyyy%/%mm%/%dd%.log
                    // TODO: Handle less terribly. Particular multiple-games-running logging
                    // FileHandler.AppendText("console.log", twaiting);
                    lock (WriteLock)
                    {
                        WriteInternal(twaiting);
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// The console title.
        /// </summary>
        public static string Title = "";

        /// <summary>
        /// Fixes the title of the system console to how the Client expects it.
        /// </summary>
        public static void FixTitle()
        {
            Title = Program.GameName + " / " + Process.GetCurrentProcess().Id.ToString();
            Console.Title = Title;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Hides the system console from view.
        /// </summary>
        public static void HideConsole()
        {
            // TODO ShowWindow(Program.ConsoleHandle, 0);
        }

        /// <summary>
        /// Shows (un-hides) the system console.
        /// </summary>
        public static void ShowConsole()
        {
            // TODO ShowWindow(Program.ConsoleHandle, 1);
        }

        /// <summary>
        /// Writes a line of colored text to the system console.
        /// </summary>
        /// <param name="text">The text to write</param>
        public static void WriteLine(string text)
        {
            Write(text + "\n");
        }

        /// <summary>
        /// Writes some colored text to the system console.
        /// </summary>
        /// <param name="text">The text to write</param>
        private static void Write(string text)
        {
            lock (ConsoleLock)
            {
                Waiting += text;
            }
        }

        static void WriteInternal(string text)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            StringBuilder outme = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '^' && i + 1 < text.Length && Utilities.IsColorSymbol(text[i + 1]))
                {
                    if (outme.Length > 0)
                    {
                        Console.Write(outme);
                        outme.Clear();
                    }
                    i++;
                    switch (text[i])
                    {
                        case '0': Console.ForegroundColor = ConsoleColor.Black; break;
                        case '1': Console.ForegroundColor = ConsoleColor.Red; break;
                        case '2': Console.ForegroundColor = ConsoleColor.Green; break;
                        case '3': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case '4': Console.ForegroundColor = ConsoleColor.Blue; break;
                        case '5': Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case '6': Console.ForegroundColor = ConsoleColor.Magenta; break;
                        case '7': Console.ForegroundColor = ConsoleColor.White; break;
                        case '8': Console.ForegroundColor = ConsoleColor.Magenta; break;
                        case '9': Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case 'a': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case ')': Console.ForegroundColor = ConsoleColor.DarkGray; break;
                        case '!': Console.ForegroundColor = ConsoleColor.DarkRed; break;
                        case '@': Console.ForegroundColor = ConsoleColor.DarkGreen; break;
                        case '#': Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                        case '$': Console.ForegroundColor = ConsoleColor.DarkBlue; break;
                        case '%': Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                        case '-': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                        case '&': Console.ForegroundColor = ConsoleColor.Gray; break;
                        case '*': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                        case '(': Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                        case 'A': Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                        case 'b': break;
                        case 'i': break;
                        case 'u': break;
                        case 's': break;
                        case 'O': break;
                        case 'j': break;
                        case 'e': break;
                        case 't': break;
                        case 'T': break;
                        case 'o': break;
                        case 'R': break;
                        case 'p': break; // TODO: Probably shouldn't be implemented, but... it's possible
                        case 'k': break;
                        case 'S': break;
                        case 'l': break;
                        case 'd': break;
                        case 'f': break;
                        case 'n': break;
                        case 'q': outme.Append('"'); break;
                        case 'r': Console.BackgroundColor = ConsoleColor.Black; break;
                        case 'h': Console.BackgroundColor = Console.ForegroundColor; break;
                        default: outme.Append("INVALID-COLOR-CHAR:" + text[i] + "?"); break;
                    }
                }
                else
                {
                    outme.Append(text[i]);
                }
            }
            if (outme.Length > 0)
            {
                Console.Write(outme);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(">");// TODO + ConsoleHandler.read);
        }

        /// <summary>
        /// Properly formats system console output.
        /// </summary>
        /// <param name="ot">What type of output to use</param>
        /// <param name="text">The text to output</param>
        public static void Output(OutputType ot, string text)
        {
            if (OutputColors[(int)ot] == "^7")
            {
                WriteLine("^r^7" + Utilities.DateTimeToString(DateTime.Now) + " [" + OutputNames[(int)ot] + "] " + text);
            }
            else
            {
                WriteLine("^r^7" + Utilities.DateTimeToString(DateTime.Now) + " [" + OutputColors[(int)ot] +
                    OutputNames[(int)ot] + "^7] " + OutputColors[(int)ot] + text);
            }
        }

        static string[] OutputColors = new string[]
        {
            "^7ERROR:OUTPUTTYPE=NONE?",
            "^7",
            "^2",
            "^3",
            "^7^h^0",
            "^7",
        };

        static string[] OutputNames = new string[]
        {
            "NONE",
            "INFO/CLIENT",
            "INIT",
            "WARNING",
            "ERROR",
            "INFO",
        };
    }

    /// <summary>
    /// All possible console output types.
    /// </summary>
    public enum OutputType : int
    {
        /// <summary>
        /// Do not use.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// When the client is sending information to console.
        /// </summary>
        CLIENTINFO = 1,
        /// <summary>
        /// During the startup sequence.
        /// </summary>
        INIT = 2,
        /// <summary>
        /// An ignorable error.
        /// </summary>
        WARNING = 3,
        /// <summary>
        /// A major error.
        /// </summary>
        ERROR = 4,
        /// <summary>
        /// General information.
        /// </summary>
        INFO = 5,
        // TODO: More?
    }
}
