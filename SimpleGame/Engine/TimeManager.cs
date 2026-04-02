using Microsoft.Xna.Framework;

namespace SimpleGame.Engine;

public static class TimeManager
{
    public static float DeltaTime;
    
    public static void Update(GameTime gameTime)
    {
        DeltaTime  = (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}