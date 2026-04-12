using System.Runtime.CompilerServices;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct HiddenCheckUpdate : IForEachWithEntity<Position, Sprite>
{
    public int PreferredBackBufferWidth;
    public int PreferredBackBufferHeight;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(Entity entity, ref Position pos, ref Sprite spr)
    {
        if (pos.Current.X + spr.HalfSize.X >= 100
            && pos.Current.X - spr.HalfSize.X <= PreferredBackBufferWidth - 100
            && pos.Current.Y + spr.HalfSize.Y >= 100
            && pos.Current.Y - spr.HalfSize.Y <= PreferredBackBufferHeight - 100)
        {
            Game1.HiddenBuffer.Add<Visible>(entity);
        }
    }
}