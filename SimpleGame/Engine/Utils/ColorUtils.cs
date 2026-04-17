using System;
using Microsoft.Xna.Framework;

namespace SimpleGame.Engine.Utils;

public static class ColorUtils
{
    public static Color RandomColor(Random random) => new(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
    public static Color Inverse(this Color color) => new(
        255 - color.R, 
        255 - color.G, 
        255 - color.B, 
        color.A
    );
}