using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleGame.Engine.Managers;
using SimpleGame.Engine.Testing;

namespace SimpleGame;

public class Game1 : Game
{
    #region Monogame

    private SpriteBatch _spriteBatch;
    private GraphicsDeviceManager _graphics;

    #endregion

    private Vector2 ScreenCenter =>
        new(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f);

    private readonly Random _random = new();


    private VertexBuffer _quadVertexBuffer;
    private IndexBuffer _quadIndexBuffer;
    private DynamicVertexBuffer _instanceBuffer;
    private VertexBufferBinding[] _bindings;

    // Sprite Data
    private const int SpriteCount = 500_000; // Half a million for this test
    private InstanceData[] _instances;
    private Vector2[] _velocities; // Kept separate from the struct to save GPU bandwidth

    // Camera
    private Matrix _viewProjection;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;

        // Unlimited FPS
        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;

        // Limited FPS
        // IsFixedTimeStep = true;
        // TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // Setup a simple 2D Orthographic Camera matching the screen size
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight, 0, 0, 1);
        Matrix view = Matrix.Identity;
        _viewProjection = view * projection;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        AssetManager.Load(Content);
#if DEBUG
        AssetManager.InitializeHotReload();
#endif

        Vector2 atlasSize = new Vector2(AssetManager.RoguelikeAtlas.Width, AssetManager.RoguelikeAtlas.Height);

        // 1. Setup Base Quad Geometry
        VertexPositionTexture[] quadVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(0.5f, -0.5f, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(0.5f, 0.5f, 0), new Vector2(1, 0))
        };

        short[] quadIndices = new short[] { 0, 1, 2, 1, 3, 2 };

        _quadVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        _quadVertexBuffer.SetData(quadVertices);

        _quadIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6, BufferUsage.WriteOnly);
        _quadIndexBuffer.SetData(quadIndices);

        // 2. Setup Instance Buffer & Bindings
        _instanceBuffer =
            new DynamicVertexBuffer(GraphicsDevice, typeof(InstanceData), SpriteCount, BufferUsage.WriteOnly);

        _bindings = new VertexBufferBinding[2];
        _bindings[0] = new VertexBufferBinding(_quadVertexBuffer);
        _bindings[1] = new VertexBufferBinding(_instanceBuffer, 0, 1); // Advance once per instance

        // 3. Populate Initial Sprite Data
        _instances = new InstanceData[SpriteCount];
        _velocities = new Vector2[SpriteCount];
        Random rng = new Random();

        // Define your sub-grid properties
        int spriteWidth = 16;
        int spriteHeight = 16;
        int atlasStartX = 64;
        int atlasStartY = 448;
        int gridColumns = 10;
        int gridRows = 7;

        for (int i = 0; i < SpriteCount; i++)
        {
            // Random position on screen
            _instances[i].Position = new Vector2(
                rng.Next(0, _graphics.PreferredBackBufferWidth),
                rng.Next(0, _graphics.PreferredBackBufferHeight)
            );

            // 1. Pick a random column (0 to 9) and row (0 to 6)
            int randomColumn = rng.Next(0, gridColumns);
            int randomRow = rng.Next(0, gridRows);

            // 2. Calculate the exact pixel coordinates on the atlas
            int pixelX = atlasStartX + (randomColumn * spriteWidth);
            int pixelY = atlasStartY + (randomRow * spriteHeight);

            // 3. Create the source rectangle and convert to UVs
            Rectangle sourceRect = new Rectangle(pixelX, pixelY, spriteWidth, spriteHeight);

            _instances[i].UV = GetUVsFromRectangle(sourceRect, atlasSize);
            _instances[i].Scale = new Vector2(spriteWidth, spriteHeight);
            _instances[i].Color = Color.White;
            
            // 1. Get a random direction between -1.0 and 1.0
            Vector2 randomDirection = new Vector2(
                (float)rng.NextDouble() * 2 - 1, 
                (float)rng.NextDouble() * 2 - 1
            );

            // 2. Normalize it so diagonal movement isn't faster than cardinal movement
            if (randomDirection != Vector2.Zero)
                randomDirection.Normalize();

            // 3. Define a speed in Pixels Per Second (e.g., 150 to 300)
            float speedInPixelsPerSecond = rng.Next(50, 100);

            // Give it a random velocity for the update loop
            _velocities[i] = randomDirection * speedInPixelsPerSecond;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

#if DEBUG
        if (AssetManager.NeedsReload)
        {
            AssetManager.PerformReload();
        }
#endif

        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;

        // Use Parallel.For to multithread the CPU update loop for massive numbers
        Parallel.For(0, SpriteCount, i =>
        {
            _instances[i].Position += _velocities[i] * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Simple screen bounce logic
            if (_instances[i].Position.X < 0 || _instances[i].Position.X > screenWidth) _velocities[i].X *= -1;
            if (_instances[i].Position.Y < 0 || _instances[i].Position.Y > screenHeight) _velocities[i].Y *= -1;
        });

        // Push the updated positions to the GPU
        // SetDataOptions.Discard is crucial here to prevent the CPU from stalling while waiting for the GPU
        _instanceBuffer.SetData(_instances, 0, SpriteCount, SetDataOptions.Discard);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // 1. Tell the GPU to use our custom buffers
        GraphicsDevice.SetVertexBuffers(_bindings);
        GraphicsDevice.Indices = _quadIndexBuffer;

        // 2. Set Shader Parameters
        AssetManager.InstancingEffect.Parameters["ViewProjection"].SetValue(_viewProjection);
        AssetManager.InstancingEffect.Parameters["SpriteTexture"].SetValue(AssetManager.RoguelikeAtlas);

        // 3. Disable depth tracking and enable alpha blending (if your sprites have transparency)
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // 4. Draw Half a Million Sprites in one call
        foreach (EffectPass pass in AssetManager.InstancingEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            GraphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.TriangleList,
                baseVertex: 0,
                startIndex: 0,
                primitiveCount: 2, // 2 triangles make our quad
                instanceCount: SpriteCount
            );
        }

        base.Draw(gameTime);

        //// Print FPS to the window title
        //Window.Title = $"Sprites: {SpriteCount:N0} | FPS: {1f / gameTime.ElapsedGameTime.TotalSeconds:00.0}";
    }

    // Helper Method
    private Vector4 GetUVsFromRectangle(Rectangle sourceRect, Vector2 atlasSize)
    {
        return new Vector4(
            sourceRect.X / atlasSize.X,
            sourceRect.Y / atlasSize.Y,
            sourceRect.Width / atlasSize.X,
            sourceRect.Height / atlasSize.Y
        );
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}