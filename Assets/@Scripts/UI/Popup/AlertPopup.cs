using System;
using TMPro;
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
        ContentsText,
    }

    public enum ContentsType
    {
        None,
        Save
    }

    private ContentsType _contentsType;

    private Action _yesAction;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.YesButton).onClick.AddListener(OnClickYes);
        Get<Button>(Buttons.NoButton).onClick.AddListener(OnClickNo);
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
        else if (Managers.Input.KeyDown_Esc ||
                 (_contentsType == ContentsType.Save && Managers.Input.KeyDown_O))
        {
            OnClickNo();
        }
    }

    protected override void DestroyOverride()
    {
        _yesAction = null;

        Get<Button>(Buttons.YesButton)?.onClick.RemoveAllListeners();
        Get<Button>(Buttons.NoButton)?.onClick.RemoveAllListeners();

        HandleContentsDestroyed();
    }

    /// <summary> 컨텐츠 타입에 따른 Destroy 후처리 </summary>
    private void HandleContentsDestroyed()
    {
        if (Managers.Scene == null)
        {
            return;
        }

        switch (_contentsType)
        {
            case ContentsType.Save:
                if (Managers.Scene.TryGetCurrentScene(out GameScene scene))
                {
                    scene.HUD.SetSideIconActive(GameHUD.SideBar.Save, false);
                }

                break;
        }
    }

    /// <summary> 팝업의 내용과 선택 시 실행할 동작을 설정한다. </summary>
    public void Set(string content, bool showCancelButton = false, Action yesAction = null)
    {
        _yesAction = yesAction;

        Get<TMP_Text>(Texts.ContentsText).text = content;
        Get<Button>(Buttons.NoButton).gameObject.SetActive(showCancelButton);
    }

    /// <summary> 팝업의 내용과 선택 시 실행할 동작을 설정한다. </summary>
    public void Set(ContentsType contentsType)
    {
        _contentsType = contentsType;

        SetContentsAction();
        SetContentsText();

        Get<Button>(Buttons.NoButton).gameObject.SetActive(true);
    }

    /// <summary> ContentsType에 따른 YesAction 설정 </summary>
    private void SetContentsAction()
    {
        _yesAction = _contentsType switch
        {
            ContentsType.Save => () =>
            {
                Managers.UI.OpenToastMessage("저장이 완료되었습니다.");
                Managers.Data.Save();
            },
            _ => null
        };
    }

    /// <summary> ContentsType에 따른 ContentsText 설정 </summary>
    private void SetContentsText()
    {
        Get<TMP_Text>(Texts.ContentsText).text = _contentsType switch
        {
            ContentsType.Save => "저장하시겠습니까?",
            _ => string.Empty
        };
    }

    /// <summary> Yes 버튼을 선택했을 때 팝업을 닫고 지정된 동작을 실행한다. </summary>
    private void OnClickYes()
    {
        Close(() => _yesAction?.Invoke());
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeConfirm);
    }

    /// <summary> No 버튼을 선택했을 때 팝업을 닫는다. </summary>
    private void OnClickNo()
    {
        Close();
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);
    }
}