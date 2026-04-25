using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace lib.core;

public enum AnimationState
{
    ATTACK = 0,
    HURT = 1,
    IDLE = 2,
    RUN = 3
}
public class Player
{
    public int Health { get; set; } = 200;
    public int MaxHealth { get; set; } = 200;
    public Rectangle Collider { get; private set; }
    
    // NEW: Hitbox for your katana to check enemy collisions later
    public Rectangle AttackHitbox { get; private set; } 
    public int AttackDamage { get; set; } = 10;

    private int currentFrame = 0;
    private int frameCounter = 0;
    private int frameSpeed = 8;
    private Dictionary<AnimationState, Texture2D> textures;
    private Dictionary<AnimationState, int> frameCounts;
    private AnimationState animationState = AnimationState.IDLE;
    private Vector2 position;
    private Rectangle frameRec;
    private bool isFacingRight = true;
    
    // NEW: State flag to track if we are currently swinging the katana
    public bool isAttacking = false; 

    private static string baseDir = AppContext.BaseDirectory;
    private static string resourcesDir = Path.Combine(baseDir, "Resources", "sprite", "player");

    private float invincibilityTimer = 0f;

    public Player(Vector2 _position)
    {
        textures = new Dictionary<AnimationState, Texture2D>();
        frameCounts = new Dictionary<AnimationState, int>();

        textures[AnimationState.ATTACK] = Raylib.LoadTexture(Path.Combine(resourcesDir,"ATTACK.png"));
        frameCounts[AnimationState.ATTACK] = 7;

        textures[AnimationState.HURT] = Raylib.LoadTexture(Path.Combine(resourcesDir,"HURT.png"));
        frameCounts[AnimationState.HURT] = 4;
        
        textures[AnimationState.IDLE] = Raylib.LoadTexture(Path.Combine(resourcesDir,"IDLE.png"));
        frameCounts[AnimationState.IDLE] = 10;
        
        textures[AnimationState.RUN] = Raylib.LoadTexture(Path.Combine(resourcesDir, "RUN.png"));
        frameCounts[AnimationState.RUN] = 16;
        
        animationState = AnimationState.IDLE;
        UpdateFrameRectangle();

        position = _position;;

        Collider = new Rectangle(position.X, position.Y, frameRec.Width, frameRec.Height);
    }

    private void ChangeState(AnimationState newState)
    {
        if (animationState != newState)
        {
            animationState = newState;
            currentFrame = 0;
            frameCounter = 0;
            UpdateFrameRectangle();
        }
    }

    private void UpdateFrameRectangle()
    {
        Texture2D texture = textures[animationState];
        int frames = frameCounts[animationState];

        frameRec = new Rectangle(0, 0, (float)texture.Width/frames, (float)texture.Height);
    }

    public void Update()
    {
        // NEW: Tick down the invincibility timer
        if (invincibilityTimer > 0) invincibilityTimer -= Raylib.GetFrameTime();

        // Prevent attacking while hurt
        if (animationState != AnimationState.HURT && !isAttacking && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            isAttacking = true;
            ChangeState(AnimationState.ATTACK);
        }

        frameCounter++;

        if (frameCounter >= (Game.fps/frameSpeed))
        {
            frameCounter = 0;
            currentFrame++;

            if (currentFrame >= frameCounts[animationState]) 
            {
                if (isAttacking || animationState == AnimationState.HURT)
                {
                    // Return to IDLE after an attack OR after taking damage
                    isAttacking = false;
                    ChangeState(AnimationState.IDLE);
                }
                else
                {
                    currentFrame = 0;
                }
            }
            
            if (currentFrame < frameCounts[animationState])
            {
                frameRec.X = (float)currentFrame * (float)frameRec.Width;
            }
        }
        
        Collider = new Rectangle(position.X, position.Y, frameRec.Width, frameRec.Height);
        UpdateAttackHitbox();
    }

    public void Draw()
    {
        // NEW: Blinking effect when invincible (Draws only every other frame)
        if (invincibilityTimer > 0 && (int)(invincibilityTimer * 10) % 2 == 0) return;

        Rectangle drawRec = frameRec;

        if (!isFacingRight) drawRec.Width = -drawRec.Width;
        
        Raylib.DrawTextureRec(textures[animationState], drawRec, position, Color.White);
    }

    private void UpdateAttackHitbox()
    {
        if (isAttacking)
        {
            // Position the hitbox in front of the player based on the direction they face
            float hitboxWidth = 60f; // Adjust based on your katana sprite reach
            float hitboxX = isFacingRight ? position.X + frameRec.Width : position.X - hitboxWidth;
            
            AttackHitbox = new Rectangle(hitboxX, position.Y, hitboxWidth, frameRec.Height);
        }
        else
        {
            // Hide the hitbox when not attacking
            AttackHitbox = new Rectangle(0, 0, 0, 0);
        }
    }

    public void TakeDamage(int damage)
    {
        // Don't take damage if already dead or currently invincible
        if (Health <= 0 || invincibilityTimer > 0) return;

        Health -= damage;
        invincibilityTimer = 1.5f; // 1.5 seconds of invulnerability
        isAttacking = false;       // Cancel current attack
        
        ChangeState(AnimationState.HURT);
    }

    public void Move(int speed)
    {
        bool isMoving = false;

        // Note: You can still move left/right while attacking. 
        // If you want the player to freeze in place while swinging, 
        // wrap this whole movement block in 'if (!isAttacking)'
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
        {
            position.X -= speed;
            isFacingRight = false;
            isMoving = true;
        }
        else if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
        {
            position.X += speed;
            isFacingRight = true;
            isMoving = true;
        }

        // NEW: Only change to RUN or IDLE animations if we aren't currently swinging
        if (!isAttacking)
        {
            if (isMoving)
            {
                ChangeState(AnimationState.RUN);
            }
            else
            {
                ChangeState(AnimationState.IDLE);
            }
        }
    }

    public void Unload()
    {
        foreach (var texture in textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }
    }

    public Vector2 GetPlayerPosition()
    {
        return position;
    }

    public void SetPosition(Vector2 newPos)
    {
        position = newPos;
        Collider = new Rectangle(position.X, position.Y, frameRec.Width, frameRec.Height);
    }
}