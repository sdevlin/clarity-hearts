using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cmd = System.Console;

namespace Console
{
    internal static class Printer
    {
        static Printer()
        {
            cmd.CursorVisible = false;
        }

        public static void PrintInfo(object value)
        {
            PrintInfo(() => cmd.WriteLine(value));
        }

        public static void PrintInfo(string format, params object[] args)
        {
            PrintInfo(() => cmd.WriteLine(format, args));
        }

        private static void PrintInfo(Action print)
        {
            var prevColor = cmd.ForegroundColor;
            cmd.ForegroundColor = ConsoleColor.Green;
            print();
            cmd.ForegroundColor = prevColor;
        }

        public static void PrintError(object value)
        {
            PrintError(() => cmd.WriteLine(value));
        }

        public static void PrintError(string format, params object[] args)
        {
            PrintError(() => cmd.WriteLine(format, args));
        }

        private static void PrintError(Action print)
        {
            var prevColor = cmd.ForegroundColor;
            cmd.ForegroundColor = ConsoleColor.Red;
            cmd.Beep();
            cmd.WriteLine();
            print();
            cmd.WriteLine();
            cmd.ForegroundColor = prevColor;
        }
    }
}
