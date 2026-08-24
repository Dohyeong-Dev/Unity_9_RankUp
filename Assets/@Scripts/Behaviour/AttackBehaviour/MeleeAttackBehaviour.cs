using UnityEngine;

public class MeleeAttackBehaviour : BaseAttackBehaviour
{
    private const int FirstComboStep = 1;
    private const int SecondComboStep = 2;

    private int _meleeComboStep;
    private bool _canCombo;

    [SerializeField] private AttackRange _meleeAttackRange;
    [SerializeField] private LayerMask _meleeLayerMask;

    protected override void OnUpdate()
    {
        if (Managers.Input.MouseDown_Left)
        {
            TryMeleeAttack();
        }
    }

    public void CheckMeleeAttackRange()
    {
        Collider[] colliders = _meleeAttackRange.GetColliders(_meleeLayerMask);

        if (colliders.Length == 0)
        {
            return;
        }

        // 공격이 실제로 적중했을 때만 카메라 흔들림
        Player.Cam.ShakeCamera();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(Mathf.RoundToInt(GetRandomDamage()));
            }
        }
    }

    public void SetCanCombo(bool canCombo)
    {
        _canCombo = canCombo;
    }

    public void Clear()
    {
        Player.UnsetCurAttack();

        _meleeComboStep = 0;
        _canCombo = false;

        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, 0);
    }

    private void TryMeleeAttack()
    {
        if (!Player.IsDefaultBehaviour)
        {
            return;
        }
        
        if (!Player.HasEnoughSp(RequiredSp))
        {
            return;
        }

        // 첫 번째 공격
        if (!Player.IsAttacking)
        {
            StartAttack(FirstComboStep);
            return;
        }

        // 두 번째 공격
        if (_canCombo && _meleeComboStep == FirstComboStep)
        {
            StartAttack(SecondComboStep);
        }
    }

    private void StartAttack(int comboStep)
    {
        Player.SetCurAttack(this);
        Player.SetSp(-RequiredSp);

        _meleeComboStep = comboStep;
        SetCanCombo(false);

        Player.Animator.SetInteger(AnimatorKey.Hash.MeleeComboStep, _meleeComboStep);
    }
}