using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void SetLoadingProgress(float progress)
    {
        Get<Slider>(Sliders.LoadingSlider).value = Mathf.Clamp01(progress);

        if (Get<Slider>(Sliders.LoadingSlider).value >= 1f)
        {
            ShowContinueMessage();
        }
    }

    private void ShowContinueMessage()
    {
        if (Get<TMP_Text>(Texts.AlertTxt).gameObject.activeSelf)
        {
            return;
        }

        Get<Slider>(Sliders.LoadingSlider).gameObject.SetActive(false);
        Get<TMP_Text>(Texts.AlertTxt).gameObject.SetActive(true);
    }
}