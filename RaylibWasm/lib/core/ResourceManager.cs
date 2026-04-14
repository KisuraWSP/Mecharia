using System.Collections.Generic;
using Raylib_cs;

namespace lib.core;

public static class ResourceManager
{
    private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    /// <summary>
    /// Requests a texture. If it's not loaded, it loads it from the disk. 
    /// If it is loaded, it returns the cached version.
    /// </summary>
    public static Texture2D GetTexture(string filepath)
    {
        if (_textures.TryGetValue(filepath, out Texture2D existingTexture))
        {
            return existingTexture; 
        }

        Texture2D newTexture = Raylib.LoadTexture(filepath);
        
        _textures.Add(filepath, newTexture);
        
        return newTexture;
    }

    /// <summary>
    /// Cleans up all VRAM memory. Call this exactly once when the game closes.
    /// </summary>
    public static void UnloadAllResources()
    {
        foreach (var texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }
        
        _textures.Clear();
    }
}