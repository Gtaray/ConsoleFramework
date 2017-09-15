using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFramework
{
    public class ConsoleCommand
    {
        private string _invokeCommand;           // The text that invokes this command
        private Dictionary<string, ConsoleSwitch> _switches; // The switches that this command accepts
        private string _description;                // The description of this command (used for the 'help' command)
        private Func<ConsoleArguments, int> _function;  // The function executed when this command is executed
        CommandManager _cmdManager;         // Pointer to the CommandManager
        private string _fileNotes;          // Notes about the files this command uses

        #region Accessors
        /// <summary>
        ///  String that is used by the user to invoke the command. Known as the "verb" for the command.
        /// </summary>
        public string InvokeCommand
        {
            get { return _invokeCommand; }
            set
            {
                _invokeCommand = value;
            }
        }

        /// <summary>
        /// Description of what the function does
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        public CommandManager CmdManager
        {
            set { _cmdManager = value; }
        }

        /// <summary>
        /// Collection of switches, organized by the character that identifies that switch (the "tag")
        /// </summary>
        public Dictionary<string, ConsoleSwitch> Switches
        {
            get { return _switches; }
        }

        /// <summary>
        /// Description of what files or strings to supply the program that are not parsed switches
        /// </summary>
        public string FileNotes
        {
            get { return _fileNotes; }
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ConsoleCommand c = obj as ConsoleCommand;
            if ((System.Object)c == null)
                return false;

            return string.Equals(InvokeCommand, c.InvokeCommand, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Converts the command to a string that is the help blurb for this command
        /// </summary>
        /// <returns>A string that is the help blurb for this command.</returns>
        public override string ToString()
        {
            string s = "";
            s += string.IsNullOrEmpty(InvokeCommand) ? "" : InvokeCommand + " - ";
            s += Description + "\n";
            foreach (ConsoleSwitch w in Switches.Values)
                s += "\t" + w.ToString() + "\n";
            if (!string.IsNullOrEmpty(FileNotes))
                s += FileNotes + "\n";
            return s;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a Command object that contains information about executed commands
        /// </summary>
        /// /// <param name="description">A string that describes the command's function. Displayed when help is invoked</param>
        /// <param name="invokeCommand">A string that is the command used by teh user to invoke this command.</param>
        /// <param name="function">The functino that is run when this command executes. Must accept a single Arguments object as a parameter, and return an integer.</param>
        /// <param name="switches">An array of Switch objects that contain inforamtion on the parameters accepted by this command.</param>
        public ConsoleCommand(string description, string invokeCommand, Func<ConsoleArguments, int> function, ConsoleSwitch[] switches)
        {
            _description = description;
            _invokeCommand = invokeCommand.ToLower();
            _switches = new Dictionary<string, ConsoleSwitch>();
            foreach (ConsoleSwitch s in switches)
            {
                if (s.Tag == "h")
                {
                    ConsoleLogger.PrintDebug("WARNING: Tried to add -h tag to " + invokeCommand + ". This is disallowed as it would overwrite the help switch.");
                    continue;
                }
                _switches.Add(s.Tag, s);
            }
            _function = function;
        }

        /// <summary>
        /// Creates a Command object that contains information about executed commands
        /// </summary>
        /// /// <param name="description">A string that describes the command's function. Displayed when help is invoked</param>
        /// <param name="invokeCommand">A string that is the command used by teh user to invoke this command.</param>
        /// <param name="function">The functino that is run when this command executes. Must accept a single Arguments object as a parameter, and return an integer.</param>
        /// <param name="switches">An array of Switch objects that contain inforamtion on the parameters accepted by this command.</param>
        /// <param name="fileNotes">String that is displayed with the help function that describes the files the user is expected to pass in the args.</param>
        public ConsoleCommand(string description, string invokeCommand, Func<ConsoleArguments, int> function, ConsoleSwitch[] switches, string fileNotes)
        {
            _description = description;
            _invokeCommand = invokeCommand.ToLower();
            _switches = new Dictionary<string, ConsoleSwitch>();
            _fileNotes = fileNotes;
            foreach (ConsoleSwitch s in switches)
            {
                if (s.Tag == "h")
                {
                    ConsoleLogger.PrintDebug("WARNING: Tried to add -h tag to " + invokeCommand + ". This is disallowed as it would overwrite the help switch.");
                    continue;
                }
                _switches.Add(s.Tag, s);
            }
            _function = function;
        }
        #endregion

        #region Execution
        /// <summary>
        /// Executes this command
        /// </summary>
        /// <param name="args">Array of string _arguments input by the user (does NOT contain the command verb). This ONLY contains switches</param>
        /// <returns>And int based on if the command was executed successfully. Successful execution is defined in your command functions.</returns>
        public int ExecuteCommand(string[] args)
        {
            bool argumentsPassed = false;
            ConsoleArguments arguments = new ConsoleArguments(args, this, out argumentsPassed);

            if (!argumentsPassed)
            {
                return DisplayCmdManagerHelp();
            }

            // Test for help flag
            if (arguments.Contains("h"))
                return DisplayCmdManagerHelp();

            if (!(RequiredSwitchesExist(arguments) && SwitchesHaveValues(arguments)))
            {
                return DisplayCmdManagerHelp();
            }

            return _function.Invoke(arguments);
        }

        /// <summary>
        /// Test whether switches in an Arguments object that are required for this command exist
        /// </summary>
        /// <param name="arguments">Arguments object that contains switches to test.</param>
        /// <returns>True if all switches that are required exist</returns>
        private bool RequiredSwitchesExist(ConsoleArguments arguments)
        {
            foreach (ConsoleSwitch s in Switches.Values)
            {
                if (s.IsRequired && !arguments.Contains(s.Tag))
                {
                    ConsoleLogger.PrintError("The " + InvokeCommand + " command requires the -" + s.Tag + " switch.");
                    return false;
                }
            }

            return true;
        }

        private bool SwitchesHaveValues(ConsoleArguments arguments)
        {
            foreach (ConsoleSwitch s in Switches.Values)
            {
                // at this point we've confirmed that required switches exist, so we can make sure to only test 
                // optional switches if they exist, and are not boolean
                if (s.IsBoolean || !arguments.Contains(s.Tag))
                    continue;

                if (arguments[s.Tag] == null)
                {
                    ConsoleLogger.PrintError("No value found for the -" + s.Tag + " switch.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Executes the CommandManager's Help Function. NOTE: Do not call this function from the Command Manager's Help function 
        /// </summary>
        /// <returns>Always returns 0</returns>
        public int DisplayCmdManagerHelp()
        {
            return _cmdManager.HelpFunction.Invoke();
        }
        #endregion
    }
}
