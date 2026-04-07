using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class SpriteDrawSystem
{
    private World _world;
    private QueryDescription _query;
    private SpriteBatch _spriteBatch;
    private SpriteDrawUpdate _spriteUpdate;
    
    public SpriteDrawSystem(World world, QueryDescription queryDescription, SpriteBatch spriteBatch)
    {
        _world = world;
        _query = queryDescription;
        _spriteBatch = spriteBatch;
        _spriteUpdate = new SpriteDrawUpdate(_spriteBatch);
    }

    public void Update()
    {
        _world.InlineParallelQuery<SpriteDrawUpdate, Position, SimpleGame.Engine.ECS.Components.Sprite>(in _query, ref _spriteUpdate);
    }
}