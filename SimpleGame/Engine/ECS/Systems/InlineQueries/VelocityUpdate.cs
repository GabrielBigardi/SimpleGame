using System.Runtime.CompilerServices;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct VelocityUpdate : IForEach<Position, Velocity>
{
    public float DeltaTime;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Position pos, ref Velocity vel)
    {
        pos.Current += vel.Current * DeltaTime;
    }
}