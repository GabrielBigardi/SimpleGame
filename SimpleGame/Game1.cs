using System;
using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schedulers;
using SimpleGame.Engine;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems;
using SimpleGame.Engine.Utils;

namespace SimpleGame;

public class Game1 : Game
{
    // === ECS ===
    private World _world;
    private JobScheduler _jobScheduler;

    public static CommandBuffer VisibilityBuffer = new();
    // ======

    // === MonoGame ===
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public static int CachedPreferredBackBufferWidth;
    public static int CachedPreferredBackBufferHeight;
    // ======
    
    // === Game ===
    private bool _spawned = false;
    private Matrix _projectionMatrix;

    private Group<float> _deltatimeSystems;
    private Group<SpriteBatch> _drawSystems;

    private Group<int> _visibilitySystems;
    // ======
    

    private Vector2 ScreenCenter => new(CachedPreferredBackBufferWidth / 2f, CachedPreferredBackBufferHeight / 2f);

    private Random _random = new();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;

        CachedPreferredBackBufferWidth = 1280;
        CachedPreferredBackBufferHeight = 720;

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
        _world = World.Create();
        
        _deltatimeSystems = new Group<float>("Movement Related", new VelocitySystem(_world));
        _deltatimeSystems.Initialize();

        _drawSystems = new Group<SpriteBatch>("Drawing Systems", new SpriteDrawSystem(_world));
        _drawSystems.Initialize();

        _visibilitySystems = new Group<int>("Visibility Systems",
            new HiddenCheckSystem(_world),
            new VisibleCheckSystem(_world));
        _visibilitySystems.Initialize();

        _jobScheduler = new JobScheduler(
            new JobScheduler.Config
            {
                ThreadPrefixName = "SimpleGame",
            }
        );
        
        Console.WriteLine(_jobScheduler.ThreadCount);

        World.SharedJobScheduler = _jobScheduler;

        // If this is a AOT build add components to arrayregistry
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            ArrayRegistry.Add<Position>();
            ArrayRegistry.Add<Sprite>();
            ArrayRegistry.Add<Velocity>();
            ArrayRegistry.Add<Visible>();
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        AssetManager.Load(Content);
#if DEBUG
        AssetManager.InitializeHotReload();
#endif

        _projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, 0, 0, 1);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _visibilitySystems.Update(in CachedPreferredBackBufferWidth);

        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
        {
            for (var i = 0; i < 100; i++)
            {
                var randomX = _random.Next(100, CachedPreferredBackBufferWidth - 100);
                var randomY = _random.Next(100, CachedPreferredBackBufferHeight - 100);
                var pos = new Vector2(randomX, randomY);

                var randomVelocity = VectorUtils.RandomInsideUnitCircle(_random);
                randomVelocity.Normalize();

                var scale = 0.25f;
                var randomColor = new Color(_random.Next(0, 256), _random.Next(0, 256), _random.Next(0, 256));
                var origin = new Vector2(AssetManager.PlayerTexture.Width * 0.5f, AssetManager.PlayerTexture.Height * 0.5f);
                var halfSize = origin * scale;

                _world.Create(
                    new Position { Current = pos },
                    new Velocity { Current = randomVelocity * 200f },
                    new Sprite
                    {
                        Texture = AssetManager.PlayerTexture,
                        Scale = Vector2.One * scale,
                        Color = randomColor,
                        Origin = origin,
                        HalfSize = halfSize
                    },
                    new Visible()
                );
            }
        }

        TimeManager.Update(gameTime);
        InputManager.Update();
        DayTimeManager.Update(TimeManager.DeltaTime);

#if DEBUG
        if (AssetManager.NeedsReload)
        {
            AssetManager.PerformReload();
            
            var query = new QueryDescription().WithAll<Sprite>();
            _world.Query(in query, (ref Sprite spr) =>
            {
                spr.Texture = AssetManager.PlayerTexture;
            }); 
        }
#endif

        _deltatimeSystems.Update(in TimeManager.DeltaTime);

        base.Update(gameTime);
        
        VisibilityBuffer.Playback(_world);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Draw the game
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _drawSystems.Update(in _spriteBatch);
        _spriteBatch.End();

        // Draw UI
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.DrawLine(new Vector2(ScreenCenter.X, 0f), new Vector2(ScreenCenter.X, ScreenCenter.Y * 2f),
            new Color(255,0,0,64), 4f);
        _spriteBatch.DrawLine(new Vector2(0f, ScreenCenter.Y), new Vector2(ScreenCenter.X * 2f, ScreenCenter.Y),
            new Color(255,0,0,64), 4f);

        var entitiesLabelText = "Entities:";
        var entitiesCountText = $"{_world.Size}";
        var drawScale = 2f;

        var entitiesLabelSize = AssetManager.Font.MeasureString(entitiesLabelText) * drawScale;

        _spriteBatch.DrawOutlinedString(
            AssetManager.Font, entitiesLabelText, new Vector2(ScreenCenter.X, 10f),
            Color.White, 0f,
            FontUtils.CalculateFontOriginTopCenter(entitiesLabelText, AssetManager.Font), drawScale, SpriteEffects.None,
            0f, OutlineFlags.Cross, Color.Black);
        
        _spriteBatch.DrawOutlinedString(AssetManager.Font, entitiesCountText,
            new Vector2(ScreenCenter.X, 10f + entitiesLabelSize.Y), Color.Red, 0f,
            FontUtils.CalculateFontOriginTopCenter(entitiesCountText, AssetManager.Font), drawScale, SpriteEffects.None,
            0f, OutlineFlags.Cross, Color.Black);


        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _jobScheduler.Dispose();
        _world.Dispose();
        _deltatimeSystems.Dispose();
    }
}