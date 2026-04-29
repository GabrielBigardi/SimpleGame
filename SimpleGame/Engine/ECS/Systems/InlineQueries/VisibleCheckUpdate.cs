using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.Managers;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct VisibleCheckUpdate : IForEachWithEntity<Position, Sprite, Visible>
{
    public CommandBuffer VisibilityBuffer;
    public float CameraLeft;
    public float CameraRight;
    public float CameraTop;
    public float CameraBottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(Entity entity, ref Position pos, ref Sprite spr, ref Visible visible)
    {
        if (pos.Current.X + spr.HalfSize.X < CameraLeft
            || pos.Current.X - spr.HalfSize.X > CameraRight
            || pos.Current.Y + spr.HalfSize.Y < CameraTop
            || pos.Current.Y - spr.HalfSize.Y > CameraBottom)
        {
            VisibilityBuffer.Remove<Visible>(entity);
        }
    }
}