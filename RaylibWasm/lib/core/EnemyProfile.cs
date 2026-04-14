using System.Collections.Generic;
using Raylib_cs;

namespace lib.core;

public class AnimationConfig
{
    public EnemyState State { get; set; }
    public Texture2D Texture { get; set; }
    public int FrameCount { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public int OffsetY { get; set; }
}

public class EnemyProfile
{
    public EnemyType Type { get; private set; }
    public int MaxHealth { get; private set; }
    public float Speed { get; private set; }
    public float AttackRange { get; private set; }
    public Dictionary<EnemyState, AnimationConfig> Animations { get; private set; } = new();

    public EnemyProfile(EnemyType type, int maxHealth, float speed, float attackRange)
    {
        Type = type;
        MaxHealth = maxHealth;
        Speed = speed;
        AttackRange = attackRange;
    }

    // Method for Single Files (e.g. ATTACK.png, BLOCK.png)
    public EnemyProfile AddSingleFileAnimation(EnemyState state, string texturePath, int frameCount)
    {
        Texture2D tex = ResourceManager.GetTexture(texturePath);
        
        Animations[state] = new AnimationConfig
        {
            State = state,
            Texture = tex,
            FrameCount = frameCount,
            // Dynamically calculates the width based on the number of frames you pass in!
            FrameWidth = tex.Width / frameCount, 
            FrameHeight = tex.Height,
            OffsetY = 0
        };
        return this; // Returning 'this' allows for method chaining
    }

    // Method for a Full Grid Spritesheet (e.g. spritesheet.png)
    public EnemyProfile AddSheetAnimation(EnemyState state, string texturePath, int frameCount, int cellWidth, int cellHeight, int rowIndex)
    {
        Texture2D tex = ResourceManager.GetTexture(texturePath);

        Animations[state] = new AnimationConfig
        {
            State = state,
            Texture = tex,
            FrameCount = frameCount,
            FrameWidth = cellWidth,
            FrameHeight = cellHeight,
            OffsetY = rowIndex * cellHeight // Calculates how far down the sheet to slice
        };
        return this;
    }
}