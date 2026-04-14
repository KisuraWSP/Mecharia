using System.Collections.Generic;
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
    private static bool isInventoryOpen = false;
    private static InventoryManager inventoryManager;
    private List<ItemEntity> worldItems;
    private static List<Enemy> enemies = new List<Enemy>();
    public static void Init()
    {
        Vector2 startPos = new Vector2(width / 2, height / 2);
        player = new Player(startPos);  
        camera = new Camera2D(new Vector2(width / 2, height / 2), new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f), 0.0f, 3.0f);
        inventoryManager = new InventoryManager(width, height);
        enemies.Add(new Enemy(new Vector2(startPos.X + 300, startPos.Y)));
    }

    public static void Draw()
    {
        Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);
            
            Raylib.BeginMode2D(camera);
                player.Draw();  
                foreach (var enemy in enemies)
                {
                    enemy.Draw();
                }  
            Raylib.EndMode2D();
            
            DrawPlayerUI();
            
            if (isInventoryOpen)
            {
                inventoryManager.Draw();
            }
        Raylib.EndDrawing();
    }

    public static void Update()
    {
        player.Move(10);
        player.Update();
        camera.Target = new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f);
        if (Raylib.IsKeyPressed(KeyboardKey.Tab) || Raylib.IsKeyPressed(KeyboardKey.I))
        {
            isInventoryOpen = !isInventoryOpen;
        }

        if (isInventoryOpen)
        {
            inventoryManager.Update();
            return; 
        }

        // --- ENEMY UPDATE & COLLISION LOGIC ---
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            enemy.Update(player.GetPlayerPosition());

            // Check if player's attack hitbox hits the enemy
            if (player.isAttacking)
            {
                if (Raylib.CheckCollisionRecs(player.AttackHitbox, enemy.Collider))
                {
                    // Deal damage! (You might want to add a cooldown timer to the player 
                    // so they don't do 60 instances of damage in a single swing frame)
                    enemy.TakeDamage(1); 
                }
            }

            // If enemy dies, remove them from the list
            if (enemy.IsDead)
            {
                enemies.RemoveAt(i);
            }
        }
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