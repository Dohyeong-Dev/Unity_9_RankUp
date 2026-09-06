/// <summary> 외부로부터 데미지를 받고 사망 처리를 수행할 수 있는 객체를 정의한다. </summary>
public interface IDamageable
{
    /// <summary> 현재 사망 상태인지 반환한다. </summary>
    bool IsDead { get; }

    /// <summary> 지정된 데미지를 적용한다. </summary>
    void TakeDamage(float damage);

    /// <summary> 사망 처리를 수행한다. </summary>
    void Die();
}