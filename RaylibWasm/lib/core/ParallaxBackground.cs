using Raylib_cs;
using System.Numerics;

namespace lib.core;

public class ParallaxBackground
{
    private Texture2D sky;
    private Texture2D mountainsFar;
    private Texture2D mountainsMid;
    private Texture2D mountainsNear;
    private Texture2D crystals;
    private Texture2D ground;

    // Tweak this number to move the whole background up or down relative to the player's feet
    private float yOffset = 20f; 

    public ParallaxBackground()
    {
        sky = ResourceManager.GetTexture("Resources/bg/01_sky_planet.png");
        mountainsFar = ResourceManager.GetTexture("Resources/bg/02_mountains_far.png");
        mountainsMid = ResourceManager.GetTexture("Resources/bg/03_mountains_mid.png");
        mountainsNear = ResourceManager.GetTexture("Resources/bg/04_mountains_near.png");
        crystals = ResourceManager.GetTexture("Resources/bg/05_crystals_big.png");
        ground = ResourceManager.GetTexture("Resources/bg/06_ground_fg.png");
    }

    // We now accept the Camera2D object directly!
    public void Draw(Camera2D camera)
    {
        // 1. Find the exact top-left corner of the screen in the game world
        Vector2 topLeft = Raylib.GetScreenToWorld2D(new Vector2(0, 0), camera);

        // 2. Calculate exactly how tall the camera's view is in world units (Height / Zoom)
        float viewHeight = Game.height / camera.Zoom;

        // 3. Draw layers with different parallax speeds
        DrawLayer(sky, camera.Target.X, topLeft, viewHeight, 0.01f);
        DrawLayer(mountainsFar, camera.Target.X, topLeft, viewHeight, 0.1f);
        DrawLayer(mountainsMid, camera.Target.X, topLeft, viewHeight, 0.3f);
        DrawLayer(mountainsNear, camera.Target.X, topLeft, viewHeight, 0.5f);
        DrawLayer(crystals, camera.Target.X, topLeft, viewHeight, 0.7f);
        DrawLayer(ground, camera.Target.X, topLeft, viewHeight, 0.9f);
    }

    private void DrawLayer(Texture2D tex, float playerX, Vector2 cameraTopLeft, float viewHeight, float parallaxFactor)
    {
        // Calculate scale to make the texture fit the camera's height perfectly
        float scale = viewHeight / tex.Height;
        float scaledWidth = tex.Width * scale;

        // Calculate shift. We use extra math here to prevent negative wrapping issues when walking left!
        float rawShift = playerX * parallaxFactor;
        float scrollOffset = ((rawShift % scaledWidth) + scaledWidth) % scaledWidth;

        // Start drawing from the left edge of the screen, minus the offset
        float drawX = cameraTopLeft.X - scrollOffset;
        float drawY = cameraTopLeft.Y + yOffset; 

        // Draw 3 tiles using DrawTextureEx so we can apply the scale
        Raylib.DrawTextureEx(tex, new Vector2(drawX - scaledWidth, drawY), 0f, scale, Color.White);
        Raylib.DrawTextureEx(tex, new Vector2(drawX, drawY), 0f, scale, Color.White);
        Raylib.DrawTextureEx(tex, new Vector2(drawX + scaledWidth, drawY), 0f, scale, Color.White);
    }
}