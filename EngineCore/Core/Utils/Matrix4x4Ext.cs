using System.Numerics;
using System.Runtime.InteropServices;

namespace MtgWeb.Core.Utils;

public static class Matrix4x4Ext
{
    public static void ToArray(this Matrix4x4 matrix, in float[] target)
    {
        var bytes = MemoryMarshal.Cast<float, byte>(target.AsSpan());
        MemoryMarshal.TryWrite(bytes, ref matrix);
    }
}