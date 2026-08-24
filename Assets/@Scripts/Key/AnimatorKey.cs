using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        #region int

        public const string MeleeComboStep = "MeleeComboStep";
        
        #endregion
        
        #region float

        public const string Speed = "Speed";

        #endregion
        
        #region bool
        
        public const string IsGround = "IsGround";
        public const string IsJump = "IsJump";
        public const string IsDash = "IsDash";
        
        #endregion
        
        #region trigger
        
        public const string DoIdleChange = "DoIdleChange";
        public const string DoDie = "DoDie";

        #endregion
    }

    public static class Hash
    {
        #region int
        
        public static readonly int MeleeComboStep = Animator.StringToHash(Parameter.MeleeComboStep);
        
        #endregion
        
        #region float
        
        public static readonly int Speed = Animator.StringToHash(Parameter.Speed);
        
        #endregion
        
        #region bool
        
        public static readonly int IsGround = Animator.StringToHash(Parameter.IsGround);
        public static readonly int IsJump = Animator.StringToHash(Parameter.IsJump);
        public static readonly int IsDash = Animator.StringToHash(Parameter.IsDash);
        
        #endregion
        
        #region trigger
        
        public static readonly int DoIdleChange = Animator.StringToHash(Parameter.DoIdleChange);
        public static readonly int DoDie = Animator.StringToHash(Parameter.DoDie);
        
        #endregion
    }
}