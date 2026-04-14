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
    private int currentFrame = 0;
    private int frameCounter = 0;
    private int frameSpeed = 8;
    private Dictionary<AnimationState, Texture2D> textures;
    private Dictionary<AnimationState, int> frameCounts;
    private AnimationState animationState = AnimationState.IDLE;
    private Vector2 position;
    private Rectangle frameRec;
    private bool isFacingRight = true;

    private static string baseDir = AppContext.BaseDirectory;
    private static string resourcesDir = Path.Combine(baseDir, "Resources", "sprite", "player");

    public Player(Vector2 _position)
    {
        position = _position;
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
        
        UpdateFrameRectangle();
        Collider = new Rectangle(position.X, position.Y, 50, 50);
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
        frameCounter++;

        if (frameCounter >= (Game.fps/frameSpeed))
        {
            frameCounter = 0;
            currentFrame++;

            if (currentFrame >= frameCounts[animationState]) 
            {
                currentFrame = 0;
            }
            frameRec.X = (float)currentFrame * (float)frameRec.Width;
        }
        
        Collider = new Rectangle(position.X, position.Y, frameRec.Width, frameRec.Height);
    }

    public void Draw()
    {
        Rectangle drawRec = frameRec;

        if (!isFacingRight)
        {
            drawRec.Width = -drawRec.Width;
        }
        
        Raylib.DrawTextureRec(textures[animationState], drawRec, position, Color.White);
    }

    public void Move(int speed)
    {
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
        {
            position.X -= speed;
            isFacingRight = false;
            ChangeState(AnimationState.RUN);
        }
        else if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
        {
            position.X += speed;
            isFacingRight = true;
            ChangeState(AnimationState.RUN);
        }

        if (Raylib.IsKeyReleased(KeyboardKey.A) || Raylib.IsKeyReleased(KeyboardKey.D) || Raylib.IsKeyReleased(KeyboardKey.Left) || Raylib.IsKeyReleased(KeyboardKey.Right))
        {
            ChangeState(AnimationState.IDLE);
            speed = 0;
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
}