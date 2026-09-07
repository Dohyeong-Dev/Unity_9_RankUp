public abstract class PoolKey
{
    private const string RootPath = "Prefab/Pool/";

    public enum ProjectileType
    {
        Fireball,
        Iceball,
    }

    public static class Path
    {
        private const string ProjectilePath = RootPath + "Projectile/";

        /// <summary> 지정된 발사체 타입의 Pool 경로를 반환한다. </summary>
        public static string GetProjectilePath(ProjectileType projectileType)
        {
            return ProjectilePath + projectileType;
        }

        public const string EnemyMelee = RootPath + "Enemy/EnemyMelee";
        public const string EnemyRange = RootPath + "Enemy/EnemyRange";
        public const string EnemyBoss = RootPath + "Enemy/EnemyBoss";

        public const string PlayerHitEffect = RootPath + "Effect/PlayerHitEffect";
    }
}