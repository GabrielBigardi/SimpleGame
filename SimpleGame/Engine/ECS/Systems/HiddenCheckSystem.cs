using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems;

public partial class HiddenCheckSystem(World world) : BaseSystem<World, int>(world)
{
    [Query(Parallel = true)]
    [All<Position,Sprite>, None<Visible>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Check(in Entity entity, ref Position pos, ref Sprite spr)
    {
        if (pos.Current.X + spr.HalfSize.X >= 100
            && pos.Current.X - spr.HalfSize.X <= Game1.CachedPreferredBackBufferWidth - 100
            && pos.Current.Y + spr.HalfSize.Y >= 100
            && pos.Current.Y - spr.HalfSize.Y <= Game1.CachedPreferredBackBufferHeight - 100)
        {
            Game1.VisibilityBuffer.Add<Visible>(entity);
        }
    }
}