using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFramework
{
    public class ConsoleSwitch
    {
        private string _tag;          // tags used with this command. The tag is the first part of a switch, before the colon
        private bool _required;          // Whether this switch is required for the command to execute
        private bool _isBoolean;         // Whether this switch contains a value, or is a modifier
        private string _description;     // Description of what the switch does and how its used (used for the 'help' command)

        #region Accessors
        /// <summary>
        /// Character that identifies the switch (the "a" in -a when invoking a command)
        /// </summary>
        public string Tag
        {
            get { return _tag; }
        }

        /// <summary>
        /// Boolean flag as to whether this switch is required for its function to be invoked.
        /// </summary>
        public bool IsRequired
        {
            get { return _required; }
        }

        /// <summary>
        ///  Boolean flag as to whether this switch represents a boolean flag, and whether it should expect a value to follow it
        /// </summary>
        public bool IsBoolean
        {
            get { return _isBoolean; }
        }

        /// <summary>
        /// Descriptin of what this flag is/does
        /// </summary>
        public string Description
        {
            get { return _description; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a Switch object. Switches contain parameter information for command line functions
        /// </summary>
        /// <param name="tag">A single character that represents a command line parameter for a command</param>
        /// <param name="description">Description of the switch's function</param>
        /// <param name="required">Flag whether the switch is required for the command to execute. Default is false.</param>
        public ConsoleSwitch(string tag, string description, bool required = false, bool isBoolean = false)
        {
            _tag = tag.ToLower();
            _description = description;
            _required = required;
            _isBoolean = isBoolean;
        }
        #endregion

        #region Execution
        public override string ToString()
        {
            return "-" + Tag + " - " + Description;
        }
        #endregion
    }
}
