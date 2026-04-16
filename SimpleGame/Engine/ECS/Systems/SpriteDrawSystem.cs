using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class SpriteDrawSystem
{
    private World _world;
    private QueryDescription _query;
    private SpriteDrawUpdate _spriteUpdate;
    
    public SpriteDrawSystem(World world, SpriteBatch spriteBatch)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Position, Sprite, Shadow, Visible>().WithNone<Destroy>();
        _spriteUpdate = new SpriteDrawUpdate(spriteBatch);
    }

    public void Update()
    {
        _world.InlineQuery<SpriteDrawUpdate, Position, Sprite, Shadow, Visible>(in _query, ref _spriteUpdate);
    }
}