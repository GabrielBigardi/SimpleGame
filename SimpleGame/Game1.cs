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

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        AssetManager.Load(Content);

#if DEBUG
        // Start watching the output content directory
        AssetManager.InitializeHotReload();
#endif
        
        _player = new Sprite()
        {
            Position = new Vector2(_graphics.PreferredBackBufferWidth/2f, _graphics.PreferredBackBufferHeight/2f), Texture = AssetManager.Player, Rotation = 0f, Scale = new Vector2(1f)
        };
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

#if DEBUG
        // Check if the watcher flagged a change
        if (AssetManager.NeedsReload)
        {
            AssetManager.PerformReload();
            
            // Re-assign the reloaded texture to your active sprite!
            _player.Texture = AssetManager.Player; 
        }
#endif
        
        // TODO: Add your update logic here
        var horizontal = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Right)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Left));
        var vertical =  Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Down)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Up));
        var movement = new Vector2(horizontal, vertical);

        if (movement != Vector2.Zero)
            movement.Normalize();
        
        _player.Position += movement * 200f * (float)gameTime.ElapsedGameTime.TotalSeconds;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: AssetManager.Shader);
        
        AssetManager.Shader.Parameters["TexelSize"].SetValue(new Vector2(1f / _player.Texture.Width, 1f / _player.Texture.Height));
        AssetManager.Shader.Parameters["OutlineColor"].SetValue(Color.Red.ToVector4());
        AssetManager.Shader.Parameters["IncludeCorners"].SetValue(0f);
        
        // TODO: Add your drawing code here
        _spriteBatch.Draw(_player.Texture, _player.Position, new Rectangle(0, 0, _player.Texture.Width, _player.Texture.Height), Color.White, _player.Rotation,
            new Vector2(_player.Texture.Width * 0.5f, _player.Texture.Height * 0.5f), _player.Scale, SpriteEffects.None, 0f);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}