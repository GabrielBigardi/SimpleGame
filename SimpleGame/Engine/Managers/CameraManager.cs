using Microsoft.Xna.Framework;

namespace SimpleGame.Engine.Managers;

public static class CameraManager
{
    private static Vector2 _cameraPosition;
    public static Vector2 CameraPosition => _cameraPosition;
    
    public static Matrix GetCameraMatrix()
    {
        return Matrix.CreateTranslation(new Vector3(
            -_cameraPosition.X + Game1.ScreenCenter.X + ShakeManager.ShakeOffset.X,
            -_cameraPosition.Y + Game1.ScreenCenter.Y + ShakeManager.ShakeOffset.Y,
            0f));
    }

    public static void SetCameraPosition(Vector2 position)
    {
        _cameraPosition = position;
    }
}