using System.Runtime.CompilerServices;
using Arch.Core;
using Microsoft.Xna.Framework;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.Testing;

namespace SimpleGame.Engine;

public struct TestUpdate : IForEach<InstanceData, Vector2> {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref InstanceData instance, ref Vector2 velocity)
    {
        instance.Position += velocity;
        
        if (instance.Position.X < 0 || instance.Position.X > 1280) velocity.X *= -1;
        if (instance.Position.Y < 0 || instance.Position.Y > 720) velocity.Y *= -1;
    }
}
