using Microsoft.Xna.Framework;

namespace SimpleGame.Engine;

public static class DayTimeManager
{
    public static Color CurrentLighting = Color.Black;
    
    private static float _currentTime;

    public static void Update(float deltaTime)
    {
        _currentTime += deltaTime * 0.05f;
        _currentTime %= 2f;
        var baseIntensity = _currentTime <= 1f ? _currentTime : 2f - _currentTime;
        var maxLighting = 0.8625f;
        var finalIntensity = baseIntensity * maxLighting;
        //CurrentLighting = new Color(finalIntensity, finalIntensity, 0f, 255);
        
        CurrentLighting = new Color(225, 225, 0, 255);
    }
}