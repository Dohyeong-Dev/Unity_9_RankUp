using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : BaseHUD
{
    private enum Sliders
    {
        HpSlider,
    }
    
    private enum Texts
    {
        TimerTxt,
    }

    public float HpProgress => Get<Slider>(Sliders.HpSlider)?.value ?? 0f;
    
    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Managers.Input.SetCursorLock(true);
    }

    protected override void OnUpdate()
    {
        if (Managers.Scene.CurrentScene.Type == SceneType.GameScene)
        {
            GameScene gameScene = Managers.Scene.CurrentScene as GameScene;

            if (gameScene != null && gameScene.IsStart)
            {
                Get<Slider>(Sliders.HpSlider).value -= Time.deltaTime * 0.08f;

                if (Get<Slider>(Sliders.HpSlider).value <= 0)
                {
                    gameScene.SetStart(false);
                    Managers.UI.OpenScreen<EndScreen>().Open(true);
                }
            }
        }
    }

    public void UpdateTimerText(float remainingTime)
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        Get<TMP_Text>(Texts.TimerTxt).text = $"남은시간 : {minutes:00}:{seconds:00}";
    }

    public void IncreaseHp()
    {
        Get<Slider>(Sliders.HpSlider).value += 0.1f;
    }
}
