using System.Drawing;
using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.ECS.Components;
using SimpleGame.Engine.ECS.Systems.InlineQueries;

namespace SimpleGame.Engine.ECS.Systems;

public class LightDrawSystem
{
    private World _world;
    private QueryDescription _query;
    private LightDrawUpdate _lightDrawUpdate;
    
    public LightDrawSystem(World world, SpriteBatch spriteBatch)
    {
        _world = world;
        _query = new QueryDescription().WithAll<Position, LightSource, Visible>().WithNone<Destroy>();
        _lightDrawUpdate = new LightDrawUpdate(spriteBatch);
    }

    public void Update()
    {
        _world.InlineQuery<LightDrawUpdate, Position, LightSource, Visible>(in _query, ref _lightDrawUpdate);
    }
}