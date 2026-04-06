using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleGame.Engine;

namespace SimpleGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private RenderTarget2D _lightMaskTarget;

    // private PhysicsBody _player;
    // private Sprite _playerLight;
    //
    // private PhysicsBody _playerB;
    // private Sprite _playerBLight;
    //
    // private Sprite _gameWorld;

    private GameObject _gameWorld;
    
    private GameObject _player;
    private GameObject _playerLight;
    
    private GameObject _playerB;
    private GameObject _playerBLight;

    private List<GameObject> _gameObjects = new();
    private List<GameObject> _worldObjects = new();
    private List<GameObject> _lightSources = new();
    private List<PhysicsBody> _physicsObjects = new();

    private Vector2 ScreenCenter =>
        new(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f);

    // private readonly List<Sprite> _sprites = new();
    // private readonly List<PhysicsBody> _physicsSprite = new();
    // private readonly List<LightSource> _lightSources = new();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
    }

    protected override void Initialize() => base.Initialize();

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var presentationParameters = GraphicsDevice.PresentationParameters;
        _lightMaskTarget = new RenderTarget2D(GraphicsDevice, presentationParameters.BackBufferWidth,
            presentationParameters.BackBufferHeight);

        AssetManager.Load(Content);
#if DEBUG
        AssetManager.InitializeHotReload();
#endif
        _gameWorld = new GameObject();
        var gameWorldTransform = _gameWorld.AddComponent<Transform>();
        gameWorldTransform.Position = ScreenCenter;
        gameWorldTransform.Scale = Vector2.One * 3f;
        var gameWorldSprite = _gameWorld.AddComponent<Sprite>();
        gameWorldSprite.Transform = gameWorldTransform;
        gameWorldSprite.Texture = AssetManager.GameWorldTexture;
        _gameObjects.Add(_gameWorld);
        _worldObjects.Add(_gameWorld);

        _player = new GameObject();
        var playerTransform = _player.AddComponent<Transform>();
        playerTransform.Position = ScreenCenter;
        playerTransform.Scale = Vector2.One;
        var playerSprite = _player.AddComponent<Sprite>();
        playerSprite.Transform = playerTransform;
        playerSprite.Texture = AssetManager.Player;
        var playerPhysics = _player.AddComponent<PhysicsBody>();
        playerPhysics.Transform  = playerTransform;
        playerPhysics.ColliderSize = new Vector2(64, 120 - 64);
        playerPhysics.CollisionOffset = new Vector2(0, 32);
        _physicsObjects.Add(playerPhysics);
        _gameObjects.Add(_player);
        _worldObjects.Add(_player);
        
        _playerB = new GameObject();
        var playerBTransform = _playerB.AddComponent<Transform>();
        playerBTransform.Position = ScreenCenter;
        playerBTransform.Scale = Vector2.One;
        var playerBSprite = _playerB.AddComponent<Sprite>();
        playerBSprite.Transform = playerBTransform;
        playerBSprite.Texture = AssetManager.Player;
        var playerBPhysics = _playerB.AddComponent<PhysicsBody>();
        playerBPhysics.Transform  = playerBTransform;
        playerBPhysics.ColliderSize = new Vector2(64, 120 - 64);
        playerBPhysics.CollisionOffset = new Vector2(0, 32);
        _physicsObjects.Add(playerBPhysics);
        _gameObjects.Add(_playerB);
        _worldObjects.Add(_playerB);

        _playerLight = new GameObject();
        var playerLightTransform = _playerLight.AddComponent<Transform>();
        playerLightTransform.Position = ScreenCenter;
        playerLightTransform.Scale = Vector2.One * 3f;
        var playerLightSource  = _playerLight.AddComponent<LightSource>();
        playerLightSource.Transform = playerLightTransform;
        playerLightSource.Sprite = new Sprite { Transform =  playerLightTransform, Texture =  AssetManager.LightGradientTexture };
        _gameObjects.Add(_playerLight);
        _lightSources.Add(_playerLight);
        
        _playerBLight = new GameObject();
        var playerBLightTransform = _playerBLight.AddComponent<Transform>();
        playerBLightTransform.Position = ScreenCenter;
        playerBLightTransform.Scale = Vector2.One * 3f;
        var playerBLightSource  = _playerBLight.AddComponent<LightSource>();
        playerBLightSource.Transform = playerBLightTransform;
        playerBLightSource.Sprite = new Sprite { Transform =  playerBLightTransform, Texture =  AssetManager.LightGradientTexture };
        _gameObjects.Add(_playerBLight);
        _lightSources.Add(_playerBLight);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        TimeManager.Update(gameTime);
        InputManager.Update();
        DayTimeManager.Update(TimeManager.DeltaTime);

        foreach (var gameObject in _gameObjects)
            gameObject.Update(TimeManager.DeltaTime);

#if DEBUG
        if (AssetManager.NeedsReload)
        {
            AssetManager.PerformReload();
            _player.GetComponent<Sprite>().Texture = AssetManager.Player;

            foreach (var lightSource in _lightSources)
                lightSource.GetComponent<LightSource>().Sprite.Texture = AssetManager.LightGradientTexture;
        }
#endif
        var velocity = InputManager.NormalizedInput * 200f * TimeManager.DeltaTime;
        
        if (velocity.X > 0)
            _player.GetComponent<Sprite>().FlipX = false;
        
        if (velocity.X < 0)
            _player.GetComponent<Sprite>().FlipX = true;
        
        _player.GetComponent<PhysicsBody>().Move(velocity, _physicsObjects);
        _playerLight.GetComponent<Transform>().Position = _player.GetComponent<Transform>().Position;
        
        _playerBLight.GetComponent<Transform>().Position = _playerB.GetComponent<Transform>().Position;
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // var collisionColor = _player.CollidesWith(_playerB)
        //     ? Color.Red
        //     : Color.Lime;

        // Lightmask (clear to subtraction color and carve lights)
        {
            GraphicsDevice.SetRenderTarget(_lightMaskTarget);
            GraphicsDevice.Clear(DayTimeManager.CurrentLighting);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendStates.LightCarveBlend, SamplerState.PointClamp);

            foreach (var lightSource in _lightSources)
                lightSource.Draw(_spriteBatch);

            _spriteBatch.End();
        }

        // Draw the real game
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                effect: AssetManager.Shader);

            foreach (var worldObject in _worldObjects)
                worldObject.Draw(_spriteBatch);

            _spriteBatch.End();
        }

        // Apply Stardew like blending
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendStates.LightingBlend);
            _spriteBatch.Draw(_lightMaskTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

#if DEBUG
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        foreach (var physicsObject in _physicsObjects)
            physicsObject.DrawDebug(_spriteBatch, Color.Lime);

        _spriteBatch.End();
#endif

        base.Draw(gameTime);
    }
}