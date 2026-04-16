using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class VisibleCheckSystem
{
    private World _world;
    private QueryDescription _query;
    private VisibleCheckUpdate _visibleCheckUpdate;
    private CommandBuffer _visibilityBuffer;
    
    public VisibleCheckSystem(World world, CommandBuffer commandBuffer)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Position, Sprite, Visible>().WithNone<Destroy>();
        _visibilityBuffer  = commandBuffer;
        _visibleCheckUpdate = new VisibleCheckUpdate();
    }

    public void Update()
    {
        _visibleCheckUpdate.VisibilityBuffer = _visibilityBuffer;
        _visibleCheckUpdate.PreferredBackBufferWidth = Game1.CachedPreferredBackBufferWidth;
        _visibleCheckUpdate.PreferredBackBufferHeight = Game1.CachedPreferredBackBufferHeight;
        _world.InlineParallelEntityQuery<VisibleCheckUpdate, Position, Sprite, Visible>(in _query, ref _visibleCheckUpdate);
    }
}