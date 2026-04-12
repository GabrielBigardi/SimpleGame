using Arch.Core;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class HiddenCheckSystem
{
    private World _world;
    private QueryDescription _query;
    private HiddenCheckUpdate _hiddenCheckUpdate;
    
    public HiddenCheckSystem(World world, QueryDescription queryDescription)
    {
        _world = world;
        _query = queryDescription;
        _hiddenCheckUpdate = new HiddenCheckUpdate();
    }

    public void Update()
    {
        _hiddenCheckUpdate.PreferredBackBufferWidth = Game1.CachedPreferredBackBufferWidth;
        _hiddenCheckUpdate.PreferredBackBufferHeight = Game1.CachedPreferredBackBufferHeight;
        _world.InlineParallelEntityQuery<HiddenCheckUpdate, Position, Sprite>(in _query, ref _hiddenCheckUpdate);
    }
}