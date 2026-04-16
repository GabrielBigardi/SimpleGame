using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Schedulers;
using SimpleGame.Engine;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems;
using SimpleGame.Engine.Utils;

namespace SimpleGame;

public class Game1 : Game
{
    // ECS
    private World _world;
    private JobScheduler _jobScheduler;

    private readonly CommandBuffer _visibilityBuffer = new();
    private readonly CommandBuffer _destroyBuffer = new();

    private VisibleCheckSystem _visibleSystem;
    private HiddenCheckSystem _hiddenSystem;
    private SpriteDrawSystem _spriteDrawSystem;
    private VelocitySystem _velocitySystem;
    private EntityDestroySystem _destroySystem;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public static int CachedPreferredBackBufferWidth;
    public static int CachedPreferredBackBufferHeight;

    private Vector2 ScreenCenter => new(CachedPreferredBackBufferWidth / 2f, CachedPreferredBackBufferHeight / 2f);

    private Random _random = new();

    private Entity _player;
    private Vector2 _cameraPosition;
    
    private float _shakeTime;
    private float _shakeDuration;
    private float _shakeStrength;
    private Vector2 _shakeOffset;

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

        _jobScheduler = new JobScheduler(
            new JobScheduler.Config
            {
                ThreadPrefixName = "SimpleGame",
            }
        );

        Console.WriteLine(_jobScheduler.ThreadCount);

        World.SharedJobScheduler = _jobScheduler;

        // Culling System (2 systems)
        _visibleSystem = new VisibleCheckSystem(_world, _visibilityBuffer);
        _hiddenSystem = new HiddenCheckSystem(_world, _visibilityBuffer);
        _destroySystem = new EntityDestroySystem(_world, _destroyBuffer);
        _velocitySystem = new VelocitySystem(_world);

        base.Initialize();

        // If this is a AOT build add components to arrayregistry
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            ArrayRegistry.Add<Destroy>();
            ArrayRegistry.Add<Position>();
            ArrayRegistry.Add<Shadow>();
            ArrayRegistry.Add<Sprite>();
            ArrayRegistry.Add<Velocity>();
            ArrayRegistry.Add<Visible>();
        }
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _spriteDrawSystem = new SpriteDrawSystem(_world, _spriteBatch);

        AssetManager.Load(Content);
#if DEBUG
        AssetManager.InitializeHotReload();
#endif

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 1f;
        MediaPlayer.Play(AssetManager.GameplaySong);

        var scale = 2f;
        var spriteSize = new[] { 16, 16 };
        var origin = new Vector2(spriteSize[0] * 0.5f, spriteSize[1] * 0.5f);
        var halfSize = origin * scale;
        
        _player =  _world.Create(
                new Position { Current = ScreenCenter },
                new Velocity(),
                new Sprite
                {
                    Texture = AssetManager.RoguelikeAtlas,
                    Scale = Vector2.One * scale,
                    Color = Color.White,
                    Origin = origin,
                    Source = new Rectangle(176, 544, spriteSize[0], spriteSize[1]),
                    HalfSize = halfSize
                },
                new Shadow
                {
                    Texture = AssetManager.RoguelikeAtlas,
                    Scale = 1f,
                    Offset = new Vector2(0f, 16f),
                    //Origin = new Vector2(spriteSize[0] * 0.5f, spriteSize[1] * 0.5f),
                    Source = new Rectangle(192, 416, spriteSize[0], spriteSize[1]),
                    Color = Color.Black * 0.5f
                },
                new Visible()
            );
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive) 
            return;
        
        if (_shakeTime > 0f)
        {
            _shakeTime -= TimeManager.DeltaTime;

            var progress = _shakeTime / _shakeDuration; // fades out

            _shakeOffset = new Vector2(
                _random.NextFloat(-1f, 1f),
                _random.NextFloat(-1f, 1f)
            ) * _shakeStrength * progress;
        }
        else
        {
            _shakeOffset = Vector2.Zero;
        }
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (InputManager.LeftMousePressedThisFrame())
        {
            var bubblePopInstance = AssetManager.BubblePopSound.CreateInstance();
            bubblePopInstance.Pitch = _random.NextFloat(-1f, 1f);
            bubblePopInstance.Play();
            
            for (var i = 0; i < 10; i++)
            {
                var randomX = _random.Next(100, CachedPreferredBackBufferWidth - 100);
                var randomY = _random.Next(100, CachedPreferredBackBufferHeight - 100);
                var pos = new Vector2(randomX, randomY);

                var randomVelocity = VectorUtils.RandomInsideUnitCircle(_random);
                randomVelocity.Normalize();

                var scale = 2f;
                var randomColor = ColorUtils.RandomColor(_random);
                var spriteSize = new[] { 16, 16 };
                var origin = new Vector2(spriteSize[0] * 0.5f, spriteSize[1] * 0.5f);
                var halfSize = origin * scale;

                var monstersStartSource = new ValueTuple<int, int>(64, 496);
                var spritesSourceList = new List<(int, int)>();
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        var bla = new ValueTuple<int, int>(monstersStartSource.Item1 + spriteSize[0] * x,
                            monstersStartSource.Item2 + spriteSize[0] * y);
                        spritesSourceList.Add(bla);
                    }
                }

                var spriteToUse = spritesSourceList[_random.Next(spritesSourceList.Count)];

                // Create monster
                _world.Create(
                    new Position { Current = pos },
                    new Velocity { Current = randomVelocity * 50f },
                    new Sprite
                    {
                        Texture = AssetManager.RoguelikeAtlas,
                        Scale = Vector2.One * scale,
                        Color = Color.White,
                        Origin = origin,
                        Source = new Rectangle(spriteToUse.Item1, spriteToUse.Item2, spriteSize[0], spriteSize[1]),
                        HalfSize = halfSize
                    },
                    new Shadow
                    {
                        Texture = AssetManager.RoguelikeAtlas,
                        Scale = 1f,
                        Offset = new Vector2(0f, 16f),
                        //Origin = new Vector2(spriteSize[0] * 0.5f, spriteSize[1] * 0.5f),
                        Source = new Rectangle(192, 416, spriteSize[0], spriteSize[1]),
                        Color = Color.Black * 0.5f
                    },
                    new Visible()
                );
            }
        }

        if (InputManager.RightMousePressedThisFrame())
        {
            Shake(0.3f, 10f);
            
            var explosionInstance = AssetManager.ExplosionSound.CreateInstance();
            explosionInstance.Pitch = _random.NextFloat(-1f, 0.25f);
            explosionInstance.Play();
            
            var query = new QueryDescription().WithAll<Sprite>();
            _world.Query(in query, (Entity entity, ref Sprite spr) => {
                if (entity != _player)
                    entity.Add<Destroy>();
            }); 
        }

        _player.Get<Velocity>().Current = InputManager.NormalizedPlayerInput * 100f;
        
        //_cameraPosition = _player.Get<Position>().Current;
        _cameraPosition = Vector2.Lerp(
            _cameraPosition,
            _player.Get<Position>().Current,
            10f * TimeManager.DeltaTime
        );
        
        TimeManager.Update(gameTime);
        InputManager.Update();
        DayTimeManager.Update(TimeManager.DeltaTime);

#if DEBUG
        if (AssetManager.NeedsReload)
        {
            AssetManager.PerformReload();

            var query = new QueryDescription().WithAll<Sprite>();
            _world.Query(in query, (ref Sprite spr) => { spr.Texture = AssetManager.RoguelikeAtlas; });
        }
#endif

        _velocitySystem.Update();
        _visibleSystem.Update();
        _hiddenSystem.Update();
        _destroySystem.Update();

        _visibilityBuffer.Playback(_world);
        _destroyBuffer.Playback(_world);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Draw the game
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(new(59, 48, 78));

        //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: GetCameraMatrix());
        _spriteDrawSystem.Update();
        _spriteBatch.End();

        // Draw UI
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.DrawLine(new Vector2(ScreenCenter.X, 0f), new Vector2(ScreenCenter.X, ScreenCenter.Y * 2f),
            new Color(255, 0, 0, 24), 4f);
        _spriteBatch.DrawLine(new Vector2(0f, ScreenCenter.Y), new Vector2(ScreenCenter.X * 2f, ScreenCenter.Y),
            new Color(255, 0, 0, 24), 4f);

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
    }
    
    private void Shake(float duration, float strength)
    {
        _shakeDuration = duration;
        _shakeTime = duration;
        _shakeStrength = strength;
    }
    
    private Matrix GetCameraMatrix()
    {
        return Matrix.CreateTranslation(new Vector3(
            -_cameraPosition.X + ScreenCenter.X + _shakeOffset.X,
            -_cameraPosition.Y + ScreenCenter.Y + _shakeOffset.Y,
            0f));
    }
}