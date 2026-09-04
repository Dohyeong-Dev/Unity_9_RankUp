using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        #region ===== Int =====

        public const string MeleeComboStep = "MeleeComboStep";
        public const string AttackIndex = "AttackIndex";

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
        public const string DoHit1 = "DoHit1";

        #endregion ===== Trigger =====
    }

    public static class Name
    {
        public const string Hit1 = "Hit1";
        public const string Hit2 = "Hit2";
        public const string Die = "Die";
    }
    
    public static class Layer
    {
        public const string BaseLayer = "BaseLayer";
        public const string Locomotion = "Locomotion";
        public const string MeleeAttack = "MeleeAttack";
        public const string Reaction = "Reaction";
    }
    
    public static class Hash
    {
        #region ===== Int =====

        public static readonly int MeleeComboStep = Animator.StringToHash(Parameter.MeleeComboStep);
        public static readonly int AttackIndex = Animator.StringToHash(Parameter.AttackIndex);

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

        #region ===== Name =====

        public static readonly int Hit1 = Animator.StringToHash(Name.Hit1);
        public static readonly int Hit2 = Animator.StringToHash(Name.Hit2);
        public static readonly int Die = Animator.StringToHash(Name.Die);

        #endregion ===== Name =====
    }
}