using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        public const string Speed = "Speed";
        public const string IsGround = "IsGround";
        public const string IsJump = "IsJump";
        public const string IsFall = "IsFall";
    }

    public static class Hash
    {
        public static readonly int Speed = Animator.StringToHash(Parameter.Speed);
        public static readonly int IsGround = Animator.StringToHash(Parameter.IsGround);
        public static readonly int IsJump = Animator.StringToHash(Parameter.IsJump);
        public static readonly int IsFall = Animator.StringToHash(Parameter.IsFall);
    }
}