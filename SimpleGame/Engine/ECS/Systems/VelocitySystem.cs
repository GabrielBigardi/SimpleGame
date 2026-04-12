using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class VelocitySystem
{
    private World _world;
    private QueryDescription _query;
    private VelocityUpdate _velocityUpdate;
    
    public VelocitySystem(World world, QueryDescription queryDescription)
    {
        _world = world;
        _query = queryDescription;
        _velocityUpdate = new VelocityUpdate();
    }

    public void Update()
    {
        _velocityUpdate.DeltaTime = TimeManager.DeltaTime;
        _world.InlineParallelQuery<VelocityUpdate, Position, Velocity>(in _query, ref _velocityUpdate);
    }
}