using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace scanner
{
    public class RegexUtil
    {
        public static string Replace(string input, Dictionary<string, string> infos)
        {
            string output = input;
            foreach (var info in infos)
            {
                if (output.Contains($"$${{{info.Key}}}"))
                {
                    output = output.Replace($"$${{{info.Key}}}", $"{infos[$"T-{info.Key}"]}");
                }
                output = output.Replace($"${{{info.Key}}}", info.Value);
            }
            return output;
        }
    }
}
