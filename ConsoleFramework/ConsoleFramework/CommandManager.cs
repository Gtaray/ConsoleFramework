using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFramework
{
    public class CommandManager
    {
        private string _name;
        private string _description;
        private string _synopsis;
        private Dictionary<string, ConsoleCommand> _commands = new Dictionary<string, ConsoleCommand>(StringComparer.OrdinalIgnoreCase);
        private Func<int> _helpFunc;
        private Dictionary<int, string> _errorCodes;
        private ConsoleCommand _defaultCommand;
        private bool hasDefaultCommand = false;
        private string _version = "";

        #region Accessors
        /// <summary>
        ///  Name of the program
        /// </summary>
        public string ApplicationName
        {
            get { return _name; }
            set
            {
                _name = value;
                ConsoleLogger.CmdManagerName = value;
            }
        }

        /// <summary>
        ///  Summary of the syntax of the program. Ex: proc.exe [COMMAND] [SWITCHES]
        /// </summary>
        public string ApplicationSynopsis
        {
            get { return _synopsis; }
            set { _synopsis = value; }
        }

        /// <summary>
        ///  Longer description of the program
        /// </summary>
        public string ApplicationDescription
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        // Function that is run when "help" is invoked by -h or -?. This is provided by default but can be overridden.
        /// </summary>
        public Func<int> HelpFunction
        {
            get { return _helpFunc; }
            set { _helpFunc = value; }
        }

        /// <summary>
        /// Collection of error codes that the program can return, as well as the text returned by the program when that error code is returned.
        /// /// </summary>
        public Dictionary<int, string> ErrorCodes
        {
            get { return _errorCodes; }
            set { _errorCodes = value; }
        }

        /// <summary>
        /// Collections of Command objects, organized by their invocation string (a.k.a "verb").
        /// </summary>
        public Dictionary<string, ConsoleCommand> Commands
        {
            get { return _commands; }
        }

        /// <summary>
        /// The default command that is run if no verb is necessary to run the program (ex: ping.exe)
        /// </summary>
        public ConsoleCommand DefaultCommand
        {
            get { return _defaultCommand; }
            set
            {
                hasDefaultCommand = value == null ? false : true;
                _defaultCommand = value;
                if (_defaultCommand != null)
                {
                    _defaultCommand.CmdManager = this;
                    _defaultCommand.InvokeCommand = "Default Function";
                }
            }
        }

        /// <summary>
        /// Version of the program
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Command Manager object that handles all of the command line argument parsing and function execution, including help text display, version numbering, error code management, and more.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="Synopsis"></param>
        public CommandManager(string Name, string Description, string Synopsis)
        {
            ApplicationSynopsis = Synopsis;
            ApplicationName = Name;
            ApplicationDescription = Description;
            HelpFunction = ShowHelp;
            ErrorCodes = new Dictionary<int, string>();
        }
        #endregion

        #region Execution
        /// <summary>
        /// Add a command to the list of commands that can be executed by command line.
        /// </summary>
        /// <param name="cmd">The command object to be added.</param>
        /// <returns>True if the command was added successfully. False if the command could not be added due to duplicate or null command string.</returns>
        public bool AddCommand(ConsoleCommand cmd)
        {

            if (_commands.ContainsKey(cmd.InvokeCommand))
            {
                ConsoleLogger.PrintDebug("Tried to add a command with an invocation keyword that already exists in another command: " + cmd.InvokeCommand);
                return false;
            }

            if (string.IsNullOrEmpty(cmd.InvokeCommand))
            {
                ConsoleLogger.PrintDebug("Failed to add a command that contains no invocation keyword with description: " + cmd.Description);
                return false;
            }

            cmd.CmdManager = this;
            _commands.Add(cmd.InvokeCommand, cmd);
            return true;
        }

        /// <summary>
        /// Executes a command based on the parameters passed in by the user
        /// </summary>
        /// <param name="args">String array that is passed in by the user from the command line (exactly the same as args[] passed into the Main function).</param>
        /// <returns>True if the command was executed successfully. Successful execution is defined in your command functions.</returns>
        public int ExecuteCommand(string[] args)
        {
            try
            {
                int CRESULT;
                if (args.Contains("-h") || args.Contains("-?"))
                    return HelpFunction.Invoke();

                // check if there's a default command
                // and if there is, check if we use the default or another command
                // by checking if there is either:
                // A) There are no args, in which case the default is the only option.
                // B) There are args, but the first one is a switch
                if (hasDefaultCommand && UseDefaultCommand(args))
                {
                    CRESULT = DefaultCommand.ExecuteCommand(args);
                }
                else
                {
                    if (args.Length == 0)
                    {
                        ConsoleLogger.PrintError("No command found.");
                        return HelpFunction.Invoke();
                    }
                    string cmd = args[0].ToLower();

                    if (!_commands.ContainsKey(cmd))
                    {
                        ConsoleLogger.PrintError("Unknown command \"" + cmd + "\".");
                        return HelpFunction.Invoke();
                    }

                    string[] switches = new string[args.Length - 1];
                    Array.Copy(args, 1, switches, 0, args.Length - 1);

                    // Cmd is guaranteed to only contain 1 element at this point
                    // Get the first element in cmd where the Command invocation string equals cmd, and execute that command, passing in switches.
                    CRESULT = _commands[cmd].ExecuteCommand(switches);
                }

                if (CRESULT != 0)
                {
                    if (ErrorCodes.ContainsKey(CRESULT))
                    {
                        ConsoleLogger.PrintError(ErrorCodes[CRESULT]);
                    }
                    else
                    {
                        ConsoleLogger.PrintDebug("WARNING: Returning an error code not defined in CommandManager.ErrorCodes: " + CRESULT);
                    }
                }

                return CRESULT;
            }
            catch (Exception e)
            {
                ConsoleLogger.PrintDebug("FATAL ERROR: " + e.ToString());
                ConsoleLogger.PrintDebug(e.StackTrace);
                return 1;
            }
        }

        private bool UseDefaultCommand(string[] args)
        {
            return args.Length == 0 || !_commands.ContainsKey(args[0]);
        }
        #endregion

        #region Default command functions
        /// <summary>
        /// Shows the help for this command line utility tool. It prints out the help display, and can be overridden.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private int ShowHelp()
        {
            ConsoleLogger.PrintLine("");
            ConsoleLogger.PrintLine("NAME");
            ConsoleLogger.PrintLine(_name + " Version " + Version);
            ConsoleLogger.PrintLine("");
            ConsoleLogger.PrintLine("SYNOPSIS");
            ConsoleLogger.PrintLine(_synopsis);
            ConsoleLogger.PrintLine("");
            ConsoleLogger.PrintLine("DESCRIPTION");
            ConsoleLogger.PrintLine(_description);
            ConsoleLogger.PrintLine("");
            ConsoleLogger.PrintLine("COMMANDS");
            if (hasDefaultCommand)
            {
                ConsoleLogger.PrintLine(DefaultCommand.ToString());
                ConsoleLogger.PrintLine("");
            }
            foreach (ConsoleCommand c in _commands.Values)
            {
                ConsoleLogger.PrintLine(c.ToString());
                ConsoleLogger.PrintLine("");
            }

            return 0;
        }
        #endregion
    }
}
