using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class SpriteFlipSystem
{
    private World _world;
    private QueryDescription _query;
    private SpriteFlipUpdate _spriteFlipUpdate;
    
    public SpriteFlipSystem(World world)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Velocity, Sprite>().WithNone<Destroy>();
        _spriteFlipUpdate = new SpriteFlipUpdate();
    }

    public void Update()
    { ;
        _world.InlineParallelQuery<SpriteFlipUpdate, Velocity, Sprite>(in _query, ref _spriteFlipUpdate);
    }
}