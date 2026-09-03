using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        #region ===== Int =====

        public const string MeleeComboStep = "MeleeComboStep";
        
        #endregion ===== Int =====
        
        #region ===== Float =====

        public const string Speed = "Speed";
        
        #endregion ===== Float =====
        
        #region ===== Bool =====
        
        public const string IsFall = "IsFall";
        public const string IsDash = "IsDash";
        
        #endregion ===== Bool =====
        
        #region ===== Trigger =====
        
        public const string DoIdleChange = "DoIdleChange";
        public const string DoDie = "DoDie";
        public const string DoCancelSheathe = "DoCancelSheathe";
        public const string DoAttack = "DoAttack";
        public const string DoHit = "DoHit";
        
        #endregion  ===== Trigger =====
    }

    public static class Hash
    {
        #region ===== Int =====
        
        public static readonly int MeleeComboStep = Animator.StringToHash(Parameter.MeleeComboStep);
        
        #endregion ===== Int =====
        
        #region ===== Float =====
        
        public static readonly int Speed = Animator.StringToHash(Parameter.Speed);
        
        #endregion ===== Float =====
        
        #region ===== Bool =====
        
        public static readonly int IsFall = Animator.StringToHash(Parameter.IsFall);
        public static readonly int IsDash = Animator.StringToHash(Parameter.IsDash);

        #endregion ===== Bool =====
        
        #region ===== Trigger =====
        
        public static readonly int DoIdleChange = Animator.StringToHash(Parameter.DoIdleChange);
        public static readonly int DoDie = Animator.StringToHash(Parameter.DoDie);
        public static readonly int DoCancelSheathe = Animator.StringToHash(Parameter.DoCancelSheathe);
        public static readonly int DoAttack = Animator.StringToHash(Parameter.DoAttack);
        public static readonly int DoHit = Animator.StringToHash(Parameter.DoHit);
        
        #endregion ===== Trigger =====
    }
}