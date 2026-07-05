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
    private GraphicsDevice _graphicsDevice;
    private Texture2D _defaultTexture;
    
    public ModelDrawSystem(World world, GraphicsDevice graphicsDevice)
    {
        _world = world;
        _graphicsDevice = graphicsDevice;
        _query = new QueryDescription().WithAll<Position, Rotation, ModelComponent>().WithNone<Destroy>();
        
        _defaultTexture = new Texture2D(graphicsDevice, 1, 1);
        _defaultTexture.SetData(new[] { Color.White });
    }

    public void DrawShadowMap()
    {
        if (AssetManager.ShadowShader == null) return;
        
        var effect = AssetManager.ShadowShader;
        effect.CurrentTechnique = effect.Techniques["CreateShadowMap"];

        _world.Query(in _query, (ref Position pos, ref Rotation rot, ref ModelComponent modelComp) =>
        {
            if (modelComp.Model == null) return;

            var transforms = new Matrix[modelComp.Model.Bones.Count];
            modelComp.Model.CopyAbsoluteBoneTransformsTo(transforms);

            var worldMatrix = Matrix.CreateScale(modelComp.Scale) * 
                              Matrix.CreateRotationX(rot.Current.X) * 
                              Matrix.CreateRotationY(rot.Current.Y) * 
                              Matrix.CreateRotationZ(rot.Current.Z) * 
                              Matrix.CreateTranslation(pos.Current);

            foreach (var mesh in modelComp.Model.Meshes)
            {
                effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * worldMatrix);

                foreach (var part in mesh.MeshParts)
                {
                    _graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                    _graphicsDevice.Indices = part.IndexBuffer;

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        });
    }

    public void DrawModels()
    {
        if (AssetManager.ShadowShader == null) return;
        
        var effect = AssetManager.ShadowShader;
        effect.CurrentTechnique = effect.Techniques["DrawWithShadowMap"];

        _world.Query(in _query, (ref Position pos, ref Rotation rot, ref ModelComponent modelComp) =>
        {
            if (modelComp.Model == null) return;

            var transforms = new Matrix[modelComp.Model.Bones.Count];
            modelComp.Model.CopyAbsoluteBoneTransformsTo(transforms);

            var worldMatrix = Matrix.CreateScale(modelComp.Scale) * 
                              Matrix.CreateRotationX(rot.Current.X) * 
                              Matrix.CreateRotationY(rot.Current.Y) * 
                              Matrix.CreateRotationZ(rot.Current.Z) * 
                              Matrix.CreateTranslation(pos.Current);

            effect.Parameters["View"].SetValue(CameraManager.ViewMatrix);
            effect.Parameters["Projection"].SetValue(CameraManager.ProjectionMatrix);
            effect.Parameters["AmbientColor"]?.SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1f));
            effect.Parameters["DiffuseColor"]?.SetValue(new Vector4(1f, 1f, 1f, 1f));

            foreach (var mesh in modelComp.Model.Meshes)
            {
                effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * worldMatrix);

                foreach (var part in mesh.MeshParts)
                {
                    var texToUse = _defaultTexture;
                    if (part.Effect is BasicEffect basicEffect && basicEffect.Texture != null)
                    {
                        texToUse = basicEffect.Texture;
                    }
                    effect.Parameters["DiffuseTexture"]?.SetValue(texToUse);

                    _graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                    _graphicsDevice.Indices = part.IndexBuffer;

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        });
    }
}
