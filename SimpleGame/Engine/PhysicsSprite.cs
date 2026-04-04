using System;
using System.Collections.Generic;
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
        var myMin = Position - _colliderSize / 2f + _collisionOffset;
        var myMax = myMin + _colliderSize;

        var otherMin = other.Position - other._colliderSize / 2f + other._collisionOffset;
        var otherMax = otherMin + other._colliderSize;

        return myMin.X < otherMax.X &&
               myMax.X > otherMin.X &&
               myMin.Y < otherMax.Y &&
               myMax.Y > otherMin.Y;
    }
    
    public void Move(Vector2 velocity, List<PhysicsSprite> others)
    {
        // Move X
        Position.X += velocity.X;
        ResolveCollisions(Vector2.UnitX, others);

        // Move Y
        Position.Y += velocity.Y;
        ResolveCollisions(Vector2.UnitY, others);
    }
    
    private void ResolveCollisions(Vector2 axis, List<PhysicsSprite> others)
    {
        foreach (var other in others)
        {
            if (other == this)
                continue;

            if (!CollidesWith(other))
                continue;

            var myCenter = Position + _collisionOffset;
            var otherCenter = other.Position + other._collisionOffset;

            var myHalf = _colliderSize / 2f;
            var otherHalf = other._colliderSize / 2f;

            var overlapX = (myHalf.X + otherHalf.X) - Math.Abs(myCenter.X - otherCenter.X);
            var overlapY = (myHalf.Y + otherHalf.Y) - Math.Abs(myCenter.Y - otherCenter.Y);

            if (axis.X != 0)
            {
                float direction = Math.Sign(myCenter.X - otherCenter.X);
                Position.X += overlapX * direction;
            }
            else if (axis.Y != 0)
            {
                float direction = Math.Sign(myCenter.Y - otherCenter.Y);
                Position.Y += overlapY * direction;
            }
        }
    }
    
    public void DrawDebug(SpriteBatch spriteBatch, Color color)
    {
        spriteBatch.DrawRectangle(Position - _colliderSize / 2f + _collisionOffset, _colliderSize, color, 2f);
    }
}