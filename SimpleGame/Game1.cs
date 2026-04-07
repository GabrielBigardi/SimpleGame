using System;
using System.Runtime.CompilerServices;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schedulers;
using SimpleGame.Engine;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems;
using Sprite = SimpleGame.Engine.Sprite;

namespace SimpleGame;

public class Game1 : Game
{
    // ECS
    private World _world;
    private SpriteDrawSystem _spriteDrawSystem;
    private QueryDescription _spriteQuery =  new QueryDescription().WithAll<Position, SimpleGame.Engine.ECS.Components.Sprite>();

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Vector2 ScreenCenter =>
        new(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f);

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
        
        var jobScheduler = new JobScheduler(
            new JobScheduler.Config
            {
                ThreadPrefixName = "Arch.Samples",
                ThreadCount = 0,                         
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            }
        );
        
        World.SharedJobScheduler = jobScheduler;
        

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
            for (var i = 0; i < 5000; i++)
            {
                var randomX = new Random().Next(0, _graphics.PreferredBackBufferWidth);
                var randomY = new Random().Next(0, _graphics.PreferredBackBufferHeight);
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
        
        // var query = _world.Query(in _spriteQuery);
        // foreach(ref var chunk in query.GetChunkIterator())
        // {
        //     var references = chunk.GetFirst<Position, SimpleGame.Engine.ECS.Components.Sprite>();  
        //     foreach(var entity in chunk)                          
        //     {
        //         ref var pos = ref Unsafe.Add(ref references.t0, entity);
        //         ref var spr = ref Unsafe.Add(ref references.t1, entity);
        //
        //         pos.Current += spr.Scale;
        //         //_spriteBatch.Draw(spr.Texture, pos.Current, null, Color.White, 0, new(spr.Texture.Width / 2f, spr.Texture.Height / 2f), spr.Scale, SpriteEffects.None, 0f);
        //     }
        // }

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
}