using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleGame.Engine;

namespace SimpleGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Sprite _player;
    private Sprite _lightA;
    private Sprite _lightB;

    private RenderTarget2D _lightMaskTarget;

    private readonly BlendState _lightCarveBlend = new()
    {
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        ColorSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.One
    };

    private readonly BlendState _lightingBlend = new()
    {
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        ColorSourceBlend = Blend.SourceColor,
        ColorDestinationBlend = Blend.One
    };

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

        var pp = GraphicsDevice.PresentationParameters;
        _lightMaskTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);

        AssetManager.Load(Content);

        _player = new Sprite(
            AssetManager.Player,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f,  _graphics.PreferredBackBufferHeight / 2f),
            Vector2.One);

        _lightA = new Sprite(
            AssetManager.LightGradientTexture,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f),
            Vector2.One * 2f);
        
        _lightB = new Sprite(
            AssetManager.LightGradientTexture,
            new Vector2(_graphics.PreferredBackBufferWidth / 2f + 300f, _graphics.PreferredBackBufferHeight / 2f + 300f),
            Vector2.One);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        var horizontal = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A));
        var vertical =  Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W));
        var movement = new Vector2(horizontal, vertical);

        if (movement != Vector2.Zero)
            movement.Normalize();
        
        _player.Position += movement * 200f * (float)gameTime.ElapsedGameTime.TotalSeconds;

        _lightA.Position = _player.Position;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Draw subtraction color to light mask
        {
            GraphicsDevice.SetRenderTarget(_lightMaskTarget);

            var ambientSubtractionColor = new Color(180, 180, 0);
            GraphicsDevice.Clear(ambientSubtractionColor);
        }

        // Carve lights from light mask
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, _lightCarveBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(_lightA.Texture, _lightA.Position, null, Color.White, _lightA.Rotation, _lightA.CenterOrigin, _lightA.Scale, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_lightB.Texture, _lightB.Position, null, Color.Orange, _lightB.Rotation, _lightB.CenterOrigin, _lightB.Scale, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }

        // Draw the real game
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            _spriteBatch.Draw(AssetManager.GameWorldTexture, Vector2.Zero,
                new Rectangle(0, 0, AssetManager.GameWorldTexture.Width, AssetManager.GameWorldTexture.Height), Color.White,
                0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            
            _spriteBatch.Draw(_player.Texture, _player.Position, null, Color.White, _player.Rotation, _player.CenterOrigin, _player.Scale, SpriteEffects.None, 0f);
            
            _spriteBatch.End();
        }

        // Apply Stardew like blending
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, _lightingBlend);
            _spriteBatch.Draw(_lightMaskTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }
}