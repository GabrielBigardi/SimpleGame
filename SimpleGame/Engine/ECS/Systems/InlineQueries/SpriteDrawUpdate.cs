using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct SpriteDrawUpdate : IForEach<Position, Sprite, Shadow, Visible>
{
    private readonly SpriteBatch _spriteBatch;

    public SpriteDrawUpdate(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Position pos, ref Sprite spr, ref Shadow shadow, ref Visible visible)
    {
        _spriteBatch.Draw(shadow.Texture, pos.Current + shadow.Offset, shadow.Source, shadow.Color, 0,
            spr.Origin, spr.Scale * shadow.Scale, SpriteEffects.None, 0);

        _spriteBatch.Draw(spr.Texture, pos.Current, spr.Source, spr.Color, 0, spr.Origin, spr.Scale, SpriteEffects.None,
            pos.Current.Y * 0.0001f);
    }
}