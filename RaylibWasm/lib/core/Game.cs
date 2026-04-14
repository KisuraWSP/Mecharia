using System.Numerics;
using Raylib_cs;

namespace lib.core;

public enum GameState
{
    MAIN_MENU = 0,
    GAME = 1,
    PAUSE = 2,
    SETTINGS = 3
}

public class Game
{
    public static int width = 1080;
    public static int height = 720;
    public static string title = "Mecharia";
    public static int fps = 60; 
    private static Player player;
    private static Camera2D camera;

    public static void Init()
    {
        Vector2 startPos = new Vector2(width / 2, height / 2);
        player = new Player(startPos);  
        camera = new Camera2D(new Vector2(width / 2, height / 2), new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f), 0.0f, 3.0f);
        
    }

    public static void Draw()
    {
        Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);
            Raylib.BeginMode2D(camera);
            DrawPlayerUI();
            player.Draw();
            Raylib.EndMode2D();
        Raylib.EndDrawing();
        
    }

    public static void Update()
    {
        player.Move(10);
        player.Update();
        camera.Target = new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f);

    }   

    private static void DrawPlayerUI()
    {
        Raylib.DrawRectangle(20, 20, 200, 20, Color.Red);
        
        float healthPercent = (float)player.Health / player.MaxHealth;
        Raylib.DrawRectangle(20, 20, (int)(200 * healthPercent), 20, Color.Green);
        
        Raylib.DrawRectangleLines(20, 20, 200, 20, Color.White);
        Raylib.DrawText($"HP: {player.Health}/{player.MaxHealth}", 25, 22, 16, Color.Black);
    }
}