using System.IO;
using Newtonsoft.Json;

namespace SimpleGame.Engine.Atlas;

public static class AtlasUtils
{
    public static AtlasData LoadAtlas(string path)
    {
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<AtlasData>(json);
    }
}