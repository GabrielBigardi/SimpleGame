using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class PhysicsSprite : Sprite
{
    public Vector2 ColliderSize;
    //public Vector2 CollisionOffset;

    public PhysicsSprite(Texture2D texture, Vector2 position, Vector2 scale, Vector2 colliderSize, float rotation = 0) :
        base(texture, position, scale, rotation)
    {
        ColliderSize = colliderSize;
    }

    public bool CollidesWith(PhysicsSprite other)
    {
        // (other.X < X + Width) && (X < other.X + other.Width) &&
        //     (other.Y < Y + Height) && (Y < other.Y + other.Height);
        
        return other.Position.X < Position.X + ColliderSize.X &&
               Position.X < other.Position.X + other.ColliderSize.X &&
               other.Position.Y < Position.Y + ColliderSize.Y &&
               Position.Y < other.Position.Y + other.ColliderSize.Y;
               
    }
}