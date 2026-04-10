using System.Runtime.CompilerServices;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct SpriteDrawUpdate : IForEach<Position, SimpleGame.Engine.ECS.Components.Sprite>
{
    private readonly SpriteBatch _spriteBatch;

    public SpriteDrawUpdate(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Position pos, ref SimpleGame.Engine.ECS.Components.Sprite spr)
    {
        if (spr.Texture == null)
            return;

        if (pos.Current.X + spr.HalfSize.X < 100
            || pos.Current.X - spr.HalfSize.X > Game1.CachedPreferredBackBufferWidth - 100
            || pos.Current.Y + spr.HalfSize.Y < 100
            || pos.Current.Y - spr.HalfSize.Y > Game1.CachedPreferredBackBufferHeight - 100)
            return;

        _spriteBatch.Draw(spr.Texture, pos.Current, null, spr.Color, 0, spr.Origin, spr.Scale, SpriteEffects.None, 0f);
    }
}