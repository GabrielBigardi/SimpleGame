using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SimpleGameContentBuilder.Builder;

public static class TexturePacker
{
    public static void Run(string assetsDir, string atlasOutputDir, string jsonOutputDir)
    {
        var files = Directory.GetFiles(assetsDir, "*.png", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToList();

        if (files.Count == 0)
            return;

        var images = files.Select(f => (Path: f, Image: Image.Load<Rgba32>(f))).ToList();

        int padding = 2;
        int x = 0, y = 0, rowHeight = 0;
        int maxWidth = 1024;

        var atlasWidth = 0;
        var atlasHeight = 0;

        var regions = new Dictionary<string, Rectangle>();

        foreach (var img in images)
        {
            if (x + img.Image.Width > maxWidth)
            {
                x = 0;
                y += rowHeight + padding;
                rowHeight = 0;
            }

            regions[Path.GetFileNameWithoutExtension(img.Path)] =
                new Rectangle(x, y, img.Image.Width, img.Image.Height);

            x += img.Image.Width + padding;
            rowHeight = Math.Max(rowHeight, img.Image.Height);

            atlasWidth = Math.Max(atlasWidth, x);
            atlasHeight = Math.Max(atlasHeight, y + rowHeight);
        }

        using var atlas = new Image<Rgba32>(atlasWidth, atlasHeight);

        foreach (var img in images)
        {
            var key = Path.GetFileNameWithoutExtension(img.Path);
            var rect = regions[key];

            atlas.Mutate(ctx => ctx.DrawImage(img.Image, new Point(rect.X, rect.Y), 1f));
        }

        // Save atlas
        var atlasPath = Path.Combine(atlasOutputDir, "atlas.png");
        atlas.Save(atlasPath);

        // Save metadata
        var jsonPath = Path.Combine(jsonOutputDir, "atlas.json");
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(regions, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}