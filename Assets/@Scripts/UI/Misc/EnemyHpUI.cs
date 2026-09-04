using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUI : MonoBehaviour
{
    private EnemyCtrl _enemy;
    private Image _hpImage;

    [Header("위치")]
    [SerializeField] private float _heightOffset = 0.5f;

    [SerializeField] private float _minFillAmount = 0.05f;
    
    private void Awake()
    {
        _enemy = gameObject.FindParent<EnemyCtrl>();
        _hpImage = gameObject.FindChild<Image>("HpFill", true);
        
        transform.position = _enemy.transform.position + Vector3.up * _heightOffset;
    }

    private void OnEnable()
    {
        if (_enemy == null)
        {
            return;
        }

        _enemy.OnHpChanged += UpdateHp;
    }

    public void UpdateHp(float currentHp, float maxHp)
    {
        if (_hpImage == null)
        {
            return;
        }

        float ratio = currentHp / maxHp;
        float fillAmount = ratio <= 0f ? 0f : Mathf.Max(ratio, _minFillAmount);
        _hpImage.fillAmount = fillAmount;

        if (_hpImage.fillAmount <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (_enemy == null)
        {
            return;
        }

        _enemy.OnHpChanged -= UpdateHp;
    }
}