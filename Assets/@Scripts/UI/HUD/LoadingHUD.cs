using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 씬 로딩 진행률을 표시하고 다음 씬으로 진행할 입력을 처리하는 HUD다. </summary>
public class LoadingHUD : BaseHUD
{
    private enum Sliders
    {
        LoadingSlider,
    }

    private enum Texts
    {
        AlertTxt,
    }

    public float LoadingProgress => Get<Slider>(Sliders.LoadingSlider).value;

    public bool ContinueRequested { get; private set; }

    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));

        Get<TMP_Text>(Texts.AlertTxt).gameObject.SetActive(false);
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
        if (LoadingProgress < 1f)
        {
            return;
        }

        if (Managers.Input.MouseDown_Left)
        {
            ContinueRequested = true;
        }
    }

    /// <summary> 로딩 진행률을 설정하고 완료 시 계속 진행 안내를 표시한다. </summary>
    public void SetLoadingProgress(float progress)
    {
        Slider loadingSlider = Get<Slider>(Sliders.LoadingSlider);
        loadingSlider.value = Mathf.Clamp01(progress);

        if (loadingSlider.value >= 1f)
        {
            ShowContinueMessage();
        }
    }

    /// <summary> 로딩 완료 후 계속 진행할 수 있다는 안내를 표시한다. </summary>
    private void ShowContinueMessage()
    {
        TMP_Text alertText = Get<TMP_Text>(Texts.AlertTxt);

        if (alertText.gameObject.activeSelf)
        {
            return;
        }

        Get<Slider>(Sliders.LoadingSlider).gameObject.SetActive(false);
        alertText.gameObject.SetActive(true);
    }
}