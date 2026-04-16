using System.Runtime.CompilerServices;
using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct SpriteDrawUpdate : IForEach<Position, Sprite, Visible>
{
    private readonly SpriteBatch _spriteBatch;

    public SpriteDrawUpdate(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Position pos, ref Sprite spr, ref Visible visible)
    {
        _spriteBatch.Draw(spr.Texture, pos.Current, spr.Source, spr.Color, 0, spr.Origin, spr.Scale, SpriteEffects.None, 0f);
    }
}