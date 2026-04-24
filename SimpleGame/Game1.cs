using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Schedulers;
using SimpleGame.Engine;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems;
using SimpleGame.Engine.Managers;
using SimpleGame.Engine.Utils;

namespace SimpleGame;

public class Game1 : Game
{
    #region ECS

    private World _world;
    private JobScheduler _jobScheduler;

    private readonly CommandBuffer _visibleBuffer = new();
    private readonly CommandBuffer _hiddenBuffer = new();
    private readonly CommandBuffer _destroyBuffer = new();

    private VisibleCheckSystem _visibleSystem;
    private HiddenCheckSystem _hiddenSystem;
    private SpriteDrawSystem _spriteDrawSystem;
    private LightDrawSystem _lightDrawSystem;
    private VelocitySystem _velocitySystem;
    private EntityDestroySystem _destroySystem;
    private SpriteFlipSystem _spriteFlipSystem;

    #endregion

    #region Monogame

    private SpriteBatch _spriteBatch;
    public static GraphicsDeviceManager _graphics;
    
    #endregion

    #region Lighting

    private RenderTarget2D _lightMaskTarget;

    // Shadows
    private readonly List<(Vector2 A, Vector2 B)> _walls = [];
    private BasicEffect _shadowEffect;

    private readonly DepthStencilState[] _writeStencilStates = new DepthStencilState[256];
    private readonly DepthStencilState[] _readStencilStates = new DepthStencilState[256];

    #endregion

    public static Vector2 ScreenCenter =>
        new(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f);

    private readonly Random _random = new();
    public static Entity _player;

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
        _world = World.Create();

        _jobScheduler = new JobScheduler(
            new JobScheduler.Config
            {
                ThreadPrefixName = "SimpleGame",
            }
        );

        Console.WriteLine(_jobScheduler.ThreadCount);

        World.SharedJobScheduler = _jobScheduler;

        _visibleSystem = new VisibleCheckSystem(_world, _visibleBuffer);
        _hiddenSystem = new HiddenCheckSystem(_world, _hiddenBuffer);
        _destroySystem = new EntityDestroySystem(_world, _destroyBuffer);
        _velocitySystem = new VelocitySystem(_world);
        _spriteFlipSystem = new SpriteFlipSystem(_world);

        base.Initialize();

        // If this is a AOT build add components to arrayregistry
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            ArrayRegistry.Add<Destroy>();
            ArrayRegistry.Add<LightSource>();
            ArrayRegistry.Add<Position>();
            ArrayRegistry.Add<Shadow>();
            ArrayRegistry.Add<ShadowEmitter>();
            ArrayRegistry.Add<Sprite>();
            ArrayRegistry.Add<Velocity>();
            ArrayRegistry.Add<Visible>();
        }
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _spriteDrawSystem = new SpriteDrawSystem(_world, _spriteBatch);
        _lightDrawSystem = new LightDrawSystem(_world, _spriteBatch);

        // Initialize light mask
        var presentationParameters = GraphicsDevice.PresentationParameters;

        //_lightMaskTarget = new RenderTarget2D(GraphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

        _lightMaskTarget = new RenderTarget2D(
            GraphicsDevice,
            presentationParameters.BackBufferWidth,
            presentationParameters.BackBufferHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24Stencil8, // <--- CRITICAL: Enables the Stencil Buffer
            0,
            RenderTargetUsage.PreserveContents
        );

        // Generate our 255 stencil states once during load
        for (int i = 0; i < 256; i++)
        {
            _writeStencilStates[i] = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = i,
                DepthBufferEnable = false
            };

            _readStencilStates[i] = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.NotEqual,
                ReferenceStencil = i,
                DepthBufferEnable = false
            };
        }

        AssetManager.Load(Content);
#if DEBUG
        AssetManager.InitializeHotReload();
#endif

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 1f;
        MediaPlayer.Play(AssetManager.GameplaySong);

        var scale = 1f;
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
                Scale = scale,
                Offset = new Vector2(0f, 8f),
                //Origin = new Vector2(spriteSize[0] * 0.5f, spriteSize[1] * 0.5f),
                Source = new Rectangle(192, 416, spriteSize[0], spriteSize[1]),
                Color = Color.Black * 0.5f
            },
            new LightSource()
            {
                Scale = 4f,
                Color = Color.White,
                Origin = new(AssetManager.LightGradientTexture.Width * 0.5f,
                    AssetManager.LightGradientTexture.Height * 0.5f)
            },
            new ShadowEmitter(),
            new Visible()
        );

        var topLeft = new Vector2(400, 300);
        var topRight = new Vector2(600, 300);
        var bottomLeft = new Vector2(400, 500);
        var bottomRight = new Vector2(600, 500);

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
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight,
                0,
                0,
                1
            ),
            View = Matrix.Identity,
            World = CameraManager.GetCameraMatrix()
        };

        CameraManager.SetCameraPosition(_player.Get<Position>().Current);
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive)
            return;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (InputManager.IsLeftMouseDown())
        {
            //var bubblePopInstance = AssetManager.BubblePopSound.CreateInstance();
            //bubblePopInstance.Pitch = _random.NextFloat(-1f, 1f);
            //bubblePopInstance.Play();

            for (var i = 0; i < 10; i++)
            {
                var randomX = _random.Next(100, _graphics.PreferredBackBufferWidth - 100);
                var randomY = _random.Next(100, _graphics.PreferredBackBufferHeight - 100);
                var pos = new Vector2(randomX, randomY);

                var randomVelocity = VectorUtils.RandomInsideUnitCircle(_random);
                randomVelocity.Normalize();

                var scale = 1f;
                var randomColor = ColorUtils.RandomColor(_random);
                var spriteSize = new[] { 16, 16 };
                var origin = new Vector2(spriteSize[0] * 0.5f, spriteSize[1] * 0.5f);
                var halfSize = origin * scale;

                var monstersConfigList = new List<Tuple<int, int, int>>()
                {
                    new (64, 496, 4), // Slime
                    new (80, 496, 8), // Goblin
                    new (96, 496, 8), // Planta
                    new (112, 496, 8), // ?
                    new (128, 496, 8), // Sereia
                    new (144, 496, 8), // Gargoyle
                    new (160, 496, 8), // Mimic
                    new (176, 496, 8), // Tree
                    new (192, 496, 8), // Reaper
                    new (208, 496, 8), // Mula
                    new (64, 512, 8), // DragaoGreen
                    new (80, 512, 8), // DragaoRed
                    new (96, 512, 8), // DragaoBlue
                    new (112, 512, 8), // Skeleton
                    new (128, 512, 8), // Crab
                    new (144, 512, 8), // Reaper2
                    new (160, 512, 8), // ?
                    new (176, 512, 8), // Eye
                    new (192, 512, 8), // Sereia2
                    new (208, 512, 8), // Imp
                    new (64, 528, 8), // Minotaur
                    new (80, 528, 8), // Goblin2
                    new (96, 528, 8), // Goblin3
                    new (112, 528, 8), // Shadow
                    new (128, 528, 8), // ?
                    new (144, 528, 8), // ?
                    new (160, 528, 8), // ?
                    new (176, 528, 8), // ?
                    new (192, 528, 8), // Skull
                    new (208, 528, 8), // MulaHumana
                };

                var spriteToUse = monstersConfigList[_random.Next(monstersConfigList.Count)];

                // Create monster
                _world.Create(
                    new Position { Current = pos },
                    new Velocity { Current = randomVelocity * 4f },
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
                        Scale = scale,
                        Offset = new Vector2(0f, spriteToUse.Item3),
                        //Origin = new Vector2(spriteSize[0] * 0.5f, spriteSize[1] * 0.5f),
                        Source = new Rectangle(192, 416, spriteSize[0], spriteSize[1]),
                        Color = Color.Black * 0.5f
                    },
                    new LightSource()
                    {
                        Color = Color.White * 1f,
                        Origin = new(AssetManager.LightGradientTexture.Width * 0.5f,
                            AssetManager.LightGradientTexture.Height * 0.5f),
                        Scale = 4f
                    },
                    //new ShadowEmitter(),
                    new Visible()
                );
            }
        }

        if (InputManager.RightMousePressedThisFrame())
        {
            ShakeManager.Shake(0.3f, 10f);

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

        //_cameraPosition = _player.Get<Position>().Current;
        CameraManager.SetCameraPosition(Vector2.Lerp(
            CameraManager.CameraPosition,
            _player.Get<Position>().Current,
            10f * TimeManager.DeltaTime
        ));

        TimeManager.Update(gameTime);
        InputManager.Update();
        DayTimeManager.Update(TimeManager.DeltaTime);
        ShakeManager.Update(_random);

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
        _spriteFlipSystem.Update();

        _visibleBuffer.Playback(_world);
        _hiddenBuffer.Playback(_world);
        _destroyBuffer.Playback(_world);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var ambientDarkness = DayTimeManager.CurrentLighting;

        DrawLightmapAndBasicLights(ambientDarkness);
        DrawShadowEmittingLights(ambientDarkness);
        //DrawCutoutShadows(ambientDarkness);

        DrawGame();
        DrawBlendedLightmap();
        DrawUI();

        base.Draw(gameTime);
    }

    private void DrawShadowEmittingLights(Color ambientDarkness)
    {
        var noColorWriteBlend = new BlendState { ColorWriteChannels = ColorWriteChannels.None };

        _shadowEffect.World = Matrix.Identity;
        _shadowEffect.View = CameraManager.GetCameraMatrix();
        _shadowEffect.Projection = Matrix.CreateOrthographicOffCenter(0, _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight, 0, 0, 1);

        int stencilRef = 1; // Start at ID 1

        var query = new QueryDescription().WithAll<Position, LightSource, ShadowEmitter>();
        _world.Query(in query, (ref Position pos, ref LightSource light, ref ShadowEmitter emitter) =>
        {
            if (stencilRef > 255)
            {
                GraphicsDevice.Clear(ClearOptions.Stencil, Color.Transparent, 0, 0);
                stencilRef = 1;
            }

            // Grab the PRE-CACHED write state for this ID
            GraphicsDevice.DepthStencilState = _writeStencilStates[stencilRef];
            GraphicsDevice.BlendState = noColorWriteBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            float shadowLength = 2000f;

            foreach (var wall in _walls)
            {
                Vector2 a = wall.A;
                Vector2 b = wall.B;

                Vector2 edge = b - a;
                Vector2 normal = new Vector2(-edge.Y, edge.X);

                if (Vector2.Dot(normal, pos.Current - a) <= 0) continue;

                Vector2 dirA = Vector2.Normalize(a - pos.Current);
                Vector2 dirB = Vector2.Normalize(b - pos.Current);

                var verts = new VertexPositionColor[6]
                {
                    new(new Vector3(a, 0), Color.White), new(new Vector3(b, 0), Color.White),
                    new(new Vector3(a + dirA * shadowLength, 0), Color.White),
                    new(new Vector3(b, 0), Color.White), new(new Vector3(b + dirB * shadowLength, 0), Color.White),
                    new(new Vector3(a + dirA * shadowLength, 0), Color.White),
                };

                foreach (var pass in _shadowEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                }
            }

            // Grab the PRE-CACHED read state for this ID
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                _readStencilStates[stencilRef], RasterizerState.CullNone, null, CameraManager.GetCameraMatrix());

            _spriteBatch.Draw(AssetManager.LightGradientTexture, pos.Current, null, light.Color, 0f, light.Origin,
                light.Scale, SpriteEffects.None, 0f);

            _spriteBatch.End();

            // Increment the ID for the next light!
            stencilRef++;
        });
    }

    private void DrawLightmapAndBasicLights(Color ambientDarkness)
    {
        // Clear lightmap/stencil buffer
        GraphicsDevice.SetRenderTarget(_lightMaskTarget);
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.Stencil, ambientDarkness, 0, 0);

        // Draw lights in a additive way
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, null, null, null, CameraManager.GetCameraMatrix());

        _lightDrawSystem.Update();

        _spriteBatch.End();
    }

    private void DrawGame()
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(new(59, 48, 78));

        _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp,
            transformMatrix: CameraManager.GetCameraMatrix());
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
    }

    private void DrawBlendedLightmap()
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendStates.MultiplyBlend, SamplerState.LinearClamp);
        _spriteBatch.Draw(_lightMaskTarget, Vector2.Zero, _lightMaskTarget.Bounds, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 1f);
        _spriteBatch.End();
    }

    private void DrawUI()
    {
        // Draw UI
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Debug
        //_spriteBatch.DrawLine(new Vector2(ScreenCenter.X, 0f), new Vector2(ScreenCenter.X, ScreenCenter.Y * 2f),
        //    new Color(255, 0, 0, 24), 4f);
        //_spriteBatch.DrawLine(new Vector2(0f, ScreenCenter.Y), new Vector2(ScreenCenter.X * 2f, ScreenCenter.Y),
        //    new Color(255, 0, 0, 24), 4f);

        var entitiesLabelText = "Entities:";
        var visibleEntitiesCount = _world.CountEntities(new QueryDescription().WithAll<Visible>());
        var entitiesCountText = $"{visibleEntitiesCount}/{_world.Size}";
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
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _jobScheduler.Dispose();
        _world.Dispose();
    }
}