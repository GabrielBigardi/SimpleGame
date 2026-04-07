using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public abstract class Component
{
    public GameObject GameObject { get; internal set; }
    public bool Enabled { get; set; }
    
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
}