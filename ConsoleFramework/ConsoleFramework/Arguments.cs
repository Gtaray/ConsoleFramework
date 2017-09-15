using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFramework
{
    public class ConsoleArguments
    {
        private Dictionary<string, string> _arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private List<string> _nonParsedArgs = new List<string>();
        private ConsoleCommand _cmd;

        #region Accessors
        /// <summary>
        /// Accesses a switch's value based on its tag (ex. string flag = args["f"])
        /// </summary>
        /// <param name="key">Tag of the switch to access the argument value from</param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                if (_arguments.ContainsKey(key.ToLower()))
                    return _arguments[key.ToLower()];
                else
                    return null;
            }
        }

        /// <summary>
        /// Number of switches passed in
        /// </summary>
        public int Length
        {
            get { return _arguments.Count; }
        }

        /// <summary>
        /// Array of all the tags of the switches contained in this argument object
        /// </summary>
        public string[] SwitchTags
        {
            get { return _arguments.Keys.ToArray(); }
        }

        /// <summary>
        /// The list of all raw arguments passed in after the switches (ie where lists of files are normally passed)
        /// </summary>
        public List<string> NonParsedArgs
        {
            get { return _nonParsedArgs; }
        }
        #endregion

        #region Constructors
        public ConsoleArguments(string[] args, ConsoleCommand cmd, out bool success)
        {
            _cmd = cmd;

            if (args.Length == 0)
            {
                success = true;
                return;
            }

            string tag = "";
            // If the first argument does not have the syntax of a switch, assume it and everything else is a non-parsed argument.
            // 0 - tag ; 1 - value ; 2 - files
            int state = (args[0].Length == 2 && args[0].StartsWith("-")) ? 0 : 2;

            // Cycle through each argument and put it in the _arguments dictionary
            foreach (string arg in args)
            {
                // check if the arg is double dash, which means to stop parsing switches, and start parsing files
                if (arg == "--")
                {
                    state = 2;
                    continue;
                }

                // Test if the arg is a tag, but should be a value
                if (state == 1 && arg.Length == 2 && arg.StartsWith("-"))
                {
                    state = 0;
                }

                // Under these conditions, change to file parsing
                if (state == 0 && arg.Length > 2 && !arg.StartsWith("-"))
                    state = 2;

                switch (state)
                {
                    // Tag
                    case (0):
                        if (arg.Length != 2)
                        {
                            ConsoleLogger.PrintError("Invalid Input. Expected a switch statement.");
                            cmd.DisplayCmdManagerHelp();
                            success = false;
                            return;
                        }
                        tag = arg[1].ToString();

                        if (cmd.Switches.ContainsKey(tag))
                        {
                            // Test if the arguments already contains the tag
                            if (_arguments.ContainsKey(tag))
                            {
                                ConsoleLogger.PrintError("Invalid input. Found duplicate switch: " + arg);
                                cmd.DisplayCmdManagerHelp();
                                success = false;
                                return;
                            }

                            // If the switch is boolean, stay in state 0, otherwise go to state 1
                            if (cmd.Switches[tag].IsBoolean)
                            {
                                // Boolean flag, so we set it to true
                                _arguments.Add(tag, "true");
                            }
                            else
                            {
                                // Switch has a non boolean value, so we set it to null since set it in the next loop
                                _arguments.Add(tag, null);
                                state = 1;
                            }
                        }
                        else
                        {
                            ConsoleLogger.PrintError("Invalid input. Found switch that does not exist in the command: " + arg);
                            cmd.DisplayCmdManagerHelp();
                            success = false;
                            return;
                        }
                        break;
                    // Value
                    case (1):
                        if (cmd.Switches[tag].IsBoolean)
                        {
                            ConsoleLogger.PrintError("Invalid input. Found value for switch that does not accept values: -" + tag);
                            cmd.DisplayCmdManagerHelp();
                            success = false;
                            return;
                        }

                        _arguments[tag] = arg;
                        state = 0;
                        break;
                    // File
                    case (2):
                        _nonParsedArgs.Add(arg);
                        break;
                }
            }

            success = true;
        }
        #endregion

        #region Execution
        /// <summary>
        /// Determines if a specific key exists as an argument
        /// </summary>
        /// <param name="key">String to test if it is contained as an argument</param>
        /// <returns>True if key exists as an argument</returns>
        public bool Contains(string key)
        {
            return _arguments.ContainsKey(key);
        }
        #endregion
    }
}
