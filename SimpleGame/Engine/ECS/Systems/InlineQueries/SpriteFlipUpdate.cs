using System.Runtime.CompilerServices;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct SpriteFlipUpdate : IForEach<Velocity, Sprite>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Velocity vel, ref Sprite spr)
    {
        if (vel.Current.X > 0)
            spr.FlipX = 1;
        
        if (vel.Current.X < 0)
            spr.FlipX = 0;
    }
}