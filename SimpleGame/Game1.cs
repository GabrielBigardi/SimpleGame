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

    private PhysicsSprite _player;
    private Sprite _playerLight;

    private PhysicsSprite _playerB;
    private Sprite _playerBLight;

    private Sprite _gameWorld;

    private Vector2 ScreenCenter =>
        new(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f);

    private readonly List<Sprite> _sprites = new();
    private readonly List<PhysicsSprite> _physicsSprite = new();
    private readonly List<LightSource> _lightSources = new();

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
        _gameWorld = new Sprite(AssetManager.GameWorldTexture, ScreenCenter, Vector2.One * 3f);

        _player = new PhysicsSprite(
            AssetManager.Player,
            ScreenCenter,
            Vector2.One, new Vector2(64, 120-64), new Vector2(0,32));

        _playerLight = new Sprite(
            AssetManager.LightGradientTexture,
            ScreenCenter,
            Vector2.One * 3f);

        _playerB = new PhysicsSprite(
            AssetManager.Player,
            ScreenCenter + new Vector2(300, 200),
            Vector2.One, new Vector2(64, 120-64), new Vector2(0,32));

        _playerBLight = new Sprite(
            AssetManager.LightGradientTexture,
            ScreenCenter + new Vector2(300, 200),
            Vector2.One * 3f);
        
        _sprites.Add(_gameWorld);
        _sprites.Add(_player);
        _sprites.Add(_playerB);
        
        _physicsSprite.Add(_player);
        _physicsSprite.Add(_playerB);

        _lightSources.Add(new LightSource()
        {
            Sprite = new Sprite(
                AssetManager.LightGradientTexture,
                new Vector2(870, 380),
                Vector2.One * 3f)
        });

        _lightSources.Add(new LightSource()
        {
            Sprite = new Sprite(
                AssetManager.LightGradientTexture,
                new Vector2(440, 190),
                Vector2.One * 3f)
        });

        _lightSources.Add(new LightSource() { Sprite = _playerLight });
        _lightSources.Add(new LightSource() { Sprite = _playerBLight });
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        TimeManager.Update(gameTime);
        InputManager.Update();
        DayTimeManager.Update(TimeManager.DeltaTime);

#if DEBUG
        if (AssetManager.NeedsReload)
        {
            AssetManager.PerformReload();
            _player.Texture = AssetManager.Player;
            _playerB.Texture = AssetManager.Player;

            foreach (var lightSource in _lightSources)
                lightSource.Sprite.Texture = AssetManager.LightGradientTexture;
        }
#endif

        //_player.Position += InputManager.NormalizedInput * 200f * TimeManager.DeltaTime;
        var velocity = InputManager.NormalizedInput * 200f * TimeManager.DeltaTime;

        if (velocity.X > 0)
            _player.FlipX = false;
        
        if (velocity.X < 0)
            _player.FlipX = true;

        _player.Move(velocity, _physicsSprite);
        
        
        _playerLight.Position = _player.Position;
        
        var horizontal = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Right)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Left));
        var vertical = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Down)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Up));
        var movement = new Vector2(horizontal, vertical);
        
        if (movement != Vector2.Zero)
            movement.Normalize();
        
        if (movement.X > 0)
            _playerB.FlipX = false;
        
        if (movement.X < 0)
            _playerB.FlipX = true;

        var playerBVelocity = movement * 200f * TimeManager.DeltaTime;
        _playerB.Move(playerBVelocity, _physicsSprite);
        
        _playerBLight.Position = _playerB.Position;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var collisionColor = _player.CollidesWith(_playerB)
            ? Color.Red
            : Color.Lime;

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

            foreach (var sprite in _sprites)
                sprite.Draw(_spriteBatch);

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

        foreach (var physicsSprite in _physicsSprite)
            physicsSprite.DrawDebug(_spriteBatch, collisionColor);
        
        _spriteBatch.End();
#endif

        base.Draw(gameTime);
    }
}