using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreen : BaseScreen
{
    private enum Texts
    {
        OverTxt,
        ClearTxt,
    }

    private enum Buttons
    {
        CloseBtn,
    }

    private enum Images
    {
        Bg
    }

    public override void OnInputKey()
    {
    }

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        Get<Button>(Buttons.CloseBtn).onClick.AddListener(Restart);
        Get<Image>(Images.Bg).gameObject.SetActive(false);
    }

    protected override void OnStart()
    {
        Managers.Input.SetCursorLock(false);
    }

    public void Open(bool isGameOver)
    {
        Get<Image>(Images.Bg).gameObject.SetActive(true);

        if (isGameOver)
        {
            Get<TMP_Text>(Texts.ClearTxt).gameObject.SetActive(false);
            Get<TMP_Text>(Texts.OverTxt).gameObject.SetActive(true);
        }
        else
        {
            Get<TMP_Text>(Texts.ClearTxt).gameObject.SetActive(true);
            Get<TMP_Text>(Texts.OverTxt).gameObject.SetActive(false);
        }
    }

    protected override void OnUpdate()
    {
    }

    public void Restart()
    {
        Managers.UI.OpenLoadingUI();

        Managers.Scene.LoadSceneWithLoading(SceneType.GameScene);
    }
}