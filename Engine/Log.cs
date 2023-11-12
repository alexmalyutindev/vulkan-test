using System.Runtime.CompilerServices;
using System.Text;

namespace Engine;

public static class Log
{
    public static void Info(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        LogInternal(LogType.Info, message, path, line);
    }

    public static void Warning(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        LogInternal(LogType.Warning, message, path, line);

    }

    public static void Error(string message, [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        LogInternal(LogType.Error, message, path, line);
    }

    private static void LogInternal(LogType type, string message, string path, int line)
    {
        var tag = type switch
        {
            LogType.Trace => "[" + nameof(LogType.Trace) + "] ",
            LogType.Info => "[" + nameof(LogType.Info) + "] ",
            LogType.Warning => "[" + nameof(LogType.Warning) + "] ",
            LogType.Error => "[" + nameof(LogType.Error) + "] ",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(tag)
            .AppendLine(message)
            .Append(path)
            .Append(':')
            .Append(line)
            .AppendLine();
        
        Console.WriteLine(stringBuilder.ToString());
    }

    private enum LogType
    {
        Trace,
        Info,
        Warning,
        Error,
    }
}