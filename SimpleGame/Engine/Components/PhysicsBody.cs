using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class PhysicsBody : Component
{
    public Transform Transform;
    
    private Vector2 _velocity;
    public Vector2 Velocity => _velocity;
    
    public Vector2 ColliderSize;
    public Vector2 CollisionOffset;

    public bool CollidesWith(PhysicsBody other)
    {
        var myScaledSize = ColliderSize * Transform.Scale;
        var otherScaledSize = other.ColliderSize * other.Transform.Scale;
        
        var myMin = Transform.Position - myScaledSize / 2f + CollisionOffset * Transform.Scale;
        var myMax = myMin + myScaledSize;

        var otherMin = other.Transform.Position - otherScaledSize / 2f + other.CollisionOffset * other.Transform.Scale;
        var otherMax = otherMin + otherScaledSize;

        return myMin.X < otherMax.X &&
               myMax.X > otherMin.X &&
               myMin.Y < otherMax.Y &&
               myMax.Y > otherMin.Y;
    }
    
    public void Move(Vector2 velocity, List<PhysicsBody> others)
    {
        _velocity = velocity;

        // Move X
        Transform.Position.X += velocity.X;
        ResolveCollisions(Vector2.UnitX, others);

        // Move Y
        Transform.Position.Y += velocity.Y;
        ResolveCollisions(Vector2.UnitY, others);
    }

    private void ResolveCollisions(Vector2 axis, List<PhysicsBody> others)
    {
        if (others == null)
            return;
        
        foreach (var other in others)
        {
            if (other == this)
                continue;
    
            if (!CollidesWith(other))
                continue;
    
            var myScaledSize = ColliderSize * Transform.Scale;
            var otherScaledSize = other.ColliderSize * other.Transform.Scale;

            var myHalf = myScaledSize / 2f;
            var otherHalf = otherScaledSize / 2f;

            var myCenter = Transform.Position + CollisionOffset * Transform.Scale;
            var otherCenter = other.Transform.Position + other.CollisionOffset * other.Transform.Scale;
    
            var overlapX = (myHalf.X + otherHalf.X) - Math.Abs(myCenter.X - otherCenter.X);
            var overlapY = (myHalf.Y + otherHalf.Y) - Math.Abs(myCenter.Y - otherCenter.Y);
    
            if (axis.X != 0)
            {
                float direction = Math.Sign(myCenter.X - otherCenter.X);
                Transform.Position.X += overlapX * direction;
            }
            else if (axis.Y != 0)
            {
                float direction = Math.Sign(myCenter.Y - otherCenter.Y);
                Transform.Position.Y += overlapY * direction;
            }
        }
    }

    public void DrawDebug(SpriteBatch spriteBatch, Color color)
    {
        var scaledSize = ColliderSize * Transform.Scale;
        var scaledOffset = CollisionOffset * Transform.Scale;

        var topLeft = Transform.Position - scaledSize / 2f + scaledOffset;
        var center = Transform.Position + scaledOffset;

        // Filled collider (semi-transparent)
        spriteBatch.FillRectangle(topLeft, scaledSize, color * 0.4f);
        spriteBatch.DrawRectangle(topLeft, scaledSize, color, 2f);

        // Sprite origin
        spriteBatch.DrawCircle(Transform.Position, 3f, 12, Color.Yellow, 2f);

        // Collider Offset (origin -> collider center)
        spriteBatch.DrawLine(Transform.Position, center, Color.Orange, 2f);

        // Axis cross (helps visualize penetration)
        var axisSize = 10f;
        spriteBatch.DrawLine(center + new Vector2(-axisSize, 0), center + new Vector2(axisSize, 0), Color.Green, 2f);
        spriteBatch.DrawLine(center + new Vector2(0, -axisSize), center + new Vector2(0, axisSize), Color.Green, 2f);
        
        if (!(_velocity.LengthSquared() > 0.001f))
            return;
        
        // =========================
        // Velocity arrow (line + triangle)
        // =========================
        const float velocityScale = 10f;

        var velocityEnd = center + _velocity * velocityScale;

        spriteBatch.DrawLine(center, velocityEnd, Color.Cyan, 2f);

        var dir = Vector2.Normalize(_velocity);
        var perp = new Vector2(-dir.Y, dir.X);

        const float arrowLength = 12f;
        const float arrowWidth = 12f;

        var baseCenter = velocityEnd - dir * arrowLength;

        var left = baseCenter + perp * (arrowWidth * 0.5f);
        var right = baseCenter - perp * (arrowWidth * 0.5f);

        spriteBatch.FillTriangle(velocityEnd, left, right, Color.Cyan);
    }
}