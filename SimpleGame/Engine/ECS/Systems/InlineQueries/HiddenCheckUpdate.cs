using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.Managers;

namespace SimpleGame.Engine.ECS.Systems.InlineQueries;

public struct HiddenCheckUpdate : IForEachWithEntity<Position, Sprite>
{
    public CommandBuffer VisibilityBuffer;
    public float CameraLeft;
    public float CameraRight;
    public float CameraTop;
    public float CameraBottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(Entity entity, ref Position pos, ref Sprite spr)
    {
        if (pos.Current.X + spr.HalfSize.X >= CameraLeft
            && pos.Current.X - spr.HalfSize.X <= CameraRight
            && pos.Current.Y + spr.HalfSize.Y >= CameraTop
            && pos.Current.Y - spr.HalfSize.Y <= CameraBottom)
        {
            VisibilityBuffer.Add<Visible>(entity);
        }
    }
}