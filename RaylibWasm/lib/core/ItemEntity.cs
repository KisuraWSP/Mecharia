using Raylib_cs;
using System.Numerics;

namespace lib.core;

public class ItemEntity
{
    public string Name { get; set; }
    public int GridWidth { get; set; }  
    public int GridHeight { get; set; } 
    public Rectangle Bounds { get; set; }
    public bool IsPickedUp { get; set; } = false;
    public string TexturePath { get; private set; }
    public Texture2D Sprite { get; private set; }

    public ItemEntity(string name, float x, float y, int gridW, int gridH, string texturePath)
    {
        Name = name;
        GridWidth = gridW;
        GridHeight = gridH;
        Bounds = new Rectangle(x, y, 30, 30); 

        TexturePath = texturePath;
        Sprite = ResourceManager.GetTexture(texturePath);
    }

    public void Draw()
    {
        if (!IsPickedUp)
        {
            Rectangle sourceRec = new Rectangle(0, 0, Sprite.Width, Sprite.Height);
            Raylib.DrawTexturePro(Sprite, sourceRec, Bounds, Vector2.Zero, 0f, Color.White);
            
            Raylib.DrawText(Name, (int)Bounds.X, (int)Bounds.Y - 15, 10, Color.White);
        }
    }
}