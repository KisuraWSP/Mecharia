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
    HUB_WORLD = 7
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

    // GDD Feature: Item Requirements and Restrictions
    public List<string> RequiredItemsToEnter { get; private set; } = new List<string>();
    public List<string> RestrictedItems { get; private set; } = new List<string>();

    private List<Round> rounds = new List<Round>();
    private int currentRoundIndex = 0;
    
    private float spawnTimer = 0f;
    private int activeHordeIndex = 0;

    public Level(LevelType type)
    {
        Type = type;
    }

    public void AddRound(Round round)
    {
        rounds.Add(round);
    }

    public void Update(List<Enemy> activeGameEnemies, Vector2 playerPos, Run currentRun)
    {
        if (IsCompleted || currentRoundIndex >= rounds.Count)
        {
            if (activeGameEnemies.Count == 0) IsCompleted = true;
            return;
        }

        Round currentRound = rounds[currentRoundIndex];

        // Randomize the round once when it starts if the flag is set
        if (currentRound.IsRandomized && activeHordeIndex == 0 && activeGameEnemies.Count == 0)
        {
            ShuffleHordes(currentRound.Hordes, currentRun.RNG);
            currentRound.IsRandomized = false; // Prevent re-shuffling every frame
        }

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
                    // Spawn off-screen
                    int dir = currentRun.RNG.Next(0, 2) == 0 ? -1 : 1; 
                    Vector2 spawnPos = new Vector2(playerPos.X + (dir * 600f), playerPos.Y);
                    
                    activeGameEnemies.Add(currentHorde.SpawnNext(spawnPos, currentRun.EnemyDifficultyMultiplier));
                    
                    // Use this horde's specific interval
                    spawnTimer = currentHorde.SpawnInterval; 
                }
                else
                {
                    // Move to the next horde in the round
                    activeHordeIndex++;
                }
            }
        }

        // If round is completely spawned and player killed them all, advance round
        if (allHordesSpawned && activeGameEnemies.Count == 0)
        {
            currentRoundIndex++;
            activeHordeIndex = 0;
            spawnTimer = 4.0f; // Brief pause between rounds
        }
    }

    // Utility to randomize the order hordes spawn in
    private void ShuffleHordes(List<Horde> hordes, System.Random rng)
    {
        int n = hordes.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            Horde value = hordes[k];  
            hordes[k] = hordes[n];  
            hordes[n] = value;  
        }  
    }
}

public class HubWorld
{
    // Main world before a level starts. Interface unlocked after tutorial.
    public bool IsCraftingMenuOpen { get; private set; } = false;

    public void Update()
    {
        // Logic for interacting with automation machines, farming, and crafting
    }

    public void Draw()
    {
        // Draw the safe zone, machines, and crafting UI
    }

    // GDD: Checks item restrictions & requirements to encourage horizon broadening
    public bool TryEnterLevel(Level targetLevel, InventoryManager playerInventory)
    {
        // 1. Check Requirements (Items the player MUST have)
        foreach (var reqItem in targetLevel.RequiredItemsToEnter)
        {
            if (!playerInventory.Items.Exists(i => i.Name == reqItem))
            {
                // UI should show: "Missing required item: " + reqItem
                return false; 
            }
        }

        // 2. Check Restrictions (Items the player CANNOT bring)
        foreach (var restrictedItem in targetLevel.RestrictedItems)
        {
            if (playerInventory.Items.Exists(i => i.Name == restrictedItem))
            {
                // UI should show: "You cannot bring " + restrictedItem + " into this level!"
                return false;
            }
        }

        return true; // Requirements met, restrictions obeyed!
    }
}