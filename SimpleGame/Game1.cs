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
    
    // Lighting Test
    private RenderTarget2D _lightMaskTarget;
    private BlendState _multiplyBlend;
    
    
    // 1. For Pass 1: Drawing lights onto the Lightmap
    // We want to subtract the light sprite's color from the ambient background
    protected readonly BlendState lightCarveBlend = new BlendState
    {
        ColorBlendFunction = BlendFunction.ReverseSubtract, // Dest (Ambient) - Source (Light)
        ColorSourceBlend = Blend.SourceAlpha, 
        ColorDestinationBlend = Blend.One
    };

    // 2. For Pass 3: Drawing the Lightmap over the Screen (The Stardew Code)
    protected readonly BlendState lightingBlend = new BlendState
    {
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        ColorDestinationBlend = Blend.One,
        ColorSourceBlend = Blend.SourceColor
    };
    
    

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Change the resolution to 720p
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges(); // Apply the changes
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // 1. Create the Render Target (usually the size of your viewport)
        var pp = GraphicsDevice.PresentationParameters;
        _lightMaskTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
        
        // 2. Define the Multiplicative Blend State
        // This tells the GPU: Final Color = Source Color * Destination Color
        _multiplyBlend = new BlendState
        {
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            ColorDestinationBlend = Blend.One,
            ColorSourceBlend = Blend.SourceColor
        };
        
        // Good but not identical to Stardew's lighting
        //_multiplyBlend = new BlendState
        //{
        //    ColorBlendFunction = BlendFunction.Add,
        //    ColorSourceBlend = Blend.DestinationColor,
        //    ColorDestinationBlend = Blend.Zero
        //};

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
        // ==========================================
        // PASS 1: Generate the Lightmap (The Darkness Map)
        // ==========================================
        GraphicsDevice.SetRenderTarget(_lightMaskTarget);
    
        // 1. Clear to the color you want to SUBTRACT from the world.
        // To get a blue night, clear with a warm color (e.g., Yellow/Orange)
        // You'll need to tweak this color to get the exact night vibe you want.
        Color ambientSubtractionColor = new Color(179, 180, 8); 
        GraphicsDevice.Clear(ambientSubtractionColor);

        // 2. Draw your lights using the CARVE blend.
        // This subtracts the white gradient sprite from the ambient color, 
        // pushing the pixels toward Black (0,0,0) which means "fully lit".
        _spriteBatch.Begin(SpriteSortMode.Deferred, lightCarveBlend, SamplerState.PointClamp);
        
        Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f );
        Vector2 origin = new Vector2(AssetManager.LightGradientTexture.Width / 2f, AssetManager.LightGradientTexture.Height / 2f);
    
        // Draw your light sources.
        _spriteBatch.Draw(AssetManager.LightGradientTexture, screenCenter, null, Color.White, 0f, origin, 2f, SpriteEffects.None, 0f);
        _spriteBatch.Draw(AssetManager.LightGradientTexture, screenCenter + Vector2.One * 300f, null, Color.Orange, 0f, origin, 1f, SpriteEffects.None, 0f);
        _spriteBatch.End();

        // ==========================================
        // PASS 2: Draw the Base Game World
        // ==========================================
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        _spriteBatch.Draw(AssetManager.GameWorldTexture, Vector2.Zero, new Rectangle(0,0, AssetManager.GameWorldTexture.Width, AssetManager.GameWorldTexture.Height), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        _spriteBatch.Draw(_player.Texture, _player.Position, Color.White);
        _spriteBatch.End();

        // ==========================================
        // PASS 3: Apply the Stardew Blend
        // ==========================================
        _spriteBatch.Begin(SpriteSortMode.Deferred, lightingBlend);
        _spriteBatch.Draw(_lightMaskTarget, Vector2.Zero, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}