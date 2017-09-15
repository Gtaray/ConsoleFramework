using ConsoleFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleConsoleApp
{
    class Program
    {
        public static CommandManager ConsoleManager;
        static int Main(string[] args)
        {
            ConsoleManager = new CommandManager(
                "SampleConsoleApp",
                "This exe demonstrates the ways in which you can use the CommandManager to abstract your console application's command line parsing",
                "SampleConsoleApp.exe"
                );
            ConsoleManager.DefaultCommand = new ConsoleCommand(
                "Prints out a nice welcome",    // Description of the command
                "",                             // Null because its the default command, it isn't invoked with a keyword
                DefaultCommand,                 // Function that is executed
                new ConsoleSwitch[]             // List of switches that the function can use
                {
                    new ConsoleSwitch("n", "Your name", false, false)
                }
            );

            ConsoleManager.AddCommand(new ConsoleCommand(
                "Adds two numbers",
                "ADD",
                Add,
                new ConsoleSwitch[]
                {
                    new ConsoleSwitch("a", "First number", true, false),
                    new ConsoleSwitch("b", "Second number", true, false)
                }
            ));

            // Define error codes, so that if a function returns a number other than 1,
            // the error for that number is automatically printed out
            ConsoleManager.ErrorCodes = new Dictionary<int, string>()
            {
                { 1, "Parameter was not a valid integer" }
            };

            // Accept commands and execute them until an empty line is entered, then exit.
            string line = Console.ReadLine().Trim();
            do
            {
                if(!string.IsNullOrEmpty(line))
                {
                    ConsoleManager.ExecuteCommand(line.Split(' '));
                }
                line = Console.ReadLine();
            } while (!string.IsNullOrEmpty(line));
            
            return 0;
        }

        // The Default Functions is invoked if no command is passed to the executable.
        // Like all other functions that are fired by the Commandmanager, it needs to
        // return an integer (0 = success, any other number equals failure), and accepts
        // the ConsoleArguments object as a parameter
        private static int DefaultCommand(ConsoleArguments args)
        {
            string name = args["n"];
            ConsoleLogger.PrintLine(string.Format("Hello {0}", name));
            return 0;
        }

        private static int Add(ConsoleArguments args)
        {
            int a, b;
            if (!Int32.TryParse(args["a"], out a) || !Int32.TryParse(args["b"], out b))
                return 1;

            ConsoleLogger.PrintLine(string.Format("{0} + {1} = {2}", a, b, (a + b)));
            return 0;
        }
    }
}
