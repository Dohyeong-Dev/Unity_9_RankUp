using TMPro;
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

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        Get<Button>(Buttons.ExitButton).onClick.AddListener(ClickExit);
    }

    protected override void OnStart()
    {
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

    protected override void DestroyOverride()
    {
        if (Managers.Scene != null && Managers.Scene.TryGetCurrentScene(out GameScene scene))
        {
            scene.HUD.SetSideIconActive(GameHUD.SideBar.Inventory, false);
        }
    }

    /// <summary> 현재 보유 Gold를 UI에 표시한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldText).text = Managers.Data.Gold.ToString("N0");
    }

    /// <summary> Exit 했을 때 종료소리 재생 및 팝업을 끈다. </summary>
    private void ClickExit()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeConfirm);
        Close();
    }
}