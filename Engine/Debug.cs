using System.Runtime.CompilerServices;
using System.Text;

namespace Engine;

public static class Debug
{
    public static void Log(string message, [CallerFilePath] string path = "", [CallerLineNumber]int line = 0)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("[Log] ");
        stringBuilder.Append(path);
        stringBuilder.Append(':');
        stringBuilder.Append(line);
        stringBuilder.AppendLine();
        stringBuilder.Append(message);
    }
    
    public static void Warning(string message, [CallerFilePath] string path = "", [CallerLineNumber]int line = 0)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("[Warning] ");
        stringBuilder.Append(path);
        stringBuilder.Append(':');
        stringBuilder.Append(line);
        stringBuilder.AppendLine();
        stringBuilder.Append(message);
    }
}