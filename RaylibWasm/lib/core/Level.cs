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
    
    // Primitive zones
    private Rectangle craftingStation = new Rectangle(300, Game.height / 2 - 50, 150, 100);
    private Rectangle farmingArea = new Rectangle(700, Game.height / 2 - 20, 200, 70);

    public void Update(Vector2 playerPos)
    {
        // Check for interactions when pressing 'E'
        if (Raylib.IsKeyPressed(KeyboardKey.E))
        {
            if (Raylib.CheckCollisionPointRec(playerPos, craftingStation))
            {
                IsCraftingMenuOpen = !IsCraftingMenuOpen; // Toggle menu
            }
            // Add farming logic here later
        }
    }

    public void Draw(Vector2 playerPos)
    {
        // Draw the Hub background
        Raylib.ClearBackground(Color.DarkBlue);
        Raylib.DrawRectangle(0, Game.height / 2 + 50, Game.width, Game.height, new Color(20, 40, 30, 255)); // Ground

        // Draw Crafting Station
        Raylib.DrawRectangleRec(craftingStation, Color.Purple);
        Raylib.DrawText("CRAFTING STATION", (int)craftingStation.X + 10, (int)craftingStation.Y - 20, 12, Color.White);

        // Draw Farming Area
        Raylib.DrawRectangleRec(farmingArea, Color.Brown);
        Raylib.DrawText("AUTOMATION FARM", (int)farmingArea.X + 20, (int)farmingArea.Y - 20, 12, Color.White);

        // Interaction Prompts
        if (Raylib.CheckCollisionPointRec(playerPos, craftingStation))
            Raylib.DrawText("[E] TO CRAFT", (int)playerPos.X - 20, (int)playerPos.Y - 40, 20, Color.Yellow);

        if (Raylib.CheckCollisionPointRec(playerPos, farmingArea))
            Raylib.DrawText("[E] TO MANAGE FARM", (int)playerPos.X - 50, (int)playerPos.Y - 40, 20, Color.Yellow);
            
        if (IsCraftingMenuOpen)
        {
            // Placeholder for your actual UI menu later
            Raylib.DrawRectangle(Game.width/2 - 200, Game.height/2 - 150, 400, 300, new Color(0, 0, 0, 200));
            Raylib.DrawText("CRAFTING UI PLACEHOLDER", Game.width/2 - 120, Game.height/2 - 10, 20, Color.White);
        }
    }
}