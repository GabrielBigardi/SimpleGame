using System;
using Microsoft.Xna.Framework;

namespace SimpleGame.Engine.Managers;

public static class CameraManager
{
    private static Vector3 _cameraPosition = new Vector3(0, 100, 200); // Default slightly elevated and pulled back
    public static Vector3 CameraPosition => _cameraPosition;
    
    private static Vector3 _cameraTarget = Vector3.Zero;
    public static Vector3 CameraTarget => _cameraTarget;
    
    public static Matrix ViewMatrix { get; private set; }
    public static Matrix ProjectionMatrix { get; private set; }
    
    public static void Initialize(GraphicsDeviceManager graphics)
    {
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight,
            1f,
            10000f);
        
        UpdateViewMatrix();
    }
    
    public static void SetCameraPosition(Vector3 position)
    {
        _cameraPosition = position;
        UpdateViewMatrix();
    }
    
    public static void SetCameraTarget(Vector3 target)
    {
        _cameraTarget = target;
        UpdateViewMatrix();
    }
    
    private static void UpdateViewMatrix()
    {
        // Incorporate shake if needed (ShakeManager uses Vector2, might need update later, ignoring for now)
        var shake3D = new Vector3(ShakeManager.ShakeOffset.X, ShakeManager.ShakeOffset.Y, 0);
        ViewMatrix = Matrix.CreateLookAt(_cameraPosition + shake3D, _cameraTarget, Vector3.Up);
    }
}