using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Primitives;

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

    // Pathfinding properties
    public static int CellSize = 40; 
    public static Grid WorldGrid;

    public static void Init()
    {
        int columns = width / CellSize;
        int rows = height / CellSize;
        
        var gridSize = new GridSize(columns: columns, rows: rows);

        var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1)); 
        var traversalVelocity = Velocity.FromKilometersPerHour(100);

        // Create the grid (Use CreateGridWithLateralAndDiagonalConnections if you want diagonal movement!)
        WorldGrid = Grid.CreateGridWithLateralConnections(gridSize, cellSize, traversalVelocity);

        // 2. Create a fake "wall" right in the middle of the screen by DISCONNECTING the nodes
        for (int y = 5; y < 13; y++)
        {
            WorldGrid.DisconnectNode(new GridPosition(13, y));
        }
        
        Vector2 startPos = new Vector2(width / 2, height / 2);
        player = new Player(startPos);  
        camera = new Camera2D(new Vector2(width / 2, height / 2), new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f), 0.0f, 3.0f);
        inventoryManager = new InventoryManager(width, height);
        
        // change these to cross platform paths
        // also very buggy system rework this
        EnemyProfile standardMachine = new EnemyProfile(EnemyType.StandardMachine, maxHealth: 50, speed: 2.0f, attackRange: 60f)
            .AddSingleFileAnimation(EnemyState.ATTACK, "Resources/sprite/enemies/knight/ATTACK.png", frameCount: 10)
            .AddSingleFileAnimation(EnemyState.IDLE, "Resources/sprite/enemies/knight/IDLE.png", frameCount: 4);

        // 2. Define the boss (using a Full Grid Sheet where frames are 64x64 pixels)
        EnemyProfile samuraiBoss = new EnemyProfile(EnemyType.EliteSamuraiBot, maxHealth: 300, speed: 3.5f, attackRange: 80f)
            .AddSheetAnimation(EnemyState.IDLE, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 0)
            .AddSheetAnimation(EnemyState.RUN, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 1);

        // 3. Spawn them!
        enemies.Add(new Enemy(new Vector2(400, 360), standardMachine));
        enemies.Add(new Enemy(new Vector2(800, 360), samuraiBoss));
    }

    public static void Draw()
    {
        Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);
            
            Raylib.BeginMode2D(camera);
                Raylib.DrawRectangle(13 * CellSize, 5 * CellSize, CellSize, 8 * CellSize, Color.DarkGray);
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
            
            // Pass the WorldGrid into the enemy update
            enemy.Update(player.GetPlayerPosition());

            if (player.isAttacking)
            {
                if (Raylib.CheckCollisionRecs(player.AttackHitbox, enemy.Collider))
                {
                    enemy.TakeDamage(1); 
                }
            }

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