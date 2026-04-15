using System;
using System.Numerics;
using Raylib_cs;

namespace lib.core;

// 1. Define the different types of enemies in your game
public enum EnemyType
{
    StandardMachine,
    EliteSamuraiBot
}

// 2. Define the animation states
public enum EnemyState
{
    IDLE,
    RUN,
    ATTACK,
    HURT,
    DEAD
}

// 3. Define how the sprite sheet is formatted
public enum SpriteSheetLayout
{
    SingleSheet,   // 1 image containing rows of animations
    SeparateFiles  // Multiple images (IDLE.png, RUN.png, etc.)
}

public class Enemy
{
    public EnemyProfile Profile { get; private set; }
    public int Health { get; set; }
    public Vector2 Position { get; private set; }
    
    // Collider dynamically reads the active animation config size
    public Rectangle Collider => new Rectangle(Position.X, Position.Y, 
        Profile.Animations.ContainsKey(currentState) ? Profile.Animations[currentState].FrameWidth : 40, 
        Profile.Animations.ContainsKey(currentState) ? Profile.Animations[currentState].FrameHeight : 40);

    public bool IsDead => Health <= 0 && currentState == EnemyState.DEAD && 
                          currentFrame >= Profile.Animations[EnemyState.DEAD].FrameCount - 1;

    private bool isFacingRight = true;
    private EnemyState currentState = EnemyState.IDLE;
    private int currentFrame = 0;
    private int frameCounter = 0;
    private int frameSpeed = 8;
    private Rectangle frameRec;

    private float currentAttackTimer = 0f;
    private float attackCooldown = 1.5f;

    // Notice how clean the constructor is now!
    public Enemy(Vector2 startPosition, EnemyProfile profile)
    {
        Position = startPosition;
        Profile = profile;
        Health = profile.MaxHealth;
        
        ChangeState(EnemyState.IDLE);
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState != newState && Profile.Animations.ContainsKey(newState))
        {
            currentState = newState;
            currentFrame = 0;
            frameCounter = 0;
            UpdateFrameRectangle();
        }
    }

    public void Update(Vector2 playerPosition)
    {
        if (currentState == EnemyState.DEAD || currentState == EnemyState.HURT || currentState == EnemyState.ATTACK)
        {
            UpdateAnimation();
            return;
        }

        float distanceX = Math.Abs(playerPosition.X - Position.X);
        currentAttackTimer -= Raylib.GetFrameTime();

        if (distanceX < Profile.AttackRange && currentAttackTimer <= 0)
        {
            ChangeState(EnemyState.ATTACK);
            currentAttackTimer = attackCooldown;
        }
        else if (distanceX >= Profile.AttackRange && distanceX < 600f) 
        {
            ChangeState(EnemyState.RUN);
            int dirX = playerPosition.X > Position.X ? 1 : -1;
            Position = new Vector2(Position.X + (dirX * Profile.Speed), Position.Y);
            isFacingRight = dirX == 1;
        }
        else
        {
            ChangeState(EnemyState.IDLE);
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (!Profile.Animations.ContainsKey(currentState)) return;

        AnimationConfig config = Profile.Animations[currentState];

        frameCounter++;
        if (frameCounter >= (Game.fps / frameSpeed))
        {
            frameCounter = 0;
            currentFrame++;

            if (currentFrame >= config.FrameCount)
            {
                if (currentState == EnemyState.DEAD)
                    currentFrame = config.FrameCount - 1; 
                else if (currentState == EnemyState.HURT || currentState == EnemyState.ATTACK)
                    ChangeState(EnemyState.IDLE); 
                else
                    currentFrame = 0; 
            }
        }
        UpdateFrameRectangle();
    }

    private void UpdateFrameRectangle()
    {
        if (Profile.Animations.ContainsKey(currentState))
        {
            AnimationConfig config = Profile.Animations[currentState];
            frameRec = new Rectangle(currentFrame * config.FrameWidth, config.OffsetY, config.FrameWidth, config.FrameHeight);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (Health <= 0) return;
        Health -= damageAmount;
        ChangeState(Health <= 0 ? EnemyState.DEAD : EnemyState.HURT);
    }

    public void Draw()
    {
        if (IsDead || !Profile.Animations.ContainsKey(currentState)) return;

        AnimationConfig config = Profile.Animations[currentState];
        Rectangle drawRec = frameRec;

        if (!isFacingRight) drawRec.Width = -drawRec.Width;

        Raylib.DrawTextureRec(config.Texture, drawRec, Position, Color.White);

        // Health bar
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, config.FrameWidth, 5, Color.Black);
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, (int)(config.FrameWidth * ((float)Health / Profile.MaxHealth)), 5, Color.Red);
    }
}