using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public static class AssetManager
{
    private static ContentManager _content;
    
    public static Texture2D Player;
    public static Effect Shader;
    
    public static Texture2D LightGradientTexture;
    public static Texture2D GameWorldTexture;
    
    public static bool NeedsReload { get; private set; }

    private static T Load<T>(string path) where T : class
    {
        T content = null;
        
        try
        {
            content = _content.Load<T>(path);
            
            Console.WriteLine($"Loaded {typeof(T).Name} at: \"{path}\"");
        }
        catch (ContentLoadException e)
        {
            Console.WriteLine($"Failed to load {path}: {e.Message}");
        }
        
        return content;
    }

    public static void Load(ContentManager content)
    {
        _content = content;
        Player = Load<Texture2D>("Sprites/test");
        LightGradientTexture  = Load<Texture2D>("Sprites/LightGradient");
        GameWorldTexture = Load<Texture2D>("Sprites/GameWorld");
        Shader = Load<Effect>("Shaders/test");
    }
    
    public static void Unload() => _content.Unload();
    
#if DEBUG
    private static DateTime _lastBuildTime = DateTime.MinValue;

    public static void InitializeHotReload()
    {
        // The assets directory to be watched, this is working fine
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
        var assetsDir = Path.Combine(projectDir, "../SimpleGameContentBuilder/Assets");
        //var assetsDir = @"C:\Users\Gabriel\RiderProjects\SimpleGame\SimpleGameContentBuilder\Assets";

        var watcher = new FileSystemWatcher
        {
            Path = assetsDir,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "*.*", // Watch all raw files
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Changed += OnAssetChanged;
        watcher.Created += OnAssetChanged;
        watcher.Renamed += OnAssetChanged;
    }
    
private static void OnAssetChanged(object sender, FileSystemEventArgs args)
    {
        // Debounce (prevents spam builds)
        if ((DateTime.Now - _lastBuildTime).TotalMilliseconds < 1000)
            return;

        _lastBuildTime = DateTime.Now;

        Console.WriteLine($"[Hot-Reload] Change detected: {args.FullPath}");

        Task.Run(() =>
        {
            // For some reason if i try removing path.combine which points to the same directory it gets mad
            var outDir = Path.Combine(AppContext.BaseDirectory, "../net9.0");

            // Project base dir and temp dir
            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            var tempDir = Path.Combine(projectDir, "obj", "Content");

            // Builder project
            var builderProject = Path.Combine(projectDir, "../SimpleGameContentBuilder/SimpleGameContentBuilder.csproj");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{builderProject}\" -- build -p DesktopGL -s Assets -o \"{outDir}\" -i \"{tempDir}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(builderProject) // Safe practice for dotnet commands
            };

            using (var process = Process.Start(startInfo))
            {
                // Read the output streams
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                
                process?.WaitForExit();

                if (!string.IsNullOrWhiteSpace(output)) 
                    Console.WriteLine($"[Builder Output]: {output}");
                
                if (!string.IsNullOrWhiteSpace(error)) 
                    Console.WriteLine($"[Builder Error]: {error}");
            }

            Console.WriteLine("[Hot-Reload] Build finished. Flagging reload.");
            NeedsReload = true;
        });
    }

    public static void PerformReload()
    {
        //// Give the OS a tiny fraction of a second to release the file lock
        //System.Threading.Thread.Sleep(500); 
        
        Unload();     // Dump the old cached assets
        Load(_content); // Reload everything
        
        NeedsReload = false;
        Console.WriteLine("[Hot-Reload] Assets reloaded successfully.");
    }
#endif
}