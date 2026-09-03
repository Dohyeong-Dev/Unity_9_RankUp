using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertPopup : BasePopup
{
    private Action _yesAction;

    private enum Buttons
    {
        YesButton,
        NoButton
    }

    private enum Texts
    {
        ContentText,
    }

    
    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        Get<Button>(Buttons.YesButton).onClick.AddListener(OnClickYes);
        Get<Button>(Buttons.NoButton).onClick.AddListener(OnClickNo);
    }

    protected override void OnStart()
    {
        Managers.Input.SetCursorLock(false);
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        // Enter → Yes
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnClickYes();
            return;
        }

        // Escape → No
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickNo();
        }
    }

    /// <summary> 팝업 설정 </summary>
    /// <param name="content">팝업 내용</param>
    /// <param name="showCancelButton">Yes/No 버튼을 모두 표시할지 여부</param>
    /// <param name="yesAction">Yes 선택 시 실행할 행동</param>
    public void Set(string content, bool showCancelButton = false, Action yesAction = null)
    {
        _yesAction = yesAction;

        Get<TMP_Text>(Texts.ContentText).text = content;

        // Yes/No 팝업이면 No 버튼 활성화
        Get<Button>(Buttons.NoButton).gameObject.SetActive(showCancelButton);
    }

    private void OnClickYes()
    {
        _yesAction?.Invoke();

        Close();
    }

    private void OnClickNo()
    {
        Close();
    }

    protected override void DestroyOverride()
    {
        if (Managers.Input == null)
        {
            return;
        }
        
        Managers.Input.SetCursorLock(true);
        
        _yesAction = null;
        
        Get<Button>(Buttons.YesButton).onClick.RemoveAllListeners();
        Get<Button>(Buttons.NoButton).onClick.RemoveAllListeners();
    }
}