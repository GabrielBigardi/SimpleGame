using Microsoft.Xna.Framework;

namespace SimpleGame.Engine;

public static class DayTimeManager
{
    private static readonly Color FullBrightColor = Color.White;
    private static readonly Color FullDarkColor = new(20, 20, 60);

    public static Color CurrentLighting = Color.White;

    private static float _currentTime;
    private const float CYCLE_SPEED = 0.05f;

    public static void Update(float deltaTime)
    {
        _currentTime += deltaTime * CYCLE_SPEED;
        _currentTime %= 2f;

        // Creates a smooth 0 -> 1 -> 0 cycle
        var t = _currentTime <= 1f
            ? _currentTime
            : 2f - _currentTime;

        // Blend between dark and bright
        //CurrentLighting = Color.Lerp(FullDarkColor, FullBrightColor, t);
        CurrentLighting = FullDarkColor;
    }
}