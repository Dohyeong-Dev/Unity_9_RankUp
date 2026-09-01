using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUI : MonoBehaviour
{
    private EnemyCtrl _enemy;
    private Image _hpImage;

    private void Awake()
    {
        _enemy = gameObject.FindParent<EnemyCtrl>();
    }

    private void Start()
    {
        _hpImage = gameObject.FindChild<Image>("HpFill", true);
        if (_hpImage == null)
        {
            CPrint.Error("HpImage not found");
            return;
        }

        if (_enemy == null)
        {
            CPrint.Log("enemy not found");
            return;
        }
        
        //_enemy.OnHpChanged += UpdateHp;
    }
    
    public void UpdateHp(float currentHp, float maxHp)
    {
        _hpImage.fillAmount = currentHp / maxHp;
    }
    
    private void OnDestroy()
    {
        if (_enemy == null)
        {
            return;
        }

        //_enemy.OnHpChanged -= UpdateHp;
    }
}
