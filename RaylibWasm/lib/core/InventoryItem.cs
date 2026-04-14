using lib.core;
using Raylib_cs;

namespace lib.core;

public class InventoryItem
{
    public string Name { get; set; }
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public Texture2D Sprite { get; private set; }
    public bool IsRotated { get; private set; } = false;

    public int GridX { get; set; }
    public int GridY { get; set; }

    public int OriginalX { get; set; }
    public int OriginalY { get; set; }
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }

    public InventoryItem(string name, int w, int h, string texturePath)
    {
        Name = name;
        GridWidth = w;
        GridHeight = h;
        Sprite = ResourceManager.GetTexture(texturePath);
    }

    public void Rotate()
    {
        int temp = GridWidth;
        GridWidth = GridHeight;
        GridHeight = temp;
        IsRotated = !IsRotated;
    }
}