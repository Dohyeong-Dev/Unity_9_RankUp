using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPopup : BasePopup
{
    public enum Buttons
    {
        ExitButton
    }

    public enum Texts
    {
        GoldText
    }

    public enum Toggles
    {
        EquipmentToggle,
        ConsumableToggle,
    }

    private GameObject _quickBar;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Toggle>(typeof(Toggles));

        ResolveQuickBar();
        SetQuickBarActive(false);
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.ExitButton).onClick.AddListener(ClickExit);
        Get<Toggle>(Toggles.EquipmentToggle).onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.Button);
            }
            
        });
        Get<Toggle>(Toggles.ConsumableToggle).onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.Button);
            }
        });

        UpdateGoldText();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc || Managers.Input.KeyDown_I)
        {
            ClickExit();
        }
    }

    protected override void OnOpened()
    {
        base.OnOpened();

        SetQuickBarActive(true);
    }

    protected override void DestroyOverride()
    {
        if (Managers.Scene != null && Managers.Scene.TryGetCurrentScene(out GameScene scene))
        {
            scene.HUD.SetSideIconActive(GameHUD.SideBar.Inventory, false);
        }
    }

    /// <summary> QuickBar 참조를 확보 후 비활성화 한다. </summary>
    private void ResolveQuickBar()
    {
        _quickBar = gameObject.FindChild<HorizontalLayoutGroup>("QuickBar").gameObject;
        if (_quickBar == null)
        {
            CPrint.Log("[InventoryPopup] Can't find QuickBar");
        }
    }

    private void SetQuickBarActive(bool value)
    {
        if (_quickBar == null)
        {
            return;
        }

        _quickBar.SetActive(value);
    }

    /// <summary> 현재 보유 Gold를 UI에 표시한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldText).text = Managers.Data.Gold.ToString("N0");
    }

    /// <summary> Exit 했을 때 종료소리 재생 및 팝업을 끈다. </summary>
    private void ClickExit()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);
        SetQuickBarActive(false);

        Close();
    }
}