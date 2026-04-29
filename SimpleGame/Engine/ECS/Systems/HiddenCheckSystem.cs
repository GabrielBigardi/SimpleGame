using Arch.Buffer;
using Arch.Core;
using Microsoft.Xna.Framework;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;
using SimpleGame.Engine.Managers;

namespace SimpleGame.Engine.ECS.Systems;

public class HiddenCheckSystem
{
    private World _world;
    private QueryDescription _query;
    private HiddenCheckUpdate _hiddenCheckUpdate;
    private CommandBuffer _visibilityBuffer;
    private GraphicsDeviceManager _graphicsDeviceManager;
    
    public HiddenCheckSystem(World world, CommandBuffer commandBuffer, GraphicsDeviceManager graphicsDeviceManager)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Position, Sprite>().WithNone<Visible, Destroy>();
        _visibilityBuffer  = commandBuffer;
        _graphicsDeviceManager = graphicsDeviceManager;
        _hiddenCheckUpdate = new HiddenCheckUpdate();
    }

    public void Update()
    {
        var worldWidth = _graphicsDeviceManager.PreferredBackBufferWidth / CameraManager.Zoom;
        var worldHeight = _graphicsDeviceManager.PreferredBackBufferHeight / CameraManager.Zoom;

        _hiddenCheckUpdate.CameraLeft = CameraManager.CameraPosition.X - (worldWidth / 2f);
        _hiddenCheckUpdate.CameraRight = CameraManager.CameraPosition.X + (worldWidth / 2f);
        _hiddenCheckUpdate.CameraTop = CameraManager.CameraPosition.Y - (worldHeight / 2f);
        _hiddenCheckUpdate.CameraBottom = CameraManager.CameraPosition.Y + (worldHeight / 2f);
        
        _hiddenCheckUpdate.VisibilityBuffer = _visibilityBuffer;
        _world.InlineParallelEntityQuery<HiddenCheckUpdate, Position, Sprite>(in _query, ref _hiddenCheckUpdate);
    }
}