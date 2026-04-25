using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Primitives;
using RayGUI;

namespace lib.core;

public enum GameState
{
    MAIN_MENU = 0,
    GAME = 1,
    SETTINGS = 2,
    HUB_WORLD = 3,
    CUTSCENE = 4,
    GAME_OVER = 5
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
    public static GameState CurrentGameState = GameState.MAIN_MENU;
    public static EnemyProfile standardMachine;
    public static EnemyProfile samuraiBoss;
    private static ParallaxBackground parallaxBackground;
    private static Texture2D backgroundTexture;
    public static CutsceneManager cutsceneManager;
    public static float gameOverTimer = 0f;
    public static GameState StateAfterCutscene = GameState.HUB_WORLD;

    public static void Init()
    {
        int columns = width / CellSize;
        int rows = height / CellSize;
        
        var gridSize = new GridSize(columns: columns, rows: rows);

        var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1)); 
        var traversalVelocity = Velocity.FromKilometersPerHour(100);

        WorldGrid = Grid.CreateGridWithLateralConnections(gridSize, cellSize, traversalVelocity);

        for (int y = 5; y < 13; y++)
        {
            WorldGrid.DisconnectNode(new GridPosition(13, y));
        }
        
        Vector2 startPos = new Vector2(width / 2, height / 2);
        player = new Player(startPos);  
        camera = new Camera2D(new Vector2(width / 2, height / 2), new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f), 0.0f, 3.0f);
        inventoryManager = new InventoryManager(width, height);

        CurrentRun = new Run();
        Hub = new HubWorld();
        cutsceneManager = new CutsceneManager();
        parallaxBackground = new ParallaxBackground();
        backgroundTexture = Raylib.LoadTexture("Resources/bg/background.jpg");
        
        standardMachine = new EnemyProfile(EnemyType.StandardMachine, maxHealth: 250, speed: 2.0f, attackRange: 60f)
            .AddSingleFileAnimation(EnemyState.ATTACK, "Resources/sprite/enemies/knight/ATTACK.png", frameCount: 10)
            .AddSingleFileAnimation(EnemyState.IDLE, "Resources/sprite/enemies/knight/IDLE.png", frameCount: 4)
            .AddSingleFileAnimation(EnemyState.DEAD, "Resources/sprite/enemies/knight/IDLE.png", frameCount: 4);

        samuraiBoss = new EnemyProfile(EnemyType.EliteSamuraiBot, maxHealth: 550, speed: 3.5f, attackRange: 80f)
            .AddSheetAnimation(EnemyState.IDLE, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 0)
            .AddSheetAnimation(EnemyState.RUN, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 1)
            .AddSheetAnimation(EnemyState.DEAD, "Resources/sprite/enemies/archer/spritesheet.png", frameCount: 8, cellWidth: 64, cellHeight: 64, rowIndex: 1);

        LoadNextLevel();
    }

    public static void LoadNextLevel()
    {
        enemies.Clear();
        
        CurrentLevel = new Level(CurrentRun.CurrentLevelNode, maxRounds: 7);

        for (int i = 1; i <= 6; i++)
        {
            Round r = new Round { IsRandomized = false };
            r.Hordes.Add(new Horde(EnemyType.StandardMachine, standardMachine, amount: 2 + i, HordeBehavior.Mob, spawnInterval: 1.2f));
            CurrentLevel.AddRound(r);
        }

        Round bossRound = new Round { IsRandomized = true };
        bossRound.Hordes.Add(new Horde(EnemyType.StandardMachine, standardMachine, amount: 5, HordeBehavior.Mob, spawnInterval: 1.0f));
        bossRound.Hordes.Add(new Horde(EnemyType.EliteSamuraiBot, samuraiBoss, amount: 1, HordeBehavior.Boss, spawnInterval: 3.0f));
        CurrentLevel.AddRound(bossRound);
    }

    public static void Draw()
    {
        Raylib.BeginDrawing();
        
        Raylib.ClearBackground(Raylib.GetColor(0x052c46ff));

        if (CurrentGameState == GameState.MAIN_MENU)
        {
            Rectangle sourceRec = new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height);
            Rectangle destRec = new Rectangle(0, 0, width, height);
            Raylib.DrawTexturePro(backgroundTexture, sourceRec, destRec, Vector2.Zero, 0f, Color.White);

            Raylib.DrawRectangle(0, 0, width, height, new Color(0, 0, 0, 150));

            // --- 2. CORRUPTED TITLE ANIMATION ---
            string mainTitle = "MECHARIA";
            string subTitle = "A Corrupted Protocol Game";

            int titleFontSize = 80;
            int titleWidth = Raylib.MeasureText(mainTitle, titleFontSize);
            int titleX = (width / 2) - (titleWidth / 2);
            int titleY = height / 4;

            int glitchOffsetX = 0;
            int glitchOffsetY = 0;
            if (Raylib.GetRandomValue(0, 10) > 7) 
            {
                glitchOffsetX = Raylib.GetRandomValue(-5, 5);
                glitchOffsetY = Raylib.GetRandomValue(-5, 5);
            }

            Raylib.DrawText(mainTitle, titleX + glitchOffsetX + 4, titleY + glitchOffsetY, titleFontSize, new Color(0, 255, 255, 200));
            Raylib.DrawText(mainTitle, titleX - glitchOffsetX - 4, titleY - glitchOffsetY, titleFontSize, new Color(255, 0, 255, 200));
            Raylib.DrawText(mainTitle, titleX, titleY, titleFontSize, Color.White);

            int subFontSize = 20;
            int subWidth = Raylib.MeasureText(subTitle, subFontSize);
            Raylib.DrawText(subTitle, (width / 2) - (subWidth / 2), titleY + 90, subFontSize, Color.LightGray);

            // --- 3. MENU BUTTONS ---
            int btnWidth = 200;
            int btnHeight = 50;
            int btnX = (width / 2) - (btnWidth / 2);

            RayGui.GuiSetStyle((int)GuiControl.DEFAULT, (int)GuiDefaultProperty.TEXT_SIZE, 20);

            if (RayGui.GuiButton(new Rectangle(btnX, (height / 2) + 20, btnWidth, btnHeight), "START DEPLOYMENT"))
            {
                CurrentGameState = GameState.CUTSCENE;
                StateAfterCutscene = GameState.HUB_WORLD;
                cutsceneManager.StartCutscene(new List<string> {
                    "WELCOME!",
                    "THIS IS PROTOCOL ^&#@@",
                    "Your Goal is to fight in these chambers and collect scrap to upgrade yourself",
                    "You will be fighting humanoid like machines",
                    "AS THIS TIMELINE IS @#&@&@*@ 78!2323"
                });
            }
            
            // if (RayGui.GuiButton(new Rectangle(btnX, (height / 2) + 90, btnWidth, btnHeight), "SYSTEM SETTINGS"))
            // {
            //     CurrentGameState = GameState.SETTINGS;
            // }
        }  
        // else if (CurrentGameState == GameState.SETTINGS)
        // {
        //     // Settings Screen Background
        //     Rectangle sourceRec = new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height);
        //     Rectangle destRec = new Rectangle(0, 0, width, height);
        //     Raylib.DrawTexturePro(backgroundTexture, sourceRec, destRec, Vector2.Zero, 0f, Color.DarkGray);
        //     Raylib.DrawRectangle(0, 0, width, height, new Color(0, 0, 0, 200));

        //     Raylib.DrawText("SETTINGS", (width/2) - 80, 100, 40, Color.White);

        //     if (RayGui.GuiButton(new Rectangle(50, 50, 100, 40), "<- BACK"))
        //     {
        //         CurrentGameState = GameState.MAIN_MENU;
        //     }
        // }  
        else if (CurrentGameState == GameState.CUTSCENE)
        {
            cutsceneManager.Draw();
        }
        else if (CurrentGameState == GameState.HUB_WORLD)
        {
            Raylib.BeginMode2D(camera);
                parallaxBackground.Draw(camera);
                Hub.DrawWorld(player.GetPlayerPosition());
                player.Draw(); 
            Raylib.EndMode2D();
            
            DrawPlayerUI(); 
            Hub.DrawUI(camera, player.GetPlayerPosition(), inventoryManager);
        }
        else if (CurrentGameState == GameState.GAME) 
        {
            Raylib.BeginMode2D(camera);
                parallaxBackground.Draw(camera);
                CurrentLevel.DrawWorldItems(); 
                player.Draw();  
                foreach (var enemy in enemies) enemy.Draw();
            Raylib.EndMode2D();
            
            DrawPlayerUI();
            CurrentLevel.DrawUI(); 
            DrawEnemyIndicators();
        }
        
        else if (CurrentGameState == GameState.GAME_OVER)
        {
            Raylib.ClearBackground(Color.Black);

            string mainTitle = "CORRUPTED DISK";
            int titleFontSize = 80;
            int titleWidth = Raylib.MeasureText(mainTitle, titleFontSize);
            int titleX = (width / 2) - (titleWidth / 2);
            int titleY = height / 2 - 40;

            // Violent Glitch Effect
            int glitchOffsetX = 0;
            int glitchOffsetY = 0;
            if (Raylib.GetRandomValue(0, 10) > 4)
            {
                glitchOffsetX = Raylib.GetRandomValue(-10, 10);
                glitchOffsetY = Raylib.GetRandomValue(-10, 10);
            }

            Raylib.DrawText(mainTitle, titleX + glitchOffsetX + 5, titleY + glitchOffsetY, titleFontSize, new Color(0, 255, 255, 200));
            Raylib.DrawText(mainTitle, titleX - glitchOffsetX - 5, titleY - glitchOffsetY, titleFontSize, new Color(255, 0, 255, 200));
            Raylib.DrawText(mainTitle, titleX, titleY, titleFontSize, Color.White);

            string subTitle = "FATAL ERROR. ALL INVENTORY AND RUN DATA EXPUNGED.";
            int subFontSize = 20;
            int subWidth = Raylib.MeasureText(subTitle, subFontSize);
            Raylib.DrawText(subTitle, (width / 2) - (subWidth / 2), titleY + 100, subFontSize, Color.Red);
        }
            
        if (isInventoryOpen)
        {
            inventoryManager.Draw();
        }
            
        Raylib.EndDrawing();
    }
    
    public static void Update()
    {
        if (CurrentGameState == GameState.MAIN_MENU || CurrentGameState == GameState.SETTINGS)
        {
            return; 
        }

        if (CurrentGameState == GameState.CUTSCENE)
        {
            cutsceneManager.Update();
            
            if (cutsceneManager.IsFinished)
            {
                CurrentGameState = StateAfterCutscene; 
            }
            return;
        }

        // --- 1. HUB WORLD LOGIC ---
        if (CurrentGameState == GameState.HUB_WORLD)
        {
            Hub.Update(player.GetPlayerPosition(), inventoryManager, player);
            player.Move(10);
            player.Update();
            camera.Target = new Vector2(player.GetPlayerPosition().X + 20.0f, player.GetPlayerPosition().Y + 20.0f);

            if (Hub.WantsToStartLevel) 
            {
                if (CurrentLevel.IsCompleted) LoadNextLevel();
                
                CurrentGameState = GameState.GAME;
                player.SetPosition(new Vector2(width / 2, height / 2));
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
            
            if (Raylib.CheckCollisionRecs(player.Collider, groundItem.Bounds))
            {
                InventoryItem invItem = new InventoryItem(groundItem.Name, groundItem.GridWidth, groundItem.GridHeight, groundItem.TexturePath);
                
                if (inventoryManager.TryAddItem(invItem)) 
                {
                    CurrentLevel.GroundItems.RemoveAt(i);
                }
            }
        }

        if (CurrentLevel != null && !CurrentLevel.IsCompleted)
        {
            CurrentLevel.Update(enemies, player.GetPlayerPosition(), CurrentRun);
        }
        else if (CurrentLevel != null && CurrentLevel.IsCompleted)
        {
            CurrentGameState = GameState.CUTSCENE;
            StateAfterCutscene = GameState.MAIN_MENU;
            
            cutsceneManager.StartCutscene(new List<string> {
                "ALL LOCAL THREATS PURGED.",
                "YOU HAVE SUCCESSFULLY CLEARED LEVEL 1.",
                "THANK YOU FOR PLAYING THE MECHARIA!",
                "RETURNING TO SYSTEM ROOT..."
            });

            CurrentRun = new Run(); 
            Hub = new HubWorld(); 
            inventoryManager.Clear(); 
            
            return; 
        }

        // --- ENEMY UPDATE & LOOT DROPPING ---
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            enemy.Update(player.GetPlayerPosition());

            // 1. PLAYER DAMAGES ENEMY
            if (player.isAttacking && Raylib.CheckCollisionRecs(player.AttackHitbox, enemy.Collider))
            {
                enemy.TakeDamage(player.AttackDamage);
            }

            // 2. ENEMY DAMAGES PLAYER (NEW!)
            if (!enemy.IsDead && Raylib.CheckCollisionRecs(player.Collider, enemy.Collider))
            {
                int damageAmount = enemy.Profile.Type == EnemyType.EliteSamuraiBot ? 30 : 10;
                player.TakeDamage(damageAmount);
            }

            if (enemy.IsDead)
            {
                if (enemy.Profile.Type == EnemyType.EliteSamuraiBot)
                {
                    CurrentLevel.GroundItems.Add(new ItemEntity("Energy Core", enemy.Position.X, enemy.Position.Y + 20, 2, 2, "Resources/core.png"));
                }
                else if (CurrentRun.RNG.NextSingle() <= CurrentRun.ItemDropChance)
                {
                    CurrentLevel.GroundItems.Add(new ItemEntity("Machine Scrap", enemy.Position.X, enemy.Position.Y + 20, 1, 2, "Resources/scrap.png"));
                }
                enemies.RemoveAt(i);
            }
        }

        if (player.Health <= 0 && CurrentGameState == GameState.GAME)
        {
            CurrentGameState = GameState.GAME_OVER;
            gameOverTimer = 4.0f; 
            
            inventoryManager.Clear();
            
            CurrentRun = new Run(); 
            Hub = new HubWorld(); 
        }

        // ---  GAME OVER LOGIC ---
        if (CurrentGameState == GameState.GAME_OVER)
        {
            gameOverTimer -= Raylib.GetFrameTime();
            if (gameOverTimer <= 0)
            {
                player.Health = player.MaxHealth;
                player.SetPosition(new Vector2(width / 2, height / 2)); 
                CurrentGameState = GameState.HUB_WORLD;
            }
            return;
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

    private static void DrawEnemyIndicators()
    {
        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            Vector2 screenPos = Raylib.GetWorldToScreen2D(enemy.Position, camera);

            bool isOffScreenLeft = screenPos.X < 0;
            bool isOffScreenRight = screenPos.X > width;

            if (isOffScreenLeft || isOffScreenRight)
            {
                float indicatorY = screenPos.Y;
                if (indicatorY < 50) indicatorY = 50;
                if (indicatorY > height - 50) indicatorY = height - 50;

                if (isOffScreenLeft)
                {
                    Vector2 tip = new Vector2(20, indicatorY);
                    Vector2 bottomRight = new Vector2(40, indicatorY + 15);
                    Vector2 topRight = new Vector2(40, indicatorY - 15);
                    
                    Raylib.DrawTriangle(tip, bottomRight, topRight, new Color(255, 0, 0, 200));
                }
                else if (isOffScreenRight)
                {
                    Vector2 tip = new Vector2(width - 20, indicatorY);
                    Vector2 topLeft = new Vector2(width - 40, indicatorY - 15);
                    Vector2 bottomLeft = new Vector2(width - 40, indicatorY + 15);
                    
                    Raylib.DrawTriangle(tip, topLeft, bottomLeft, new Color(255, 0, 0, 200));
                }
            }
        }
    }
}