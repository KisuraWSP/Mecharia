using System.Numerics;

namespace lib.core;

public enum HordeBehavior { Mob, Boss }

public class Horde
{
    public EnemyType Type { get; private set; }
    public EnemyProfile Profile { get; private set; }
    public int Amount { get; private set; }
    public HordeBehavior Behavior { get; private set; }
    public float SpawnInterval { get; private set; }

    public int SpawnedCount { get; private set; } = 0;
    public bool IsFinishedSpawning => SpawnedCount >= Amount;

    public Horde(EnemyType type, EnemyProfile profile, int amount, HordeBehavior behavior, float spawnInterval)
    {
        Type = type;
        Profile = profile;
        Amount = amount;
        Behavior = behavior;
        SpawnInterval = spawnInterval;
    }

    public Enemy SpawnNext(Vector2 position, float runDifficulty)
    {
        SpawnedCount++;

        Enemy newEnemy = new Enemy(position, Profile);

        if (Behavior == HordeBehavior.Boss)
        {
            // Bosses attack with more damage and have heavily scaled health
            // Note: You will need to add an 'AttackDamage' property to Enemy/Profile to fully use this!
            newEnemy.Health = (int)(newEnemy.Health * 2.5f * runDifficulty);
        }
        else if (Behavior == HordeBehavior.Mob)
        {
            // Mob logic: "gang bang them"
            // Example: Mobs get a slight speed boost to swarm the player faster
            newEnemy.Health = (int)(newEnemy.Health * runDifficulty);
            // newEnemy.Speed *= 1.2f; (Requires a public setter for Speed in Enemy.cs)
        }

        return newEnemy;
    }
}