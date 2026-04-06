using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class GameObject
{
    private List<Component> _components = new();

    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        component.GameObject = this;

        _components.Add(component);
        component.Start();

        return component;
    }

    public T GetComponent<T>() where T : Component
    {
        return _components.OfType<T>().FirstOrDefault();
    }

    public void Update(float deltaTime)
    {
        foreach (var component in _components)
            component.Update(deltaTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var component in _components)
            component.Draw(spriteBatch);
    }
}