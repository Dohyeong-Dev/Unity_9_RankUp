using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        #region -----INT-----

        public const string MeleeComboStep = "MeleeComboStep";
        
        #endregion -----INT-----
        
        #region -----FLOAT-----

        public const string Speed = "Speed";

        #endregion -----FLOAT-----
        
        #region -----BOOL-----
        
        public const string IsFall = "IsFall";
        public const string IsDash = "IsDash";
        
        #endregion -----BOOL-----
        
        #region -----TRIGGER-----
        
        public const string DoIdleChange = "DoIdleChange";
        public const string DoDie = "DoDie";
        public const string DoCancelSheathe = "DoCancelSheathe";

        #endregion  -----TRIGGER-----
    }

    public static class Hash
    {
        #region -----INT-----
        
        public static readonly int MeleeComboStep = Animator.StringToHash(Parameter.MeleeComboStep);
        
        #endregion -----INT-----
        
        #region -----FLOAT-----
        
        public static readonly int Speed = Animator.StringToHash(Parameter.Speed);
        
        #endregion -----FLOAT-----
        
        #region -----BOOL-----
        
        public static readonly int IsFall = Animator.StringToHash(Parameter.IsFall);
        public static readonly int IsDash = Animator.StringToHash(Parameter.IsDash);
        
        #endregion -----BOOL-----
        
        #region -----TRIGGER-----
        
        public static readonly int DoIdleChange = Animator.StringToHash(Parameter.DoIdleChange);
        public static readonly int DoDie = Animator.StringToHash(Parameter.DoDie);
        public static readonly int DoCancelSheathe = Animator.StringToHash(Parameter.DoCancelSheathe);
        
        #endregion -----TRIGGER-----
    }
}