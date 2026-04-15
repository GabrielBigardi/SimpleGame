using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems;

public partial class VelocitySystem(World world) : BaseSystem<World, float>(world)
{
    [Query(Parallel = true)]
    [All<Position,Velocity>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Move([Data] in float time, ref Position pos, ref Velocity vel)
    {
        pos.Current.X += time * vel.Current.X;
        pos.Current.Y += time * vel.Current.Y;
    }
}