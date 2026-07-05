using System;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Schedulers;
using SimpleGame.Engine;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems;
using SimpleGame.Engine.Managers;

namespace SimpleGame;

public class Game1 : Game
{
    #region ECS

    private World _world;
    private JobScheduler _jobScheduler;
    private readonly CommandBuffer _destroyBuffer = new();
    
    private VelocitySystem _velocitySystem;
    private EntityDestroySystem _destroySystem;
    private ModelDrawSystem _modelDrawSystem;

    #endregion

    #region Monogame

    private SpriteBatch _spriteBatch;
    private GraphicsDeviceManager _graphics;
    
    #endregion

    #region Lighting
    
    private RenderTarget2D _shadowMap;
    private Vector3 _lightDirection = Vector3.Normalize(new Vector3(-1, -1, -1));
    private Matrix _lightView;
    private Matrix _lightProjection;
    
    #endregion
    
    private Entity _player;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef; // Better for 3D

        // Unlimited FPS
        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;

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

        World.SharedJobScheduler = _jobScheduler;

        _destroySystem = new EntityDestroySystem(_world, _destroyBuffer);
        _velocitySystem = new VelocitySystem(_world);
        _modelDrawSystem = new ModelDrawSystem(_world, _graphics.GraphicsDevice);

        CameraManager.Initialize(_graphics);

        base.Initialize();

        // If this is a AOT build add components to arrayregistry
        if (!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
        {
            ArrayRegistry.Add<Destroy>();
            ArrayRegistry.Add<Position>();
            ArrayRegistry.Add<Rotation>();
            ArrayRegistry.Add<Velocity>();
            ArrayRegistry.Add<ModelComponent>();
        }
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Let's reset the depth buffer format just in case
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
        _graphics.ApplyChanges();
        
        // Create Shadow Map
        _shadowMap = new RenderTarget2D(GraphicsDevice, 2048, 2048, false, SurfaceFormat.Single, DepthFormat.Depth24);

        AssetManager.Load(Content);

        // Create player
        _player = _world.Create(
            new Position { Current = Vector3.Zero },
            new Rotation { Current = Vector3.Zero },
            new Velocity { Current = Vector3.Zero },
            new ModelComponent
            {
                Model = AssetManager.PlaceholderModel,
                Scale = 1f
            }
        );
        
        var player2 = _world.Create(
            new Position { Current = new Vector3(50, 0, 50) },
            new Rotation { Current = Vector3.Zero },
            new Velocity { Current = Vector3.Zero },
            new ModelComponent
            {
                Model = AssetManager.PlaceholderModel,
                Scale = 1f
            }
        );
        
        CameraManager.SetCameraTarget(_player.Get<Position>().Current);
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive)
            return;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Input on X/Z plane
        var input = InputManager.NormalizedPlayerInput;
        _player.Get<Velocity>().Current = new Vector3(input.X, 0, input.Y) * 100f;
        
        // Update rotation slightly based on velocity
        if (input != Vector2.Zero)
        {
            _player.Get<Rotation>().Current = new Vector3(0, MathF.Atan2(input.X, input.Y), 0);
        }

        CameraManager.SetCameraTarget(Vector3.Lerp(
            CameraManager.CameraTarget,
            _player.Get<Position>().Current,
            10f * TimeManager.DeltaTime
        ));
        
        CameraManager.SetCameraPosition(CameraManager.CameraTarget + new Vector3(0, 150, 150)); // Follow player
        
        // Update Light Matrices
        var lightPos = CameraManager.CameraTarget - _lightDirection * 500f;
        _lightView = Matrix.CreateLookAt(lightPos, CameraManager.CameraTarget, Vector3.Up);
        _lightProjection = Matrix.CreateOrthographic(1000f, 1000f, 1f, 2000f);

        TimeManager.Update(gameTime);
        InputManager.Update();
        ShakeManager.Update(new Random());

        _velocitySystem.Update();
        _destroySystem.Update();

        _destroyBuffer.Playback(_world);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (AssetManager.ShadowShader != null)
        {
            AssetManager.ShadowShader.Parameters["LightView"]?.SetValue(_lightView);
            AssetManager.ShadowShader.Parameters["LightProjection"]?.SetValue(_lightProjection);
            AssetManager.ShadowShader.Parameters["LightDirection"]?.SetValue(_lightDirection);
        }

        // --- SHADOW PASS ---
        GraphicsDevice.SetRenderTarget(_shadowMap);
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        // Optionally cull front faces for shadow maps to prevent acne
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise; 
        
        _modelDrawSystem.DrawShadowMap();

        // --- MAIN PASS ---
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(59, 48, 78), 1.0f, 0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        if (AssetManager.ShadowShader != null)
        {
            AssetManager.ShadowShader.Parameters["ShadowMap"]?.SetValue(_shadowMap);
        }

        _modelDrawSystem.DrawModels();

        DrawUI();

        base.Draw(gameTime);
    }

    private void DrawUI()
    {
        // Draw UI
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var entitiesLabelText = "Entities:";
        var entitiesCountText = $"{_world.Size}";
        var drawScale = 2f;

        var entitiesLabelSize = AssetManager.Font.MeasureString(entitiesLabelText) * drawScale;

        // Note: I removed FontUtils from this call to simplify, if it causes issues we can bring it back.
        // Assuming FontUtils is still present, let's use it:
        _spriteBatch.DrawString(AssetManager.Font, entitiesLabelText, new Vector2(10, 10), Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(AssetManager.Font, entitiesCountText, new Vector2(10, 10 + entitiesLabelSize.Y), Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);

        _spriteBatch.End();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _jobScheduler.Dispose();
        _world.Dispose();
    }
}