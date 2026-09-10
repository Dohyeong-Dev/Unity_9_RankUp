using UnityEngine;
using UnityEngine.UI;

public class ExitPopup : BasePopup
{
    private enum Buttons
    {
        YesButton,
        NoButton
    }

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        
        Get<Button>(Buttons.YesButton).onClick.AddListener(OnClickYes);
        Get<Button>(Buttons.NoButton).onClick.AddListener(OnClickNo);
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Enter)
        {
            OnClickYes();
        }
        else if (Managers.Input.KeyDown_Esc)
        {
            OnClickNo();
        }
    }

    protected override void OnOpened()
    {
        Time.timeScale = 0f;
    }
    
    protected override void DestroyOverride()
    {
    }
    
    /// <summary> Yes 버튼을 선택했을 때 종료한다. </summary>
    private void OnClickYes()
    {
        Utils.QuitApp();
    }

    /// <summary> No 버튼을 선택했을 때 팝업을 닫는다. </summary>
    private void OnClickNo()
    {
        Time.timeScale = 1f;
        Close();
    }
}
