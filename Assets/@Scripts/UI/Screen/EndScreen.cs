using TMPro;
using UnityEngine.UI;

/// <summary> 게임 종료 또는 게임 클리어 결과를 표시하고 게임 재시작을 처리하는 Screen UI다. </summary>
public class EndScreen : BaseScreen
{
    #region ===== UI 바인딩 =====

    private enum Buttons
    {
        RetryBtn,
    }

    private enum Images
    {
        Bg,
        ClearImage,
        OverImage
    }

    #endregion ===== UI 바인딩 =====

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.RetryBtn).onClick.AddListener(Restart);
        Get<Image>(Images.Bg).gameObject.SetActive(false);
        
        Managers.Sound.StopBgm();
        
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
        Get<Image>(Images.ClearImage).gameObject.SetActive(false);
        Get<Image>(Images.OverImage).gameObject.SetActive(false);
        
        Show(isGameOver);
    }

    /// <summary> 게임 결과에 맞는 화면을 표시한다. </summary>
    private void Show(bool isGameOver)
    {
        if (isGameOver)
        {
            Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.GameOver);
            Get<Image>(Images.OverImage).gameObject.SetActive(true);
        }
        else
        {
            Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.GameClear);
            Get<Image>(Images.ClearImage).gameObject.SetActive(true);
        }
    }

    /// <summary> 현재 게임을 종료하고 처음부터 다시 시작한다. </summary>
    public void Restart()
    {
        Managers.UI.OpenLoadingUI(0.5f, () =>
        {
            Managers.Scene.LoadSceneWithLoading(SceneType.GameScene);
        });
    }
}