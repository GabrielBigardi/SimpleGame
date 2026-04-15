/// <summary>
/// Entry point for the Content Builder project, 
/// which when executed will build content according to the "Content Collection Strategy" defined in the Builder class.
/// </summary>
/// <remarks>
/// Make sure to validate the directory paths in the "ContentBuilderParams" for your specific project.
/// For more details regarding the Content Builder, see the MonoGame documentation: <tbc.>
/// </remarks>

using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;
using SimpleGameContentBuilder.Builder;

#if DEBUG
    var debugContentBuilderParams = new ContentBuilderParams()
    {
        Mode = ContentBuilderMode.Builder,
        WorkingDirectory = $"{AppContext.BaseDirectory}../../../", // path to where your content folder can be located
        SourceDirectory = "Assets", // Not actually needed as this is the default, but added for reference
        OutputDirectory = $"{AppContext.BaseDirectory}../../../../SimpleGame/bin/Debug/net9.0/win-x64/",
        Platform = TargetPlatform.DesktopGL
    };
#else
    var defaultReleaseContentBuilderParams = new ContentBuilderParams()
    {
        Mode = ContentBuilderMode.Builder,
        WorkingDirectory = $"{AppContext.BaseDirectory}../../../", // path to where your content folder can be located
        SourceDirectory = "Assets", // Not actually needed as this is the default, but added for reference
        OutputDirectory = $"{AppContext.BaseDirectory}../../../../SimpleGame/bin/Release/net9.0/win-x64/",
        Platform = TargetPlatform.DesktopGL
    };

    var aotReleaseContentBuilderParams = new ContentBuilderParams()
    {
        Mode = ContentBuilderMode.Builder,
        WorkingDirectory = $"{AppContext.BaseDirectory}../../../", // path to where your content folder can be located
        SourceDirectory = "Assets", // Not actually needed as this is the default, but added for reference
        OutputDirectory = $"{AppContext.BaseDirectory}../../../../SimpleGame/bin/Release/net9.0/win-x64/publish/",
        Platform = TargetPlatform.DesktopGL
    };
#endif

var builder = new Builder();

if (args is not null && args.Length > 0)
{
    builder.Run(args);
}
else
{
#if DEBUG
    builder.Run(debugContentBuilderParams);
#else
    builder.Run(defaultReleaseContentBuilderParams);
    builder.Run(aotReleaseContentBuilderParams);
#endif
}

return builder.FailedToBuild > 0 ? -1 : 0;

public class Builder : ContentBuilder
{
    private string _workingDirectory = $"{AppContext.BaseDirectory}../../../";
    
    public override IContentCollection GetContentCollection()
    {
        RunTexturePacker();

        var contentCollection = new ContentCollection();

        contentCollection.Include<WildcardRule>("*");
        contentCollection.Exclude<WildcardRule>("Sprites/*");

        return contentCollection;
    }
    
    private void RunTexturePacker()
    {
        var assetsDir = Path.Combine(_workingDirectory, "Assets", "Sprites");
        var atlasOutputDir = Path.Combine(_workingDirectory, "Assets", "Generated");
        
#if DEBUG
        var jsonOutputDir = $"{AppContext.BaseDirectory}../../../../SimpleGame/bin/Debug/net9.0/win-x64/Content/Generated/";
#else
        var jsonOutputDir =
            $"{AppContext.BaseDirectory}../../../../SimpleGame/bin/Release/net9.0/win-x64/Content/Generated/";
#endif

        // Create directories if needed
        Directory.CreateDirectory(atlasOutputDir);
        Directory.CreateDirectory(jsonOutputDir);

        TexturePacker.Run(assetsDir, atlasOutputDir, jsonOutputDir);
    }
}