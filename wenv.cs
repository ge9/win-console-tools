using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class Program
{
    static string SplitCommandsWithEnvChanged(string input0)
    {
        string input=input0;
        while(true){
            Match m = Regex.Match(input, "^[^ ]+=");
            if (m.Success){
                string n=m.Value.TrimEnd('=');
                input=input.Substring(n.Length+1);
                string currentCommand="";
                if (input.StartsWith("\"")){
                    for (int i = 1; i < input.Length; ++i){
                        if (input[i] == '"') {
                            if (i + 1 < input.Length && input[i + 1] == '"') {
                                currentCommand += '"'; // escaped '"'
                                ++i; // Skip the next character
                            }
                            else { // a separator
                                input = input.Substring(i + 1);
                                break;
                            }
                        }
                        else {
                            currentCommand += input[i];
                        }
                    }
                }else{
                    string[] s=input.Split(new char[] {' '}, 2);
                    input = s[1];
                    currentCommand=s[0];
                }
                input=input.TrimStart();
                Environment.SetEnvironmentVariable(n, currentCommand);
            }else break;
        }
        return input;
    }
    static int Main(string[] args)
    {
        return wenv(Helper.SeparateExecPath(Environment.CommandLine).Item2);
    }
    public static int wenv(string commandLine)
    {
        var commands = SplitCommandsWithEnvChanged(commandLine);
        return Helper.StdExecuteCommand(commands);
    }
}