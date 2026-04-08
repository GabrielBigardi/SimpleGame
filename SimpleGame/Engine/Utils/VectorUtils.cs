using System;
using Microsoft.Xna.Framework;

namespace SimpleGame.Engine.Utils;

public static class VectorUtils
{
    public static Vector2 RandomInsideUnitCircle(Random rng)
    {
        var angle = (float)(rng.NextDouble() * Math.PI * 2);
        var radius = (float)Math.Sqrt(rng.NextDouble()); // important!

        var x = radius * (float)Math.Cos(angle);
        var y = radius * (float)Math.Sin(angle);

        return new Vector2(x, y);
    }
}