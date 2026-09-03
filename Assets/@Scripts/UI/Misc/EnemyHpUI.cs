using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUI : MonoBehaviour
{
    private EnemyCtrl _enemy;
    private Image _hpImage;

    [Header("위치")]
    [SerializeField] private float _heightOffset = 2.5f;

    private void Awake()
    {
        _enemy = gameObject.FindParent<EnemyCtrl>();
        _hpImage = gameObject.FindChild<Image>("HpFill", true);
    }

    private void OnEnable()
    {
        if (_enemy == null)
        {
            return;
        }

        _enemy.OnHpChanged += UpdateHp;
    }

    private void LateUpdate()
    {
        if (_enemy == null)
        {
            return;
        }

        transform.position = _enemy.transform.position + Vector3.up * _heightOffset;
    }

    public void UpdateHp(float currentHp, float maxHp)
    {
        if (_hpImage == null)
        {
            return;
        }

        _hpImage.fillAmount = currentHp / maxHp;

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