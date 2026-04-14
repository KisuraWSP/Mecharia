using System.Numerics;
using Raylib_cs;
using Roy_T.AStar.Grids;      
using Roy_T.AStar.Primitives; 
using Roy_T.AStar.Paths;
using System.Collections.Generic;
using System;

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

    // Pathfinding memory
    private List<Vector2> currentPath = new List<Vector2>();
    private int currentWaypointIndex = 0;
    private float pathRecalculateTimer = 0f;
    
    public Enemy(Vector2 startPosition)
    {
        Position = startPosition;
        Collider = new Rectangle(Position.X, Position.Y, width, height);
    }

    public void Update(Vector2 playerPosition, Grid worldGrid)
    {
        if (IsDead) return;

        // 1. Recalculate path every 0.5 seconds
        pathRecalculateTimer -= Raylib.GetFrameTime();
        if (pathRecalculateTimer <= 0)
        {
            CalculatePath(playerPosition, worldGrid);
            pathRecalculateTimer = 0.5f; 
        }

        // 2. Move along the path waypoints
        if (currentPath != null && currentWaypointIndex < currentPath.Count)
        {
            Vector2 targetWaypoint = currentPath[currentWaypointIndex];
            
            if (Vector2.Distance(Position, targetWaypoint) > 3.0f)
            {
                Vector2 direction = Vector2.Normalize(targetWaypoint - Position);
                Position += direction * speed;
            }
            else
            {
                currentWaypointIndex++; 
            }
        }

        Collider = new Rectangle(Position.X, Position.Y, width, height);
    }

    private void CalculatePath(Vector2 playerPosition, Grid grid)
    {
        int cols = Game.width / Game.CellSize;
        int rows = Game.height / Game.CellSize;

        // Convert Pixel Coordinates to Grid Coordinates
        int startX = (int)(Position.X / Game.CellSize);
        int startY = (int)(Position.Y / Game.CellSize);
        int endX = (int)(playerPosition.X / Game.CellSize);
        int endY = (int)(playerPosition.Y / Game.CellSize);

        // Clamp to map boundaries
        startX = Math.Clamp(startX, 0, cols - 1);
        startY = Math.Clamp(startY, 0, rows - 1);
        endX = Math.Clamp(endX, 0, cols - 1);
        endY = Math.Clamp(endY, 0, rows - 1);

        // v3.0.2: Instantiate a PathFinder and get the path
        var pathFinder = new PathFinder();
        var path = pathFinder.FindPath(new GridPosition(startX, startY), new GridPosition(endX, endY), grid);

        currentPath.Clear();
        currentWaypointIndex = 0;

        // v3.0.2: The path is now a series of Edges if a route was found
        if (path.Type == PathType.Complete)
        {
            foreach (var edge in path.Edges)
            {
                // Because we set our cellSize to 1 meter in Game.cs, 
                // the edge.End.Position corresponds perfectly to our Grid X/Y coordinates.
                float gridX = edge.End.Position.X; 
                float gridY = edge.End.Position.Y; 
                
                // Convert back into world pixels
                float worldX = (gridX * Game.CellSize) + (Game.CellSize / 2f) - (width / 2f);
                float worldY = (gridY * Game.CellSize) + (Game.CellSize / 2f) - (height / 2f);
                
                currentPath.Add(new Vector2(worldX, worldY));
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        Health -= damageAmount;
        // You can add a knockback effect or hit flash here later!
    }

    public void Draw()
    {
        if (IsDead) return;

        Raylib.DrawRectangleRec(Collider, Color.Red);
        
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, width, 5, Color.Black);
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y - 10, (int)(width * (Health / 50f)), 5, Color.Red);

        // Debug laser line to verify pathfinding is working
        if (currentPath != null && currentWaypointIndex < currentPath.Count)
        {
            for (int i = currentWaypointIndex; i < currentPath.Count - 1; i++)
            {
                Vector2 p1 = new Vector2(currentPath[i].X + width/2, currentPath[i].Y + height/2);
                Vector2 p2 = new Vector2(currentPath[i+1].X + width/2, currentPath[i+1].Y + height/2);
                Raylib.DrawLineEx(p1, p2, 2.0f, Color.Green);
            }
        }
    }
}