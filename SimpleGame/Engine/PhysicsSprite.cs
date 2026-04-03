using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class PhysicsSprite : Sprite
{
    public Vector2 ColliderSize;
    public Vector2 CollisionOffset;

    // Added collisionOffset with a default value so existing code won't break
    public PhysicsSprite(Texture2D texture, Vector2 position, Vector2 scale, Vector2 colliderSize, Vector2 collisionOffset = default, float rotation = 0) :
        base(texture, position, scale, rotation)
    {
        ColliderSize = colliderSize;
        CollisionOffset = collisionOffset;
    }

    public bool CollidesWith(PhysicsSprite other)
    {
        // Calculate the actual top-left positions of both colliders taking the offset into account
        var myColliderPos = Position + CollisionOffset;
        var otherColliderPos = other.Position + other.CollisionOffset;

        // Perform the AABB collision check using the adjusted offset positions
        return otherColliderPos.X < myColliderPos.X + ColliderSize.X &&
               myColliderPos.X < otherColliderPos.X + other.ColliderSize.X &&
               otherColliderPos.Y < myColliderPos.Y + ColliderSize.Y &&
               myColliderPos.Y < otherColliderPos.Y + other.ColliderSize.Y;
    }

    public void DrawDebug(SpriteBatch spriteBatch, Color color)
    {
        spriteBatch.DrawRectangle(Position - ColliderSize / 2f + CollisionOffset, ColliderSize, color, 2f);
    }
}