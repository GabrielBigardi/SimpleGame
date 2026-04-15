using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems;

public partial class SpriteDrawSystem(World world) : BaseSystem<World, SpriteBatch>(world)
{
    [Query]
    [All<Position,Sprite,Visible>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Draw([Data] in SpriteBatch spriteBatch, ref Sprite spr, ref Position pos)
    {
        spriteBatch.Draw(spr.Texture, pos.Current, null, spr.Color, 0, spr.Origin, spr.Scale, SpriteEffects.None, 0f);
    }
}