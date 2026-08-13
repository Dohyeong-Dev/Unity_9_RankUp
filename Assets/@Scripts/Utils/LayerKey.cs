public abstract class LayerKey
{
    public static class Idx
    {
        public const int Default = 0;
        public const int TransprentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Floor = 3;
        public const int Water = 4;
        public const int UI = 5;
        public const int Wall = 6;
    }

    public static class Mask
    {
        public const int Default = 1 << Idx.Default;
        public const int TransprentFX = 1 << Idx.TransprentFX;
        public const int IgnoreRaycast = 1 << Idx.IgnoreRaycast;
        public const int Floor = 1 << Idx.Floor;
        public const int Water = 1 << Idx.Water;
        public const int UI = 1 << Idx.UI;
        public const int Wall = 1 << Idx.Wall;
    }
}
