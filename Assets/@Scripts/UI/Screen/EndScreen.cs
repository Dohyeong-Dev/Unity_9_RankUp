using TMPro;
using UnityEngine.UI;

/// <summary> 게임 종료 또는 게임 클리어 결과를 표시하고 게임 재시작을 처리하는 Screen UI다. </summary>
public class EndScreen : BaseScreen
{
    #region ===== UI 바인딩 =====

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
        Bg,
    }

    #endregion ===== UI 바인딩 =====

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

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
    }

    /// <summary> 게임 종료 여부에 따라 결과 화면을 표시한다. </summary>
    public void Set(bool isGameOver)
    {
        Get<Image>(Images.Bg).gameObject.SetActive(true);

        SetResultText(isGameOver);
    }

    /// <summary> 게임 결과에 맞는 텍스트를 표시한다. </summary>
    private void SetResultText(bool isGameOver)
    {
        Get<TMP_Text>(Texts.OverTxt).gameObject.SetActive(isGameOver);
        Get<TMP_Text>(Texts.ClearTxt).gameObject.SetActive(!isGameOver);
    }

    /// <summary> 현재 게임을 종료하고 처음부터 다시 시작한다. </summary>
    public void Restart()
    {
        Managers.UI.OpenLoadingUI();
        Managers.Scene.LoadSceneWithLoading(SceneType.GameScene);
    }
}