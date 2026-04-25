using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace lib.core;

public enum LevelType
{
    TUTORIAL = 1,
    LEVEL1 = 2,
    LEVEL2 = 3,
    LEVEL3 = 4,
    LEVEL4 = 5,
    LEVEL5 = 6,
}

public class Round
{
    public List<Horde> Hordes { get; set; } = new List<Horde>();
    public bool IsRandomized { get; set; } // If true, hordes spawn in a random order
}

public class Level
{
    public LevelType Type { get; private set; }
    public bool IsCompleted { get; private set; } = false;
    public int MaxRounds { get; private set; } // NEW API
    public List<ItemEntity> GroundItems { get; private set; } = new List<ItemEntity>();

    private List<Round> rounds = new List<Round>();
    private int currentRoundIndex = 0;
    private float spawnTimer = 0f;
    private int activeHordeIndex = 0;

    // Animation Variables
    private float bannerAlpha = 0f;
    private float bannerTimer = 0f;
    private string bannerText = "";

    public Level(LevelType type, int maxRounds)
    {
        Type = type;
        MaxRounds = maxRounds;
    }

    public void AddRound(Round round)
    {
        if (rounds.Count < MaxRounds)
        {
            rounds.Add(round);
        }
        else
        {
            Console.WriteLine($"WARNING: Cannot add more rounds. Level max is {MaxRounds}.");
        }
    }

    public void TriggerRoundAnimation()
    {
        int visualRound = currentRoundIndex + 1;
        bannerText = $"ROUND {visualRound} / {MaxRounds}";
        bannerAlpha = 1.0f; // Reset transparency to fully visible
        bannerTimer = 3.0f; // Stay on screen for 3 seconds
    }

    public void Update(List<Enemy> activeGameEnemies, Vector2 playerPos, Run currentRun)
    {
        if (IsCompleted || currentRoundIndex >= rounds.Count)
        {
            if (activeGameEnemies.Count == 0) IsCompleted = true;
            return;
        }

        // Handle animation fade out
        if (bannerTimer > 0)
        {
            bannerTimer -= Raylib.GetFrameTime();
            if (bannerTimer <= 1.0f) // Fade out over the last second
            {
                bannerAlpha = bannerTimer; 
            }
        }

        // --- Keep your existing Spawning & Shuffle Logic here! ---
        Round currentRound = rounds[currentRoundIndex];
        bool allHordesSpawned = true;

        if (activeHordeIndex < currentRound.Hordes.Count)
        {
            allHordesSpawned = false;
            Horde currentHorde = currentRound.Hordes[activeHordeIndex];
            spawnTimer -= Raylib.GetFrameTime();
            if (spawnTimer <= 0)
            {
                if (!currentHorde.IsFinishedSpawning)
                {
                    int dir = currentRun.RNG.Next(0, 2) == 0 ? -1 : 1; 
                    Vector2 spawnPos = new Vector2(playerPos.X + (dir * 600f), playerPos.Y);
                    activeGameEnemies.Add(currentHorde.SpawnNext(spawnPos, currentRun.EnemyDifficultyMultiplier));
                    spawnTimer = currentHorde.SpawnInterval; 
                }
                else
                {
                    activeHordeIndex++;
                }
            }
        }

        if (allHordesSpawned && activeGameEnemies.Count == 0)
        {
            currentRoundIndex++;
            activeHordeIndex = 0;
            spawnTimer = 4.0f;
            
            if (currentRoundIndex < rounds.Count) 
            {
                TriggerRoundAnimation(); // Pop the UI up for the next round!
            }
        }
    }

    public void DrawWorldItems()
    {
        foreach (var item in GroundItems)
        {
            item.Draw();
        }
    }

    public void DrawUI()
    {
        if (bannerAlpha > 0)
        {
            // Create a fading effect using the Alpha channel
            Color textColor = Raylib.Fade(Color.Red, bannerAlpha);
            Color shadowColor = Raylib.Fade(Color.Black, bannerAlpha);
            
            // Slide animation: Text moves slightly upwards as it fades
            int yPos = (Game.height / 4) + (int)((1.0f - bannerAlpha) * 30); 
            int textWidth = Raylib.MeasureText(bannerText, 60);
            int xPos = (Game.width / 2) - (textWidth / 2);

            Raylib.DrawText(bannerText, xPos + 4, yPos + 4, 60, shadowColor); // Shadow
            Raylib.DrawText(bannerText, xPos, yPos, 60, textColor); // Main Text
        }
    }
}
public class HubWorld
{
    public bool IsCraftingMenuOpen { get; private set; } = false;
    public bool WantsToStartLevel { get; private set; } = false; 
    
    private Rectangle craftingStation = new Rectangle(420, Game.height / 2 - 50, 100, 100);
    private Rectangle farmingArea = new Rectangle(580, Game.height / 2 - 20, 120, 70);
    private Rectangle exitZone = new Rectangle(800, Game.height / 2 - 50, 80, 100); 

    // --- AUTOMATION FARM LOGIC ---
    private float farmTimer = 0f;
    private int storedScrap = 0;
    private int maxScrapStorage = 5;

    public void Update(Vector2 playerPos, InventoryManager inventory, Player player)
    {
        WantsToStartLevel = false;

        // Passive Automation: Generate 1 scrap every 3 seconds
        if (storedScrap < maxScrapStorage)
        {
            farmTimer += Raylib.GetFrameTime();
            if (farmTimer >= 3.0f)
            {
                storedScrap++;
                farmTimer = 0f;
            }
        }

        // Close UI if you walk away
        if (!Raylib.CheckCollisionPointRec(playerPos, craftingStation)) IsCraftingMenuOpen = false;

        if (Raylib.IsKeyPressed(KeyboardKey.E))
        {
            if (Raylib.CheckCollisionPointRec(playerPos, exitZone))
                WantsToStartLevel = true;

            if (Raylib.CheckCollisionPointRec(playerPos, craftingStation))
                IsCraftingMenuOpen = !IsCraftingMenuOpen; 

            // Harvest the farm!
            if (Raylib.CheckCollisionPointRec(playerPos, farmingArea) && storedScrap > 0)
            {
                for (int i = 0; i < storedScrap; i++)
                {
                    // Directly inserts items into your inventory grid
                    inventory.TryAddItem(new InventoryItem("Machine Scrap", 1, 2, "Resources/scrap.png"));
                }
                storedScrap = 0;
            }
        }

        // --- CRAFTING LOGIC ---
        if (IsCraftingMenuOpen)
        {
            int currentScrap = inventory.GetItemCount("Machine Scrap");

            // Recipe 1: Katana Upgrade (Cost: 3 Scrap)
            if (Raylib.IsKeyPressed(KeyboardKey.One) && currentScrap >= 3)
            {
                if (inventory.TryConsumeItems("Machine Scrap", 3))
                    player.AttackDamage += 5; // +5 Damage
            }
            // Recipe 2: Armor Upgrade (Cost: 5 Scrap)
            else if (Raylib.IsKeyPressed(KeyboardKey.Two) && currentScrap >= 5)
            {
                if (inventory.TryConsumeItems("Machine Scrap", 5))
                {
                    player.MaxHealth += 50; 
                    player.Health += 50; // Heal them slightly as a bonus
                }
            }
            // Recipe 3: Boss Key (Cost: 10 Scrap)
            else if (Raylib.IsKeyPressed(KeyboardKey.Three) && currentScrap >= 10)
            {
                if (inventory.TryConsumeItems("Machine Scrap", 10))
                    inventory.TryAddItem(new InventoryItem("Boss Key", 2, 2, "Resources/key.png"));
            }
        }
    }

    public void DrawWorld(Vector2 playerPos)
    {
        Raylib.ClearBackground(Color.DarkBlue);
        Raylib.DrawRectangle(0, Game.height / 2 + 50, Game.width, Game.height, new Color(20, 40, 30, 255)); 

        Raylib.DrawRectangleRec(craftingStation, Color.Purple);
        Raylib.DrawRectangleRec(exitZone, Color.DarkGreen);
        
        // Farm visually fills up with scrap
        Raylib.DrawRectangleRec(farmingArea, Color.Brown);
        float fillRatio = (float)storedScrap / maxScrapStorage;
        Raylib.DrawRectangle((int)farmingArea.X, (int)farmingArea.Y, (int)(farmingArea.Width * fillRatio), (int)farmingArea.Height, Color.Gold);
    }

    public void DrawUI(Camera2D camera, Vector2 playerPos, InventoryManager inventory)
    {
        Raylib.DrawText("CRAFTING STATION", (int)Raylib.GetWorldToScreen2D(new Vector2(craftingStation.X, craftingStation.Y - 20), camera).X, (int)Raylib.GetWorldToScreen2D(new Vector2(craftingStation.X, craftingStation.Y - 20), camera).Y, 20, Color.White);
        Raylib.DrawText($"FARM ({storedScrap}/{maxScrapStorage})", (int)Raylib.GetWorldToScreen2D(new Vector2(farmingArea.X, farmingArea.Y - 20), camera).X, (int)Raylib.GetWorldToScreen2D(new Vector2(farmingArea.X, farmingArea.Y - 20), camera).Y, 20, Color.White);
        Raylib.DrawText("DEPLOY", (int)Raylib.GetWorldToScreen2D(new Vector2(exitZone.X, exitZone.Y - 20), camera).X, (int)Raylib.GetWorldToScreen2D(new Vector2(exitZone.X, exitZone.Y - 20), camera).Y, 20, Color.White);

        if (Raylib.CheckCollisionPointRec(playerPos, craftingStation))
            Raylib.DrawText("[E] TO CRAFT", (int)Raylib.GetWorldToScreen2D(new Vector2(playerPos.X - 20, playerPos.Y - 40), camera).X, (int)Raylib.GetWorldToScreen2D(new Vector2(playerPos.X - 20, playerPos.Y - 40), camera).Y, 20, Color.Yellow);

        if (Raylib.CheckCollisionPointRec(playerPos, farmingArea))
            Raylib.DrawText("[E] TO HARVEST", (int)Raylib.GetWorldToScreen2D(new Vector2(playerPos.X - 20, playerPos.Y - 40), camera).X, (int)Raylib.GetWorldToScreen2D(new Vector2(playerPos.X - 20, playerPos.Y - 40), camera).Y, 20, Color.Yellow);

        if (Raylib.CheckCollisionPointRec(playerPos, exitZone))
            Raylib.DrawText("[E] TO DEPLOY", (int)Raylib.GetWorldToScreen2D(new Vector2(playerPos.X - 20, playerPos.Y - 40), camera).X, (int)Raylib.GetWorldToScreen2D(new Vector2(playerPos.X - 20, playerPos.Y - 40), camera).Y, 20, Color.Yellow);

        if (IsCraftingMenuOpen)
        {
            int curScrap = inventory.GetItemCount("Machine Scrap");
            
            // Draw UI Background
            Raylib.DrawRectangle(Game.width/2 - 250, Game.height/2 - 150, 500, 300, new Color(0, 0, 0, 220));
            Raylib.DrawRectangleLines(Game.width/2 - 250, Game.height/2 - 150, 500, 300, Color.White);
            
            Raylib.DrawText($"ASSEMBLER - YOUR SCRAP: {curScrap}", Game.width/2 - 180, Game.height/2 - 130, 20, Color.Gold);
            
            // Recipes
            Raylib.DrawText(curScrap >= 3 ? "[1] Sharpen Katana (+5 DMG) : Costs 3 Scrap" : "[1] Sharpen Katana : Insufficient Scrap", Game.width/2 - 220, Game.height/2 - 70, 18, curScrap >= 3 ? Color.Green : Color.Gray);
            Raylib.DrawText(curScrap >= 5 ? "[2] Forge Armor (+50 Max HP) : Costs 5 Scrap" : "[2] Forge Armor : Insufficient Scrap", Game.width/2 - 220, Game.height/2 - 30, 18, curScrap >= 5 ? Color.Green : Color.Gray);
            Raylib.DrawText(curScrap >= 10 ? "[3] Print Boss Key (Required) : Costs 10 Scrap" : "[3] Print Boss Key : Insufficient Scrap", Game.width/2 - 220, Game.height/2 + 10, 18, curScrap >= 10 ? Color.Green : Color.Gray);
        }
    }
}