using System;
using System.Collections.Generic;

namespace lib.core;

public class Run
{
    public int Seed { get; private set; }
    public Random RNG { get; private set; }
    
    // GDD: "rank up difficulty of enemies and increase/decrease spawn rate of items"
    public float EnemyDifficultyMultiplier { get; private set; } = 1.0f;
    public float ItemDropChance { get; private set; } = 0.5f;

    // Tracks the set amount of levels
    public LevelType CurrentLevelNode { get; private set; }
    public List<LevelType> CompletedLevels { get; private set; } = new List<LevelType>();

    public Run(int? specificSeed = null)
    {
        Seed = specificSeed ?? new Random().Next();
        RNG = new Random(Seed);
        
        // Base starting point
        CurrentLevelNode = LevelType.TUTORIAL;

        // Seed mathematically determines starting item drops and difficulty
        EnemyDifficultyMultiplier = 0.8f + (RNG.NextSingle() * 0.5f); 
        ItemDropChance = 0.3f + (RNG.NextSingle() * 0.4f); 
    }

    public void CompleteLevel(LevelType levelCompleted)
    {
        CompletedLevels.Add(levelCompleted);
        
        // Rank up difficulty as the run progresses
        EnemyDifficultyMultiplier += 0.2f; 
        
        // Move back to Hub World after a level (per GDD Page 14)
    }

    public void StartNextLevel(LevelType nextLevel)
    {
        CurrentLevelNode = nextLevel;
    }
}