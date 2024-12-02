using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

public class Runotherex
{
    public static string getNewCmd()
    {
        string commandLine = Helper.SeparateExecPath(Environment.CommandLine).Item2;
        
        string myName = System.Reflection.Assembly.GetExecutingAssembly().Location;
        myName = myName.Substring(0, myName.Length - 3) + 't' + myName[myName.Length - 2] + 't';

        string[] split_cmd = File.ReadAllText(myName, Encoding.UTF8).Split(new[] { "\r\n", "\r", "\n" }, 2, StringSplitOptions.None);
        return Regex.Replace(split_cmd[1], Regex.Escape(split_cmd[0])+"(.)", new MatchEvaluator(match => {
            switch (match.Groups[1].Value)
            {
                case "a": return commandLine.Replace("&", "&&");
                case "b": return commandLine.Replace("|", "||");
                case "c": return commandLine;
                case "6": return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(commandLine));
                case "p": return commandLine.Replace("'", "''").Replace("‘", "‘‘").Replace("’", "’’").Replace("‚", "‚‚").Replace("‛", "‛‛");
                case "d": return System.IO.Path.GetDirectoryName(myName);
                default: throw new System.ComponentModel.Win32Exception("illegal character after prefix:"+match.Groups[1].Value);
            }
        }));
    }
}