using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class PhysicsSprite : Sprite
{
    private Vector2 _velocity;
    public Vector2 Velocity => _velocity;
    
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
        var myScaledSize = _colliderSize * Scale;
        var otherScaledSize = other._colliderSize * other.Scale;
        
        var myMin = Position - myScaledSize / 2f + _collisionOffset * Scale;
        var myMax = myMin + myScaledSize;

        var otherMin = other.Position - otherScaledSize / 2f + other._collisionOffset * other.Scale;
        var otherMax = otherMin + otherScaledSize;

        return myMin.X < otherMax.X &&
               myMax.X > otherMin.X &&
               myMin.Y < otherMax.Y &&
               myMax.Y > otherMin.Y;
    }
    
    public void Move(Vector2 velocity, List<PhysicsSprite> others)
    {
        _velocity = velocity;

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
    
            var myScaledSize = _colliderSize * Scale;
            var otherScaledSize = other._colliderSize * other.Scale;

            var myHalf = myScaledSize / 2f;
            var otherHalf = otherScaledSize / 2f;

            var myCenter = Position + _collisionOffset * Scale;
            var otherCenter = other.Position + other._collisionOffset * other.Scale;
    
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
        var scaledSize = _colliderSize * Scale;
        var scaledOffset = _collisionOffset * Scale;

        var topLeft = Position - scaledSize / 2f + scaledOffset;
        var center = Position + scaledOffset;

        // Filled collider (semi-transparent)
        spriteBatch.FillRectangle(topLeft, scaledSize, color * 0.4f);
        spriteBatch.DrawRectangle(topLeft, scaledSize, color, 2f);

        // Sprite origin
        spriteBatch.DrawCircle(Position, 3f, 12, Color.Yellow, 2f);

        // Collider Offset (origin -> collider center)
        spriteBatch.DrawLine(Position, center, Color.Orange, 2f);

        // Axis cross (helps visualize penetration)
        var axisSize = 10f;
        spriteBatch.DrawLine(center + new Vector2(-axisSize, 0), center + new Vector2(axisSize, 0), Color.Green, 2f);
        spriteBatch.DrawLine(center + new Vector2(0, -axisSize), center + new Vector2(0, axisSize), Color.Green, 2f);
        
        // =========================
        // Velocity arrow (line + triangle)
        // =========================
        const float velocityScale = 16f;

        var velocityEnd = center + _velocity * velocityScale;

        spriteBatch.DrawLine(center, velocityEnd, Color.Cyan, 2f);

        if (!(_velocity.LengthSquared() > 0.001f))
            return;
        
        var dir = Vector2.Normalize(_velocity);
        var perp = new Vector2(-dir.Y, dir.X);

        const float arrowLength = 16f;
        const float arrowWidth = 16f;

        var baseCenter = velocityEnd - dir * arrowLength;

        var left = baseCenter + perp * (arrowWidth * 0.5f);
        var right = baseCenter - perp * (arrowWidth * 0.5f);

        spriteBatch.FillTriangle(velocityEnd, left, right, Color.Cyan);
    }
}