using System.Runtime.CompilerServices;

public static class Debug
{
    public static void Log(string message, [CallerFilePath] string path = "", [CallerLineNumber]int line = 0)
    {
        Console.WriteLine($"[Log] {path}:{line}");
        Console.WriteLine($"{message}");
    }
}