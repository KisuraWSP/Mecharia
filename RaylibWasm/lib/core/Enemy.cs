using System.Numerics;
using Raylib_cs;

namespace lib.core;

public class Enemy
{
    public int Health { get; private set; } = 50;
    public Vector2 Position { get; private set; }
    public Rectangle Collider { get; private set; }
    public bool IsDead => Health <= 0;

    private float speed = 2.0f;
    private int width = 40;
    private int height = 40;

    public Enemy(Vector2 startPosition)
    {
        Position = startPosition;
        Collider = new Rectangle(Position.X, Position.Y, width, height);
    }

    public void Update(Vector2 playerPosition)
    {
        if (IsDead) return;

        // Basic AI: Move towards the player
        Vector2 direction = Vector2.Normalize(playerPosition - Position);
        
        // Prevent twitching if the enemy is exactly on the player
        if (Vector2.Distance(Position, playerPosition) > 2.0f)
        {
            Position += direction * speed;
        }

        // Update collider position
        Collider = new Rectangle(Position.X, Position.Y, width, height);
    }

    public void TakeDamage(int damageAmount)
    {
        Health -= damageAmount;
        // You can add a knockback effect or hit flash here later!
    }

    public void Draw()
    {
        if (IsDead) return;

        // Drawing a red box for the enemy until you add a sprite
        Raylib.DrawRectangleRec(Collider, Color.Red);
        
        // Simple health bar above the enemy
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, width, 5, Color.Black);
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, (int)(width * (Health / 50f)), 5, Color.Red);
    }
}