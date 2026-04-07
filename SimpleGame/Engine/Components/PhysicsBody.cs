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

    #region Non-SweptAABB
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

        Transform.Position.X += velocity.X;
        ResolveCollisions(Vector2.UnitX, others);

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
    #endregion
    
    #region Swept AABB
    public void SweptMove(Vector2 velocity, List<PhysicsBody> others)
    {
        _velocity = velocity;

        var remainingTime = 1f;

        for (int i = 0; i < 3; i++) // iterate for sliding
        {
            var nearestT = 1f;
            PhysicsBody hit = null;
            var hitNormal = Vector2.Zero;

            foreach (var other in others)
            {
                if (other == this) continue;

                if (SweptAABB(other, velocity * remainingTime, out var t, out var normal))
                {
                    if (t < nearestT)
                    {
                        nearestT = t;
                        hit = other;
                        hitNormal = normal;
                    }
                }
            }

            // Move up to collision
            Transform.Position += velocity * remainingTime * nearestT;

            if (hit == null)
                break;

            //// Push slightly out of the surface
            //Transform.Position += hitNormal * 0.001f;

            // Slide: remove velocity along normal
            velocity = velocity * remainingTime * (1 - nearestT);
            velocity -= Vector2.Dot(velocity, hitNormal) * hitNormal;

            remainingTime = 1f;
        }
    }

    private bool SweptAABB(
        PhysicsBody other,
        Vector2 velocity,
        out float t,
        out Vector2 normal)
    {
        t = 1f;
        normal = Vector2.Zero;

        var mySize = ColliderSize * Transform.Scale;
        var otherSize = other.ColliderSize * other.Transform.Scale;

        var myMin = Transform.Position - mySize / 2f + CollisionOffset * Transform.Scale;
        var myMax = myMin + mySize;

        var otherMin = other.Transform.Position - otherSize / 2f + other.CollisionOffset * other.Transform.Scale;
        var otherMax = otherMin + otherSize;

        Vector2 invEntry, invExit;

        if (velocity.X > 0.0f)
        {
            invEntry.X = otherMin.X - myMax.X;
            invExit.X = otherMax.X - myMin.X;
        }
        else
        {
            invEntry.X = otherMax.X - myMin.X;
            invExit.X = otherMin.X - myMax.X;
        }

        if (velocity.Y > 0.0f)
        {
            invEntry.Y = otherMin.Y - myMax.Y;
            invExit.Y = otherMax.Y - myMin.Y;
        }
        else
        {
            invEntry.Y = otherMax.Y - myMin.Y;
            invExit.Y = otherMin.Y - myMax.Y;
        }

        Vector2 entry, exit;

        // Check for static overlap when velocity is zero
        if (velocity.X == 0.0f)
        {
            // If we aren't moving on X, and we aren't currently overlapping on X, a collision is impossible.
            if (myMax.X <= otherMin.X || myMin.X >= otherMax.X) return false;

            entry.X = float.NegativeInfinity;
            exit.X = float.PositiveInfinity;
        }
        else
        {
            entry.X = invEntry.X / velocity.X;
            exit.X = invExit.X / velocity.X;
        }

        if (velocity.Y == 0.0f)
        {
            // If we aren't moving on Y, and we aren't currently overlapping on Y, a collision is impossible.
            if (myMax.Y <= otherMin.Y || myMin.Y >= otherMax.Y) return false;

            entry.Y = float.NegativeInfinity;
            exit.Y = float.PositiveInfinity;
        }
        else
        {
            entry.Y = invEntry.Y / velocity.Y;
            exit.Y = invExit.Y / velocity.Y;
        }

        var entryTime = Math.Max(entry.X, entry.Y);
        var exitTime = Math.Min(exit.X, exit.Y);

        if (entryTime > exitTime || entryTime < 0.0f || entryTime > 1.0f)
            return false;

        t = entryTime;

        // Use velocity direction instead of Math.Sign to avoid returning a Zero normal when perfectly flush
        if (entry.X > entry.Y)
            normal = new Vector2(velocity.X > 0 ? -1 : 1, 0);
        else
            normal = new Vector2(0, velocity.Y > 0 ? -1 : 1);

        return true;
    }
    #endregion
    
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

        // Velocity arrow (line + triangle)
        var dir = Vector2.Normalize(_velocity);
        
        const float velocityScale = 30f;

        var velocityEnd = center + dir * velocityScale;

        spriteBatch.DrawLine(center, velocityEnd, Color.Cyan, 2f);

        var perp = new Vector2(-dir.Y, dir.X);

        const float arrowLength = 12f;
        const float arrowWidth = 12f;

        var baseCenter = velocityEnd - dir * arrowLength;

        var left = baseCenter + perp * (arrowWidth * 0.5f);
        var right = baseCenter - perp * (arrowWidth * 0.5f);

        spriteBatch.FillTriangle(velocityEnd, left, right, Color.Cyan);
    }
}