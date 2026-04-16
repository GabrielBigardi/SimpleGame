using System;

namespace SimpleGame.Engine.Utils;

public static class RandomUtils
{
    public static float NextFloat(this Random random, float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }
}