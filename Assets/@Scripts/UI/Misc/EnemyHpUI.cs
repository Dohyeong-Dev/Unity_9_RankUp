using UnityEngine;
using UnityEngine.UI;

/// <summary> 적의 현재 HP를 UI에 표시한다. </summary>
public class EnemyHpUI : MonoBehaviour
{
    #region ===== 컴포넌트 =====

    private EnemyCtrl _enemy;
    private Image _hpBar;
    private Image _hpImage;

    #endregion ===== 컴포넌트 =====

    #region ===== 설정 =====

    [Header("위치")]
    [SerializeField] private float _heightOffset = 0.15f;

    [Header("HP")]
    [SerializeField] private float _minFillAmount = 0.05f;

    #endregion ===== 설정 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        SubscribeHpChanged();
        UpdatePosition();

        if (_enemy != null)
        {
            UpdateHp(_enemy.Hp, _enemy.MaxHp);
        }
    }

    private void OnDisable()
    {
        UnsubscribeHpChanged();
    }

    #region ===== 초기화 =====

    /// <summary> 필요한 컴포넌트와 적 컨트롤러 참조를 초기화한다. </summary>
    private void InitializeComponents()
    {
        _enemy = gameObject.FindParent<EnemyCtrl>();
        _hpBar = gameObject.FindChild<Image>("HpBar");
        _hpImage = gameObject.FindChild<Image>("HpFill", true);

        if (_enemy == null)
        {
            CPrint.Warning("[EnemyHpUI] EnemyCtrl을 찾을 수 없습니다.");
        }

        if (_hpBar == null)
        {
            CPrint.Warning("[EnemyHpUI] HpBar Image를 찾을 수 없습니다.");
        }
        
        if (_hpImage == null)
        {
            CPrint.Warning("[EnemyHpUI] HpFill Image를 찾을 수 없습니다.");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 이벤트 =====

    /// <summary> 적의 HP 변경 이벤트를 구독한다. </summary>
    private void SubscribeHpChanged()
    {
        if (_enemy == null)
        {
            return;
        }

        _enemy.OnHpChanged -= UpdateHp;
        _enemy.OnHpChanged += UpdateHp;
    }

    /// <summary> 적의 HP 변경 이벤트 구독을 해제한다. </summary>
    private void UnsubscribeHpChanged()
    {
        if (_enemy == null)
        {
            return;
        }

        _enemy.OnHpChanged -= UpdateHp;
    }

    #endregion ===== 이벤트 =====

    #region ===== HP =====

    /// <summary> 현재 HP 비율을 UI에 반영한다. </summary>
    public void UpdateHp(float currentHp, float maxHp)
    {
        if (_hpImage == null || maxHp <= 0f)
        {
            return;
        }

        float ratio = Mathf.Clamp01(currentHp / maxHp);
        float fillAmount = ratio <= 0f ? 0f : Mathf.Max(ratio, _minFillAmount);

        SetHpUIVisible(currentHp > 0f);
        
        _hpImage.fillAmount = fillAmount;
    }

    #endregion ===== HP =====

    #region ===== 위치 =====

    /// <summary> HP UI를 적의 머리 위 위치로 이동한다. </summary>
    private void UpdatePosition()
    {
        if (_enemy == null)
        {
            return;
        }

        transform.position = _enemy.transform.position + Vector3.up * _heightOffset;
    }

    #endregion ===== 위치 =====
    
    #region ===== 활성화/비활성화 =====

    /// <summary> HP UI의 표시 상태를 설정한다. </summary>
    private void SetHpUIVisible(bool isVisible)
    {
        _hpBar.enabled = isVisible;
        _hpImage.enabled = isVisible;
    }
    
    #endregion ===== 활성화/비활성화 =====
}