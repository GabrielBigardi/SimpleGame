using System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schedulers;
using SimpleGame.Engine;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems;

namespace SimpleGame;

public class Game1 : Game
{
    // ECS
    private World _world;
    private JobScheduler _jobScheduler;
    private SpriteDrawSystem _spriteDrawSystem;
    private QueryDescription _spriteQuery =  new QueryDescription().WithAll<Position, SimpleGame.Engine.ECS.Components.Sprite>();

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Vector2 ScreenCenter => new(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f);

    private Random _random = new();
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _world = World.Create();
        
        _jobScheduler = new JobScheduler(
            new JobScheduler.Config
            {
                ThreadPrefixName = "Arch.Samples",
                ThreadCount = 0,                         
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            }
        );
        
        World.SharedJobScheduler = _jobScheduler;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _spriteDrawSystem =  new SpriteDrawSystem(_world, _spriteQuery, _spriteBatch);
        
        AssetManager.Load(Content);
#if DEBUG
        AssetManager.InitializeHotReload();
#endif
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
        {
            for (var i = 0; i < 1000; i++)
            {
                var randomX = _random.Next(0, _graphics.PreferredBackBufferWidth);
                var randomY = _random.Next(0, _graphics.PreferredBackBufferHeight);
                var pos  = new Vector2(randomX, randomY);
                
                _world.Create(new Position(pos), new SimpleGame.Engine.ECS.Components.Sprite(AssetManager.Player, Vector2.One * 0.25f));
            }
        }

        TimeManager.Update(gameTime);
        InputManager.Update();
        DayTimeManager.Update(TimeManager.DeltaTime);
        
#if DEBUG
        if (AssetManager.NeedsReload)
        {
            AssetManager.PerformReload();
        }
#endif

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        _spriteDrawSystem.Update();
        _spriteBatch.End();

        // Draw UI
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(AssetManager.Font, $"Entities: {_world.Size}", new Vector2(ScreenCenter.X, 10f),
                Color.Red, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        _jobScheduler.Dispose();
        _world.Dispose();
    }
}