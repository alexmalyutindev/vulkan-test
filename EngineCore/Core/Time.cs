using System.Diagnostics;

namespace MtgWeb.Core;

public static class Time
{
    public static float CurrentTime { get; private set; }
    public static float DeltaTime { get; private set; }
    public static float LastUpdateTime { get; private set; }
    public static long ActualDeltaTime { get; private set; }


    public static void StartFrame(float deltaTime)
    {
        DeltaTime = deltaTime;
        LastUpdateTime = CurrentTime;
        CurrentTime += DeltaTime;
    }

    public static void EndFrame(Stopwatch stopwatch)
    {
        ActualDeltaTime = stopwatch.ElapsedMilliseconds;
    }
}