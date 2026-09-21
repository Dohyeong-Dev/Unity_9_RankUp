using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> 로딩 화면을 표시하고 다음 Scene을 비동기로 로드한다. </summary>
public class LoadingScene : BaseScene
{
    #region ===== 참조 =====

    private LoadingHUD _hud;

    #endregion ===== 참조 =====

    private bool _isContinueRequested = false;
    
    protected override void OnAwake()
    {
        _hud = GetComponentInChildren<LoadingHUD>(true);

        if (_hud == null)
        {
            CPrint.Error("LoadingHUD를 찾을 수 없습니다.");
        }
    }

    protected override void OnStart()
    {
        if (Managers.Scene.NextScene == SceneType.None)
        {
            CPrint.Error("LoadingScene의 NextScene이 설정되지 않았습니다.");
            return;
        }

        StartCoroutine(LoadSceneAsync(Managers.Scene.NextScene));
    }

    protected override void OnUpdate()
    {
    }

    #region ===== Scene 로딩 =====

    /// <summary> 지정된 Scene을 비동기로 로드하고 로딩 진행도를 HUD에 반영한다. </summary>
    private IEnumerator LoadSceneAsync(SceneType nextScene)
    {
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene.ToString());

        if (operation == null)
        {
            CPrint.Error($"Scene을 비동기로 로드하지 못했습니다. [{nextScene}]");
            yield break;
        }

        operation.allowSceneActivation = false;

        yield return LoadSceneProgressAsync(operation);
        yield return FillLoadingProgressAsync();

        while (!_isContinueRequested)
        {
            yield return null;
        }

        Managers.Scene.ClearCurrentScene();

        operation.allowSceneActivation = true;
    }

    /// <summary> 실제 Scene 로딩 진행도를 LoadingHUD에 반영한다. </summary>
    private IEnumerator LoadSceneProgressAsync(AsyncOperation operation)
    {
        while (operation.progress < 0.9f)
        {
            UpdateLoadingProgress(operation.progress);
            yield return null;
        }
    }

    /// <summary> 실제 로딩 완료 후 LoadingHUD 게이지를 100%까지 채운다. </summary>
    private IEnumerator FillLoadingProgressAsync()
    {
        while (_hud != null && _hud.LoadingProgress < 1f)
        {
            UpdateLoadingProgress(1f);
            yield return null;
        }
    }

    /// <summary> 현재 로딩 진행도를 HUD에 반영한다. </summary>
    private void UpdateLoadingProgress(float progress)
    {
        if (_hud == null)
        {
            return;
        }

        float targetProgress = Mathf.Clamp01(progress / 0.9f);
        float nextProgress = Mathf.MoveTowards(_hud.LoadingProgress, targetProgress, Time.deltaTime);

        _hud.SetLoadingProgress(nextProgress);
    }

    #endregion ===== Scene 로딩 =====

    public void SetContinueRequested(bool isContinueRequested)
    {
        _isContinueRequested = isContinueRequested;
    }
}