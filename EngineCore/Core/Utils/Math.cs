using System.Numerics;

namespace MtgWeb.Core;

internal static class Math
{
    public const float DEG_TO_RAD = 0.017453292519943295769236907684886f;

    // Conversion between quaternions and Euler angles
    // From: https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
    // x : Pitch
    // y : Yaw
    // z : Roll
    public static Vector3 ToEuler(this Quaternion q)
    {
        Vector3 angles;
        float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.X = MathF.Atan2(sinr_cosp, cosr_cosp);

        // pitch (Y-axis rotation)
        float sinp = 2 * (q.W * q.Y - q.Z * q.X);
        if (MathF.Abs(sinp) >= 1)
            angles.Z = MathF.CopySign(MathF.PI / 2, sinp); // use 90 degrees if out of range
        else
            angles.Z = MathF.Asin(sinp);

        // yaw (Z-axis rotation)
        float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Y = MathF.Atan2(siny_cosp, cosy_cosp);

        return angles;
    }
}