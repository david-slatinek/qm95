using System;
using static System.Console;

namespace qm95
{
    class Program
    {
        private static void Help()
        {
            WriteLine("Name");
            WriteLine("\tA private banking system");

            WriteLine("Flags");
            WriteLine("-h");
            WriteLine("\tPrint help");
            WriteLine("-L");
            WriteLine("\tLogin");
            WriteLine("-r");
            WriteLine("\tRegister");
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void Login()
        {
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void Register()
        {
        }

        private static void Main(string[] args)
        {
            Title = "qm95";

            if (args.Length == 0)
            {
                Help();
                Environment.Exit(0);
            }

            switch (args[0])
            {
                case "-h":
                    Help();
                    Environment.Exit(0);
                    break;

                case "-L":
                    Login();
                    break;

                case "-r":
                    Register();
                    break;

                default:
                    WriteLine("Invalid flag");
                    Help();
                    Environment.Exit(0);
                    break;
            }
        }
    }
}