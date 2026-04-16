using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class EntityDestroySystem
{
    private World _world;
    private QueryDescription _query;
    private EntityDestroyUpdate _entityDestroyUpdate;
    private CommandBuffer _destroyBuffer;

    public EntityDestroySystem(World world, CommandBuffer commandBuffer)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Destroy>();
        _destroyBuffer = commandBuffer;
        _entityDestroyUpdate = new EntityDestroyUpdate();
    }

    public void Update()
    {
        _entityDestroyUpdate.DestroyBuffer = _destroyBuffer;
        _world.InlineParallelEntityQuery<EntityDestroyUpdate, Destroy>(in _query, ref _entityDestroyUpdate);
    }
}