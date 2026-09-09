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
        public const string DoCancelSheathe = "DoCancelSheathe";
        public const string DoAttack = "DoAttack";
        public const string DoHit = "DoHit";

        #endregion ===== Trigger =====
    }

    public static class Name
    {
        public const string Hit1 = "Hit1";
        public const string Hit2 = "Hit2";
        public const string Die = "Die";

        #region ===== 컷신 =====

        public const string Idle = "Idle";
        public const string Walk = "Walk";
        public const string Evade = "Evade";
        public const string TurnR = "TurnR";

        #endregion ===== 컷신 =====
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
        public static readonly int DoCancelSheathe = Animator.StringToHash(Parameter.DoCancelSheathe);
        public static readonly int DoAttack = Animator.StringToHash(Parameter.DoAttack);
        public static readonly int DoHit = Animator.StringToHash(Parameter.DoHit);

        #endregion ===== Trigger =====

        #region ===== State =====

        public static readonly int Hit1 = Animator.StringToHash(Name.Hit1);
        public static readonly int Hit2 = Animator.StringToHash(Name.Hit2);
        public static readonly int Die = Animator.StringToHash(Name.Die);

        public static readonly int Idle = Animator.StringToHash(Name.Idle);
        public static readonly int Walk = Animator.StringToHash(Name.Walk);
        public static readonly int Evade = Animator.StringToHash(Name.Evade);
        public static readonly int TurnR = Animator.StringToHash(Name.TurnR);

        #endregion ===== State =====
    }
}