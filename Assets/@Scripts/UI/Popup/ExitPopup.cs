using UnityEngine;
using UnityEngine.UI;

public class ExitPopup : BasePopup
{
    private enum Buttons
    {
        YesButton,
        NoButton
    }

    private enum Sliders
    {
        BgmSlider,
        SfxSlider
    }

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Slider>(typeof(Sliders));
        
        Get<Button>(Buttons.YesButton).onClick.AddListener(OnClickYes);
        Get<Button>(Buttons.NoButton).onClick.AddListener(OnClickNo);
    }

    protected override void OnStart()
    {
        UpdateSoundSliderValue();
        SubscribeVolumeEvent();
        
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimePopup);
    }
    
    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (!_graphicRaycaster.isActiveAndEnabled)
        {
            return;
        }
        
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
        Managers.Event.RaisePause();
    }
    
    protected override void DestroyOverride()
    {
        Time.timeScale = 1f;

        if (Managers.Event != null)
        {
            Managers.Event.RaiseResume();
        }
        
        UnsubscribeSliderEvent();
        
        if (Managers.Scene != null && Managers.Scene.TryGetCurrentScene(out GameScene scene))
        {
            scene.HUD.SetSideIconActive(GameHUD.SideBar.Setting, false);
        }
    }
    
    /// <summary> 사운드 슬라이더 값을 사운드 매니저의 볼륨값으로 업데이트 </summary>
    private void UpdateSoundSliderValue()
    {
        Get<Slider>(Sliders.BgmSlider).value = Managers.Sound.GetBgmVolume();
        Get<Slider>(Sliders.SfxSlider).value = Managers.Sound.GetSfxVolume();
    }
    
    /// <summary> Yes 버튼을 선택했을 때 종료한다. </summary>
    private void OnClickYes()
    {
        Utils.QuitApp();
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeConfirm);
    }

    /// <summary> No 버튼을 선택했을 때 팝업을 닫는다. </summary>
    private void OnClickNo()
    {
        Close();
        
        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.ChimeCancel);
    }
    
    #region ===== 이벤트 =====
    
    /// <summary> 슬라이더 이벤트에 사운드 매니저의 볼륨 조절 함수를 구독시킨다. </summary>
    private void SubscribeVolumeEvent()
    {
        Get<Slider>(Sliders.BgmSlider).onValueChanged.AddListener(HandleBgmVolumeChanged);
        Get<Slider>(Sliders.SfxSlider).onValueChanged.AddListener(HandleSfxVolumeChanged);
    }

    /// <summary> 슬라이더 이벤트에 구독된 이벤트들을 전부 구독해제시킨다. </summary>
    private void UnsubscribeSliderEvent()
    {
        Get<Slider>(Sliders.BgmSlider).onValueChanged.RemoveAllListeners();
        Get<Slider>(Sliders.SfxSlider).onValueChanged.RemoveAllListeners();
    }
    
    /// <summary> 사운드 매니저의 BGM 볼륨 세팅 </summary>
    private void HandleBgmVolumeChanged(float value)
    {
        Managers.Sound.SetBgmVolume(value);
    }
    
    /// <summary> 사운드 매니저의 SFX 볼륨 세팅 </summary>
    private void HandleSfxVolumeChanged(float value)
    {
        Managers.Sound.SetSfxVolume(value);
    }
    
    #endregion ===== 이벤트 =====
}
