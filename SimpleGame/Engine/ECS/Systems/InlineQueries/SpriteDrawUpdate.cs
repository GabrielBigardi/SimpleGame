using System.Runtime.CompilerServices;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct SpriteDrawUpdate : IForEach<Position, SimpleGame.Engine.ECS.Components.Sprite>
{
    private SpriteBatch _spriteBatch;

    public SpriteDrawUpdate(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Position pos, ref SimpleGame.Engine.ECS.Components.Sprite spr)
    {
        if (spr.Texture == null)
            return;

        _spriteBatch.Draw(spr.Texture, pos.Current, null, Color.White, 0, new(spr.Texture.Width / 2f, spr.Texture.Height / 2f), spr.Scale, SpriteEffects.None, 0f);
    }
}