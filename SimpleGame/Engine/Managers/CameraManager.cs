using System;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace SimpleGame.Engine.Managers;

public static class CameraManager
{
    private static Vector2 _cameraPosition;
    public static Vector2 CameraPosition => _cameraPosition;
    
    private static float _zoom = 2f;
    public static float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Clamp(value, 0.1f, 10f); // prevent crazy values
    }
    
    public static Matrix GetCameraMatrix(Vector2 screenCenter)
    {
        return
            Matrix.CreateTranslation(new Vector3(-_cameraPosition, 0f)) *
            Matrix.CreateScale(_zoom, _zoom, 1f) *
            Matrix.CreateTranslation(new Vector3(screenCenter + ShakeManager.ShakeOffset, 0f));
    }

    public static void SetCameraPosition(Vector2 position)
    {
        _cameraPosition = position;
    }
}