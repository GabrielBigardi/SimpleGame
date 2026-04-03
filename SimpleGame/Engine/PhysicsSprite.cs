using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class PhysicsSprite : Sprite
{
    private readonly Vector2 _colliderSize;
    private readonly Vector2 _collisionOffset;

    public PhysicsSprite(Texture2D texture, Vector2 position, Vector2 scale, Vector2 colliderSize, Vector2 collisionOffset = default, float rotation = 0) :
        base(texture, position, scale, rotation)
    {
        _colliderSize = colliderSize;
        _collisionOffset = collisionOffset;
    }

    public bool CollidesWith(PhysicsSprite other)
    {
        var myColliderPos = Position + _collisionOffset;
        var otherColliderPos = other.Position + other._collisionOffset;

        return otherColliderPos.X < myColliderPos.X + _colliderSize.X &&
               myColliderPos.X < otherColliderPos.X + other._colliderSize.X &&
               otherColliderPos.Y < myColliderPos.Y + _colliderSize.Y &&
               myColliderPos.Y < otherColliderPos.Y + other._colliderSize.Y;
    }

    public void DrawDebug(SpriteBatch spriteBatch, Color color)
    {
        spriteBatch.DrawRectangle(Position - _colliderSize / 2f + _collisionOffset, _colliderSize, color, 2f);
    }
}