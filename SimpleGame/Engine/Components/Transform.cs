using Microsoft.Xna.Framework;

namespace SimpleGame.Engine;

public class Transform
{
    public GameObject GameObject;
    
    public Vector2 Position;
    public Vector2 Scale = Vector2.One;
    public float Rotation;
    
    public Transform(GameObject gameObject)
    {
        GameObject = gameObject;
    }
}