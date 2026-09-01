public abstract class PoolKey
{
    public const string RootPath = "Prefabs/Pool/";

    public static class Name
    {
        public const string EnemyMelee = "EnemyMelee";
        public const string EnemyRange = "EnemyRange";
        public const string EnemyBoss = "EnemyBoss";
    }

    public static class Path
    {
        private const string EnemyPath = RootPath + "Enemy/";
        private const string EffectPath = RootPath + "Effect/";

        public const string EnemyMelee = EnemyPath + Name.EnemyMelee;
        public const string EnemyRange = EnemyPath + Name.EnemyRange;
        public const string EnemyBoss = EnemyPath + Name.EnemyBoss;
    }
}