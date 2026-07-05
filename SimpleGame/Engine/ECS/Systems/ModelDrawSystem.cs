using System.Linq;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.Managers;

namespace SimpleGame.Engine.ECS.Systems;

public class ModelDrawSystem
{
    private World _world;
    private QueryDescription _query;
    
    public ModelDrawSystem(World world)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Position, Rotation, ModelComponent>().WithNone<Destroy>();
    }

    public void Draw()
    {
        _world.Query(in _query, (ref Position pos, ref Rotation rot, ref ModelComponent modelComp) =>
        {
            if (modelComp.Model == null)
                return;

            var worldMatrix = Matrix.CreateScale(modelComp.Scale) * 
                                 Matrix.CreateRotationX(rot.Current.X) * 
                                 Matrix.CreateRotationY(rot.Current.Y) * 
                                 Matrix.CreateRotationZ(rot.Current.Z) * 
                                 Matrix.CreateTranslation(pos.Current);

            foreach (var mesh in modelComp.Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                }
            }

            modelComp.Model.Draw(worldMatrix, CameraManager.ViewMatrix, CameraManager.ProjectionMatrix);
        });
    }
}
