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
using SimpleGame.Engine.NonECS;
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

    private RenderTarget2D _lightMaskTarget;

    private LightSource _playerLight;
    private readonly List<LightSource> _lightSources = new();

    private List<(Vector2 A, Vector2 B)> _walls = new();
    private BasicEffect _shadowEffect;

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

        // Initialize lightmask
        var presentationParameters = GraphicsDevice.PresentationParameters;
        _lightMaskTarget = new RenderTarget2D(GraphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

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

        _player = _world.Create(
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

        _playerLight = new LightSource()
        {
            Position = _player.Get<Position>().Current,
            Scale = 4f,
            Color = Color.White,
        };

        _lightSources.Add(_playerLight);

        Vector2 topLeft = new Vector2(400, 300);
        Vector2 topRight = new Vector2(600, 300);
        Vector2 bottomLeft = new Vector2(400, 500);
        Vector2 bottomRight = new Vector2(600, 500);

        _walls.Add((topLeft, bottomLeft)); // Left edge (going down)
        _walls.Add((bottomLeft, bottomRight)); // Bottom edge (going right)
        _walls.Add((bottomRight, topRight)); // Right edge (going up)
        _walls.Add((topRight, topLeft)); // Top edge (going left)


        // Init shadow effect
        _shadowEffect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            Projection = Matrix.CreateOrthographicOffCenter
            (
                0,
                CachedPreferredBackBufferWidth,
                CachedPreferredBackBufferHeight,
                0,
                0,
                1
            ),
            View = Matrix.Identity,
            World = GetCameraMatrix()
        };
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
            explosionInstance.Pitch = _random.NextFloat(-0.575f, 0.2f);
            explosionInstance.Play();

            var query = new QueryDescription().WithAll<Sprite>();
            _world.Query(in query, (Entity entity, ref Sprite spr) =>
            {
                if (entity != _player)
                    entity.Add<Destroy>();
            });
        }

        _player.Get<Velocity>().Current = InputManager.NormalizedPlayerInput * 100f;
        _playerLight.Position = _player.Get<Position>().Current;

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
        // Draw lightmask
        GraphicsDevice.SetRenderTarget(_lightMaskTarget);
        
        Color ambientDarkness = new Color(0,0,40); 
        GraphicsDevice.Clear(ambientDarkness);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, null, null, null, GetCameraMatrix());
        
        foreach (var lightSource in _lightSources)
            lightSource.Draw(_spriteBatch, 1f);

        _spriteBatch.End();


        //TODO: CUT OUT SHADOWS
        // 1. Use Opaque to overwrite the light with the ambient darkness
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        
        // 2. Properly route your matrices!
        _shadowEffect.World = Matrix.Identity;
        _shadowEffect.View = GetCameraMatrix(); // Camera goes in the View matrix
        _shadowEffect.Projection = Matrix.CreateOrthographicOffCenter(
            0, CachedPreferredBackBufferWidth, CachedPreferredBackBufferHeight, 0, 0, 1);
        
        float shadowLength = 2000f;
        
        // 3. Our "eraser" ink is just the ambient lighting of the day
        Color shadowColor = ambientDarkness;
        
        foreach (var light in _lightSources)
        {
            foreach (var wall in _walls)
            {
                Vector2 a = wall.A;
                Vector2 b = wall.B;
        
                Vector2 edge = b - a;
                Vector2 normal = new Vector2(-edge.Y, edge.X);
        
                if (Vector2.Dot(normal, light.Position - a) <= 0)
                    continue;
        
                Vector2 dirA = Vector2.Normalize(a - light.Position);
                Vector2 dirB = Vector2.Normalize(b - light.Position);
        
                Vector2 aFar = a + dirA * shadowLength;
                Vector2 bFar = b + dirB * shadowLength;
        
                // Draw using the ambient color so it blends flawlessly with unlit areas
                var verts = new VertexPositionColor[6]
                {
                    new(new Vector3(a, 0), shadowColor),
                    new(new Vector3(b, 0), shadowColor),
                    new(new Vector3(aFar, 0), shadowColor),
        
                    new(new Vector3(b, 0), shadowColor),
                    new(new Vector3(bFar, 0), shadowColor),
                    new(new Vector3(aFar, 0), shadowColor),
                };
        
                foreach (var pass in _shadowEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives(
                        PrimitiveType.TriangleList,
                        verts,
                        0,
                        2
                    );
                }
            }
        }

        // Draw the game
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(new(100,200,50));

         _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: GetCameraMatrix());
        _spriteDrawSystem.Update();

        foreach (var wall in _walls)
        {
            _spriteBatch.DrawLine(
                wall.A,
                wall.B,
                Color.White,
                3f
            );
        }

        _spriteBatch.End();

        // Apply Stardew like blending
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendStates.MultiplyBlend, SamplerState.LinearClamp);
        _spriteBatch.Draw(_lightMaskTarget, Vector2.Zero, _lightMaskTarget.Bounds, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        _spriteBatch.End();

        // Draw UI
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Debug
        //_spriteBatch.DrawLine(new Vector2(ScreenCenter.X, 0f), new Vector2(ScreenCenter.X, ScreenCenter.Y * 2f),
        //    new Color(255, 0, 0, 24), 4f);
        //_spriteBatch.DrawLine(new Vector2(0f, ScreenCenter.Y), new Vector2(ScreenCenter.X * 2f, ScreenCenter.Y),
        //    new Color(255, 0, 0, 24), 4f);


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