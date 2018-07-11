using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PlannerExAndImport
{
    // This gets parameters and allows the user to select what he wants to do
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Planner Ex- and Import (" + Assembly.GetExecutingAssembly().GetName().Version + ")");

            // expects 2 arguments (tenant and client id) and tries to falls back to env variables
            if (args.Length != 2)
            {
                if (Environment.GetEnvironmentVariable("PEaI_TENANT") != null 
                    && Environment.GetEnvironmentVariable("PEaI_CLIENT_ID") != null)
                {
                    Planner.TENANT = Environment.GetEnvironmentVariable("PEaI_TENANT");
                    Planner.CLIENT_ID = Environment.GetEnvironmentVariable("PEaI_CLIENT_ID");
                }
                else
                {
                    Help();
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            else
            {
                Planner.TENANT = args[0];
                Planner.CLIENT_ID = args[1];
            }
            
            // select the action and call appropriate methods
            try
            {
                while (true)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Select action by entering the letter:");
                    Console.WriteLine("E) export plan");
                    Console.WriteLine("I) export and then import plan");
                    Console.WriteLine("F) forget stored credentials");
                    Console.WriteLine("H) help");
                    Console.WriteLine("X) exit");

                    string selected = GetInput().ToLower();

                    switch (selected)
                    {
                        case "e":
                            Planner.Export();
                            break;
                        case "i":
                            Planner.Import();
                            break;
                        case "f":
                            Planner.ForgetCredentials();
                            break;
                        case "h":
                            Help();
                            break;
                        case "x":
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Invalid entry. Please enter e, i, f, h or x");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }

        private static void Help()
        {
            Console.WriteLine("PlannerExAndImport.exe <tenant id> <client id>, e.g. PlannerExAndImport.exe yourtenant.com 0d0c56f0-1444-412a-94a5-c8df7e54958c");
        }

        public static string GetInput(string message = null)
        {
            if (message != null)
                Console.Write(message);
            return Console.ReadLine();
        }

        public static void PrintError(Exception exc)
        {
            ConsoleColor before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Something went wrong.");
            Console.WriteLine("Message: " + exc.Message + "\n");
            Console.ForegroundColor = before;
            Console.ReadLine();
        }
    }
}
