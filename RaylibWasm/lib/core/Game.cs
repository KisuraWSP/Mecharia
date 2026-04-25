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
    SETTINGS = 3,
    HUB_WORLD = 4
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
    public static Run CurrentRun;
    public static Level CurrentLevel;
    public static HubWorld Hub;
    public static GameState CurrentGameState = GameState.HUB_WORLD;
    public static EnemyProfile standardMachine;
    public static EnemyProfile samuraiBoss;

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

        // 1. Initialize the Game Managers
        CurrentRun = new Run(); // Starts the seeded RNG
        Hub = new HubWorld();
        
        // change these to cross platform paths
        // also very buggy system rework this
        standardMachine = new EnemyProfile(EnemyType.StandardMachine, maxHealth: 50, speed: 2.0f, attackRange: 60f)
            .AddSingleFileAnimation(EnemyState.ATTACK, "Resources/sprite/enemies/knight/ATTACK.png", frameCount: 10)
            .AddSingleFileAnimation(EnemyState.IDLE, "Resources/sprite/enemies/knight/IDLE.png", frameCount: 4)
            .AddSingleFileAnimation(EnemyState.DEAD, "Resources/sprite/enemies/knight/IDLE.png", frameCount: 4);

        // 2. Define the boss (using a Full Grid Sheet where frames are 64x64 pixels)
        samuraiBoss = new EnemyProfile(EnemyType.EliteSamuraiBot, maxHealth: 300, speed: 3.5f, attackRange: 80f)
            .AddSheetAnimation(EnemyState.IDLE, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 0)
            .AddSheetAnimation(EnemyState.RUN, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 1)
            .AddSheetAnimation(EnemyState.DEAD, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 1);

        LoadNextLevel();
    }

    public static void LoadNextLevel()
    {
        // Clear any leftover data from the previous level
        enemies.Clear();
        
        // CurrentLevelNode tracks if you are on Level 1, 2, 3, etc.
        CurrentLevel = new Level(CurrentRun.CurrentLevelNode, maxRounds: 2);

        Round round1 = new Round { IsRandomized = false };
        round1.Hordes.Add(new Horde(EnemyType.StandardMachine, standardMachine, amount: 5, HordeBehavior.Mob, spawnInterval: 1.5f));
        CurrentLevel.AddRound(round1);

        Round round2 = new Round { IsRandomized = true };
        round2.Hordes.Add(new Horde(EnemyType.StandardMachine, standardMachine, amount: 1, HordeBehavior.Mob, spawnInterval: 1.0f));
        round2.Hordes.Add(new Horde(EnemyType.EliteSamuraiBot, samuraiBoss, amount: 1, HordeBehavior.Boss, spawnInterval: 3.0f));
        CurrentLevel.AddRound(round2);
    }

    public static void Draw()
    {
        Raylib.BeginDrawing();
        
        // Use a default clear background
        Raylib.ClearBackground(Color.SkyBlue);
            
        if (CurrentGameState == GameState.HUB_WORLD)
        {
            Raylib.BeginMode2D(camera);
                Hub.DrawWorld(player.GetPlayerPosition());
                player.Draw(); 
            Raylib.EndMode2D();
            
            DrawPlayerUI(); 
            
            // Pass the camera in so it can calculate the text positions!
            Hub.DrawUI(camera, player.GetPlayerPosition(), inventoryManager);
        }
        else if (CurrentGameState == GameState.GAME) // <--- THIS WAS MISSING
        {
            // Draw Level
            Raylib.BeginMode2D(camera);
                CurrentLevel.DrawWorldItems(); // Draw items on ground
                player.Draw();  
                foreach (var enemy in enemies) enemy.Draw();
            Raylib.EndMode2D();
            
            DrawPlayerUI();
            CurrentLevel.DrawUI(); // Draw Round Animations Over Screen
        }
            
        if (isInventoryOpen)
        {
            inventoryManager.Draw();
        }
            
        Raylib.EndDrawing();
    }

    public static void Update()
    {
        // --- 1. HUB WORLD LOGIC ---
        if (CurrentGameState == GameState.HUB_WORLD)
        {
            Hub.Update(player.GetPlayerPosition(), inventoryManager, player);
            player.Move(10);
            player.Update();
            camera.Target = new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f);

            // Triggered when the player stands on the green exit zone and hits 'E'
            if (Hub.WantsToStartLevel) 
            {
                // If they previously beat a level, load a fresh one before jumping in
                if (CurrentLevel.IsCompleted) LoadNextLevel();
                
                CurrentGameState = GameState.GAME;
                player.SetPosition(new Vector2(width / 2, height / 2)); // Teleport to Level Start
                CurrentLevel.TriggerRoundAnimation(); 
            }
            return;
        }

        // Inventory UI Logic
        if (Raylib.IsKeyPressed(KeyboardKey.Tab) || Raylib.IsKeyPressed(KeyboardKey.I))
        {
            isInventoryOpen = !isInventoryOpen;
        }

        if (isInventoryOpen)
        {
            inventoryManager.Update();
            return; 
        }

        player.Move(10);
        player.Update();
        camera.Target = new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f);

        // --- LOOT PICKUP LOGIC ---
        for (int i = CurrentLevel.GroundItems.Count - 1; i >= 0; i--)
        {
            var groundItem = CurrentLevel.GroundItems[i];
            
            // If player touches the item, try to add it to inventory
            if (Raylib.CheckCollisionRecs(player.Collider, groundItem.Bounds))
            {
                InventoryItem invItem = new InventoryItem(groundItem.Name, groundItem.GridWidth, groundItem.GridHeight, groundItem.TexturePath);
                
                // Only remove it from the floor if there is room in the inventory grid!
                if (inventoryManager.TryAddItem(invItem)) 
                {
                    CurrentLevel.GroundItems.RemoveAt(i);
                }
            }
        }

        if (CurrentLevel != null && !CurrentLevel.IsCompleted)
        {
            // Level is active, update spawns!
            CurrentLevel.Update(enemies, player.GetPlayerPosition(), CurrentRun);
        }
        else if (CurrentLevel != null && CurrentLevel.IsCompleted)
        {
            // LEVEL WON! Transition back to the Hub World!
            CurrentRun.CompleteLevel(CurrentLevel.Type);
            CurrentGameState = GameState.HUB_WORLD;
            player.SetPosition(new Vector2(width / 2, height / 2)); // Teleport back to Hub start
            return; // Skip the rest of the update this frame
        }

        // --- ENEMY UPDATE & LOOT DROPPING ---
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            enemy.Update(player.GetPlayerPosition());

            if (player.isAttacking && Raylib.CheckCollisionRecs(player.AttackHitbox, enemy.Collider))
            {
                enemy.TakeDamage(player.AttackDamage);
            }

            if (enemy.IsDead)
            {
                // Check if it's a boss!
                if (enemy.Profile.Type == EnemyType.EliteSamuraiBot)
                {
                    // Bosses always drop a rare 2x2 Energy Core!
                    CurrentLevel.GroundItems.Add(new ItemEntity("Energy Core", enemy.Position.X, enemy.Position.Y + 20, 2, 2, "Resources/core.png"));
                }
                else if (CurrentRun.RNG.NextSingle() <= CurrentRun.ItemDropChance)
                {
                    // Standard enemies drop 1x2 Scrap
                    CurrentLevel.GroundItems.Add(new ItemEntity("Machine Scrap", enemy.Position.X, enemy.Position.Y + 20, 1, 2, "Resources/scrap.png"));
                }
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