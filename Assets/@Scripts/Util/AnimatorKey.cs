using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        public const string Speed = "Speed";
        public const string IsGround = "IsGround";
        public const string IsJump = "IsJump";
        public const string IsDash = "IsDash";
        public const string DoIdleChange = "DoIdleChange";
        public const string DoDie = "DoDie";
    }

    public static class Hash
    {
        public static readonly int Speed = Animator.StringToHash(Parameter.Speed);
        public static readonly int IsGround = Animator.StringToHash(Parameter.IsGround);
        public static readonly int IsJump = Animator.StringToHash(Parameter.IsJump);
        public static readonly int IsDash = Animator.StringToHash(Parameter.IsDash);
        public static readonly int DoIdleChange = Animator.StringToHash(Parameter.DoIdleChange);
        public static readonly int DoDie = Animator.StringToHash(Parameter.DoDie);
    }
}