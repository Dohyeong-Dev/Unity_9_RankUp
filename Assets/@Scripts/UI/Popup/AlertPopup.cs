using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 사용자에게 선택을 요청하고 결과에 따라 지정된 동작을 실행하는 알림 팝업이다. </summary>
public class AlertPopup : BasePopup
{
    private enum Buttons
    {
        YesButton,
        NoButton
    }

    private enum Texts
    {
        ContentText,
    }

    private Action _yesAction;

    private bool _isClicked;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        Get<Button>(Buttons.YesButton).onClick.AddListener(OnClickYes);
        Get<Button>(Buttons.NoButton).onClick.AddListener(OnClickNo);
    }

    protected override void OnStart()
    {
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeAlert);
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

    protected override void DestroyOverride()
    {
        _yesAction = null;

        Get<Button>(Buttons.YesButton)?.onClick.RemoveAllListeners();
        Get<Button>(Buttons.NoButton)?.onClick.RemoveAllListeners();
    }
    
    /// <summary> 팝업의 내용과 선택 시 실행할 동작을 설정한다. </summary>
    public void Set(string content, bool showCancelButton = false, Action yesAction = null)
    {
        _yesAction = yesAction;

        Get<TMP_Text>(Texts.ContentText).text = content;
        Get<Button>(Buttons.NoButton).gameObject.SetActive(showCancelButton);
    }

    /// <summary> Yes 버튼을 선택했을 때 팝업을 닫고 지정된 동작을 실행한다. </summary>
    private void OnClickYes()
    {
        if (_isClicked)
        {
            return;
        }
        
        _isClicked = true;
        Close(() => _yesAction?.Invoke());
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeConfirm);
    }

    /// <summary> No 버튼을 선택했을 때 팝업을 닫는다. </summary>
    private void OnClickNo()
    {
        if (_isClicked)
        {
            return;
        }
        
        _isClicked = true;
        
        Close();
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);
    }
}