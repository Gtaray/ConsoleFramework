using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFramework
{
    public class ConsoleLogger
    {
        public static string CmdManagerName;

        /// <summary>
        /// Writes a line to standard out, prefaced by the date and this process' name
        /// </summary>
        /// <param name="value">String to be written to standard out</param>
        public static void PrintLine(string value, bool addDateTime = false)
        {
            if (addDateTime)
                Console.WriteLine(string.Format("[{0}] {1}", DateTime.Now, value));
            else
                Console.WriteLine(value);
        }

        /// <summary>
        /// Writes a line to standard error, prefaced by the date and this process' name
        /// </summary>
        /// <param name="value">String to be written to standard error</param>
        public static void PrintError(string value, bool addDateTime = false)
        {
            if (addDateTime)
                Console.WriteLine(string.Format("[{0}] {1}", DateTime.Now, value));
            else
                Console.WriteLine(value);
        }

        /// <summary>
        /// Writes a line to standard out only if the process was built as a debug build
        /// </summary>
        /// <param name="value">String to be written to standard out on debug builds</param>
        public static void PrintDebug(string value)
        {
#if DEBUG
            Console.WriteLine(value);
#endif
        }
    }
}
