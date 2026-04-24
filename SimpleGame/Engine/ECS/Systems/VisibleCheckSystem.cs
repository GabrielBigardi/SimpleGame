using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;
using SimpleGame.Engine.Managers;

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
        var worldWidth = Game1._graphics.PreferredBackBufferWidth / CameraManager.Zoom;
        var worldHeight = Game1._graphics.PreferredBackBufferHeight / CameraManager.Zoom;

        _visibleCheckUpdate.CameraLeft = CameraManager.CameraPosition.X - (worldWidth / 2f);
        _visibleCheckUpdate.CameraRight = CameraManager.CameraPosition.X + (worldWidth / 2f);
        _visibleCheckUpdate.CameraTop = CameraManager.CameraPosition.Y - (worldHeight / 2f);
        _visibleCheckUpdate.CameraBottom = CameraManager.CameraPosition.Y + (worldHeight / 2f);
        
        _visibleCheckUpdate.VisibilityBuffer = _visibilityBuffer;
        _world.InlineParallelEntityQuery<VisibleCheckUpdate, Position, Sprite, Visible>(in _query, ref _visibleCheckUpdate);
    }
}