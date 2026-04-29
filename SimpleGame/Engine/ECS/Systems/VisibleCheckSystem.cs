using Arch.Buffer;
using Arch.Core;
using Microsoft.Xna.Framework;
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
    private GraphicsDeviceManager _graphicsDeviceManager;
    
    public VisibleCheckSystem(World world, CommandBuffer commandBuffer, GraphicsDeviceManager graphicsDeviceManager)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Position, Sprite, Visible>().WithNone<Destroy>();
        _visibilityBuffer  = commandBuffer;
        _graphicsDeviceManager = graphicsDeviceManager;
        _visibleCheckUpdate = new VisibleCheckUpdate();
    }

    public void Update()
    {
        var worldWidth = _graphicsDeviceManager.PreferredBackBufferWidth / CameraManager.Zoom;
        var worldHeight = _graphicsDeviceManager.PreferredBackBufferHeight / CameraManager.Zoom;

        _visibleCheckUpdate.CameraLeft = CameraManager.CameraPosition.X - (worldWidth / 2f);
        _visibleCheckUpdate.CameraRight = CameraManager.CameraPosition.X + (worldWidth / 2f);
        _visibleCheckUpdate.CameraTop = CameraManager.CameraPosition.Y - (worldHeight / 2f);
        _visibleCheckUpdate.CameraBottom = CameraManager.CameraPosition.Y + (worldHeight / 2f);
        
        _visibleCheckUpdate.VisibilityBuffer = _visibilityBuffer;
        _world.InlineParallelEntityQuery<VisibleCheckUpdate, Position, Sprite, Visible>(in _query, ref _visibleCheckUpdate);
    }
}