using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        public const string Speed = "Speed";
    }

    public static class Hash
    {
        public static readonly int Speed = Animator.StringToHash(Parameter.Speed);
    }
}
