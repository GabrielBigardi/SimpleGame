using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct EntityDestroyUpdate : IForEachWithEntity<Destroy>
{
    public CommandBuffer DestroyBuffer;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(Entity entity, ref Destroy destroy)
    {
        DestroyBuffer.Destroy(in entity);
    }
}