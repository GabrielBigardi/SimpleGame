using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleGame.Engine.Managers;
using SimpleGame.Engine.Testing;
using Arch.Core;
using Schedulers;
using SimpleGame.Engine; // <-- Added Arch namespace

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

    // Instance rendering buffer (Used just to ship data to the GPU)
    private const int SpriteCount = 500_000;
    private InstanceData[] _instances;
    
    // --- Arch ECS Data ---
    private World _world;
    private JobScheduler _jobScheduler;
    private QueryDescription _movementQuery;
    private QueryDescription _renderQuery;

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

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight, 0, 0, 1);
        Matrix view = Matrix.Identity;
        _viewProjection = view * projection;

        // Initialize the Arch World
        _world = World.Create();
        _jobScheduler = new(
            new JobScheduler.Config
            {
                ThreadPrefixName = "Arch.Samples",
                ThreadCount = 0,                         
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            }
        );
        World.SharedJobScheduler = _jobScheduler;
        
        // Pre-define our high-performance queries
        _movementQuery = new QueryDescription().WithAll<InstanceData, Vector2>();
        _renderQuery = new QueryDescription().WithAll<InstanceData>();

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
        _instanceBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(InstanceData), SpriteCount, BufferUsage.WriteOnly);

        _bindings = new VertexBufferBinding[2];
        _bindings[0] = new VertexBufferBinding(_quadVertexBuffer);
        _bindings[1] = new VertexBufferBinding(_instanceBuffer, 0, 1);

        // 3. Populate Entities via Arch ECS
        _instances = new InstanceData[SpriteCount];
        
        int spriteWidth = 16;
        int spriteHeight = 16;
        int atlasStartX = 64;
        int atlasStartY = 448;
        int gridColumns = 10;
        int gridRows = 7;

        for (int i = 0; i < SpriteCount; i++)
        {
            InstanceData instance = new InstanceData();
            
            instance.Position = new Vector2(
                _random.Next(0, _graphics.PreferredBackBufferWidth),
                _random.Next(0, _graphics.PreferredBackBufferHeight)
            );

            int randomColumn = _random.Next(0, gridColumns);
            int randomRow = _random.Next(0, gridRows);
            int pixelX = atlasStartX + (randomColumn * spriteWidth);
            int pixelY = atlasStartY + (randomRow * spriteHeight);

            Rectangle sourceRect = new Rectangle(pixelX, pixelY, spriteWidth, spriteHeight);

            instance.UV = GetUVsFromRectangle(sourceRect, atlasSize);
            instance.Scale = new Vector2(spriteWidth, spriteHeight);
            instance.Color = Color.White;
            
            Vector2 velocity = new Vector2(
                (float)_random.NextDouble() * 2 - 1, 
                (float)_random.NextDouble() * 2 - 1
            );

            if (velocity != Vector2.Zero)
                velocity.Normalize();

            float speedInPixelsPerSecond = _random.Next(50, 100);
            velocity *= speedInPixelsPerSecond;

            // --- ARCH: Create the Entity and attach both components ---
            _world.Create(instance, velocity);
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

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;

        // --- ARCH: Update System ---
        // Arch handles the iteration sequentially but extremely fast due to CPU cache locality
        var bla = new TestUpdate();
        _world.InlineParallelQuery<TestUpdate, InstanceData, Vector2>(in _movementQuery, ref bla);
        
        //_world.ParallelQuery(in _movementQuery, (ref InstanceData instance, ref Vector2 velocity) =>
        //{
        //    instance.Position += velocity * dt;
//
        //    // Simple screen bounce logic
        //    if (instance.Position.X < 0 || instance.Position.X > screenWidth) velocity.X *= -1;
        //    if (instance.Position.Y < 0 || instance.Position.Y > screenHeight) velocity.Y *= -1;
        //});

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // --- ARCH: Extract System ---
        // We need to copy the updated InstanceData from Arch into our MonoGame array
        int index = 0;
        _world.Query(in _renderQuery, (ref InstanceData instance) =>
        {
            _instances[index++] = instance;
        });

        // Push the extracted array to the GPU
        _instanceBuffer.SetData(_instances, 0, SpriteCount, SetDataOptions.Discard);

        // 1. Tell the GPU to use our custom buffers
        GraphicsDevice.SetVertexBuffers(_bindings);
        GraphicsDevice.Indices = _quadIndexBuffer;

        // 2. Set Shader Parameters
        AssetManager.InstancingEffect.Parameters["ViewProjection"].SetValue(_viewProjection);
        AssetManager.InstancingEffect.Parameters["SpriteTexture"].SetValue(AssetManager.RoguelikeAtlas);

        // 3. Disable depth tracking and enable alpha blending
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
                primitiveCount: 2,
                instanceCount: SpriteCount
            );
        }

        base.Draw(gameTime);

        Window.Title = $"Arch ECS | Sprites: {SpriteCount:N0} | FPS: {1f / gameTime.ElapsedGameTime.TotalSeconds:00.0}";
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
        // Don't forget to dispose the Arch World!
        _world.Dispose();
        _jobScheduler.Dispose();
        base.Dispose(disposing);
    }
}