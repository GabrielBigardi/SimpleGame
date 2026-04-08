using System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.Rendering;

namespace SimpleGame.Engine.ECS.Systems;

public class InstancedDrawSystem : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly World _world;
    private readonly QueryDescription _query;
    
    private Effect _instancedEffect;
    
    // Geometry Buffers (The Quad)
    private VertexBuffer _geometryBuffer;
    private IndexBuffer _indexBuffer;
    
    // Instance Buffers
    private DynamicVertexBuffer _instanceBuffer;
    private InstanceData[] _instanceDataArray;
    private int _instanceCount;

    // Buffer capacity (Expand as needed, currently 1.5 million)
    private const int MAX_INSTANCES = 1500000; 

    public InstancedDrawSystem(GraphicsDevice graphicsDevice, World world, QueryDescription query, Effect effect)
    {
        _graphicsDevice = graphicsDevice;
        _world = world;
        _query = query;
        _instancedEffect = effect;

        _instanceDataArray = new InstanceData[MAX_INSTANCES];
        
        InitializeGeometry();
        
        // The dynamic buffer that holds our instance data
        _instanceBuffer = new DynamicVertexBuffer(_graphicsDevice, InstanceData.VertexDeclaration, MAX_INSTANCES, BufferUsage.WriteOnly);
    }

    private void InitializeGeometry()
    {
        // Define a 1x1 quad. Origin at top-left to mimic SpriteBatch.
        var vertices = new VertexPositionTexture[4];
        vertices[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)); // Top Left
        vertices[1] = new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 0)); // Top Right
        vertices[2] = new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 1)); // Bottom Left
        vertices[3] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1)); // Bottom Right

        short[] indices = { 0, 1, 2, 2, 1, 3 };

        _geometryBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        _geometryBuffer.SetData(vertices);

        _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), 6, BufferUsage.WriteOnly);
        _indexBuffer.SetData(indices);
    }

    public void UpdateAndDraw(Texture2D texture, Matrix projectionMatrix)
    {
        _instanceCount = 0;

        // 1. Pack the data from Arch into our flat array
        _world.Query(in _query, (ref Position pos, ref SimpleGame.Engine.ECS.Components.Sprite sprite) =>
        {
            if (_instanceCount >= MAX_INSTANCES) return;

            // Note: We use texture.Width/Height to scale the 1x1 quad up to the texture's actual pixel size
            _instanceDataArray[_instanceCount] = new InstanceData(
                pos.Current, 
                new Vector2(sprite.Scale.X * texture.Width, sprite.Scale.Y * texture.Height), 
                Color.White // Or grab from a Color component if you add one!
            );
            _instanceCount++;
        });

        if (_instanceCount == 0) return;

        // 2. Upload the array to the GPU
        _instanceBuffer.SetData(_instanceDataArray, 0, _instanceCount, SetDataOptions.Discard);

        // 3. Set up the Shader parameters
        _instancedEffect.Parameters["Projection"].SetValue(projectionMatrix);
        _instancedEffect.Parameters["SpriteTexture"].SetValue(texture);

        // 4. Bind both buffers: The Geometry (Stream 0) and the Instances (Stream 1)
        _graphicsDevice.SetVertexBuffers(
            new VertexBufferBinding(_geometryBuffer, 0, 0),
            new VertexBufferBinding(_instanceBuffer, 0, 1)
        );
        _graphicsDevice.Indices = _indexBuffer;

        // 5. Draw!
        foreach (var pass in _instancedEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            
            // Draw the 1 quad (2 primitives/triangles) '_instanceCount' times
            _graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, _instanceCount);
        }
    }

    public void Dispose()
    {
        _geometryBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _instanceBuffer?.Dispose();
    }
}