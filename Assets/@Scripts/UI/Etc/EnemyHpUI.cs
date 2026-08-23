using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUI : MonoBehaviour
{
    private Image _hpImage;

    private void Start()
    {
        _hpImage = gameObject.FindChild<Image>("HpFill", true);
        if (_hpImage == null)
        {
            CPrint.Error("HpImage not found");
        }
    }

    public void SetHp(float currentHp, float maxHp)
    {
        if (_hpImage == null)
        {
            return;
        }
        
        _hpImage.fillAmount = currentHp / maxHp;
    }
}
