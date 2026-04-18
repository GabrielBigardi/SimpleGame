using System.Runtime.CompilerServices;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.Managers;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct LightDrawUpdate : IForEach<Position, LightSource, Visible>
{
    private readonly SpriteBatch _spriteBatch;

    public LightDrawUpdate(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Position pos, ref LightSource lightSource, ref Visible visible)
    {
        if (false)
        {
            var scale = new Vector2(AssetManager.LightGradientTexture.Width, AssetManager.LightGradientTexture.Height) * lightSource.Scale;
            _spriteBatch.DrawRectangle(pos.Current - scale * 0.5f, scale, lightSource.Color, 4f);
        }
        
        _spriteBatch.Draw(AssetManager.LightGradientTexture, pos.Current, null, lightSource.Color, 0f, lightSource.Origin, lightSource.Scale, SpriteEffects.None, 0f);
    }
}