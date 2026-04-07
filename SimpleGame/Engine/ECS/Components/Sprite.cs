using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.ECS.Components;

public record struct Sprite(Texture2D Texture, Vector2 Scale);