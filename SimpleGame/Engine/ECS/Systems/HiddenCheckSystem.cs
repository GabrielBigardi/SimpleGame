using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class HiddenCheckSystem
{
    private World _world;
    private QueryDescription _query;
    private HiddenCheckUpdate _hiddenCheckUpdate;
    private CommandBuffer _visibilityBuffer;
    
    public HiddenCheckSystem(World world,  CommandBuffer commandBuffer)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Position, Sprite>().WithNone<Visible>();
        _visibilityBuffer  = commandBuffer;
        _hiddenCheckUpdate = new HiddenCheckUpdate();
    }

    public void Update()
    {
        _hiddenCheckUpdate.VisibilityBuffer = _visibilityBuffer;
        _hiddenCheckUpdate.PreferredBackBufferWidth = Game1.CachedPreferredBackBufferWidth;
        _hiddenCheckUpdate.PreferredBackBufferHeight = Game1.CachedPreferredBackBufferHeight;
        _world.InlineParallelEntityQuery<HiddenCheckUpdate, Position, Sprite>(in _query, ref _hiddenCheckUpdate);
    }
}