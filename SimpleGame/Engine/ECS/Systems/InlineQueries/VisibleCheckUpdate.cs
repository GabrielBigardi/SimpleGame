using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct VisibleCheckUpdate : IForEachWithEntity<Position, Sprite, Visible>
{
    public CommandBuffer VisibilityBuffer;
    public int PreferredBackBufferWidth;
    public int PreferredBackBufferHeight;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(Entity entity, ref Position pos, ref Sprite spr, ref Visible visible)
    {
        if (pos.Current.X + spr.HalfSize.X < 100
            || pos.Current.X - spr.HalfSize.X > PreferredBackBufferWidth - 100
            || pos.Current.Y + spr.HalfSize.Y < 100
            || pos.Current.Y - spr.HalfSize.Y > PreferredBackBufferHeight - 100)
        {
            VisibilityBuffer.Remove<Visible>(entity);
        }
    }
}