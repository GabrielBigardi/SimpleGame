using System;
using Microsoft.Xna.Framework;

namespace SimpleGame.Engine.Utils;

public static class ColorUtils
{
    public static Color RandomColor(Random random) =>
        new Color(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
}