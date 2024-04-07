using System;

public class Program {
    static void Main() {
        printcd();
    }
    public static void printcd() {
        // Get current directory
        string currentDir = Environment.CurrentDirectory;
        // Print
        Console.WriteLine(currentDir);
    }
}