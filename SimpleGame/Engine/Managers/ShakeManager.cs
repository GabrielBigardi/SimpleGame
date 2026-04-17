using System;
using System.Numerics;
using SimpleGame.Engine.Utils;

namespace SimpleGame.Engine.Managers;

public static class ShakeManager
{
    private static float _shakeTime;
    private static float _shakeDuration;
    private static float _shakeStrength;
    private static Vector2 _shakeOffset;
    public static Vector2 ShakeOffset => _shakeOffset;
    
    public static void Update(Random random)
    {
        if (_shakeTime > 0f)
        {
            _shakeTime -= TimeManager.DeltaTime;

            var progress = _shakeTime / _shakeDuration; // fades out

            _shakeOffset = new Vector2(
                random.NextFloat(-1f, 1f),
                random.NextFloat(-1f, 1f)
            ) * _shakeStrength * progress;
        }
        else
        {
            _shakeOffset = Vector2.Zero;
        }
    }
    
    public static void Shake(float duration, float strength)
    {
        _shakeDuration = duration;
        _shakeTime = duration;
        _shakeStrength = strength;
    }
}