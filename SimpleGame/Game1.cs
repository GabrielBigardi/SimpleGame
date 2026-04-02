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
        _lightMaskTarget = new RenderTarget2D(GraphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);

        AssetManager.Load(Content);
#if DEBUG
        AssetManager.InitializeHotReload();
#endif

        _player = new PhysicsSprite(
            AssetManager.Player,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f),
            Vector2.One, new Vector2(64, 120));
        
        _playerLight = new Sprite(
            AssetManager.LightGradientTexture,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f),
            Vector2.One * 3f);
        
        _playerB = new PhysicsSprite(
            AssetManager.Player,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f + 300,  _graphics.PreferredBackBufferHeight / 2f + 200),
            Vector2.One, new Vector2(64, 120));
        
        _playerBLight = new Sprite(
            AssetManager.LightGradientTexture,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f + 300, _graphics.PreferredBackBufferHeight / 2f + 200),
            Vector2.One * 3f);
        
        _lightSources.Add(new LightSource() { Sprite = new Sprite(
            AssetManager.LightGradientTexture,
            new Vector2(870, 380),
            Vector2.One * 3f)});
        
        _lightSources.Add(new LightSource() { Sprite = new Sprite(
            AssetManager.LightGradientTexture,
            new Vector2(440, 190),
            Vector2.One * 3f)});
        
        _lightSources.Add(new LightSource() { Sprite = _playerLight});
        _lightSources.Add(new LightSource() { Sprite = _playerBLight});
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
        
        _player.Position += InputManager.NormalizedInput * 200f * TimeManager.DeltaTime;
        _playerLight.Position = _player.Position;
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

            AssetManager.Shader.Parameters["TexelSize"].SetValue(new Vector2(1f / AssetManager.Player.Width, 1f / AssetManager.Player.Height));
            AssetManager.Shader.Parameters["IncludeCorners"].SetValue(0f);
            AssetManager.Shader.Parameters["OutlineColor"].SetValue(collisionColor.ToVector4());

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, effect: AssetManager.Shader);
            
            _spriteBatch.Draw(AssetManager.GameWorldTexture, Vector2.Zero, new Rectangle(0, 0, AssetManager.GameWorldTexture.Width, AssetManager.GameWorldTexture.Height), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_player.Texture, _player.Position, null, Color.White, _player.Rotation, _player.CenterOrigin, _player.Scale, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_playerB.Texture, _playerB.Position, null, Color.White, _playerB.Rotation, _playerB.CenterOrigin, _playerB.Scale, SpriteEffects.None, 0f);

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

        _spriteBatch.DrawRectangle(_player.Position - _player.ColliderSize / 2f, _player.ColliderSize, collisionColor, 2f);
        _spriteBatch.DrawRectangle(_playerB.Position - _player.ColliderSize / 2f, _playerB.ColliderSize, collisionColor, 2f);

        _spriteBatch.End();
#endif

        base.Draw(gameTime);
    }
}