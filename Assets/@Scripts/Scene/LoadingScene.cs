using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : BaseScene
{
    private LoadingHUD _loadingHUD;
    
    protected override void OnAwake()
    {
        _loadingHUD = GetComponentInChildren<LoadingHUD>(true);
        if (_loadingHUD == null)
        {
            CPrint.Error("LoadingHUD를 찾을 수 없습니다.");
        }
    }

    protected override void OnStart()
    {
        if (Managers.Scene.NextScene == ScenesManager.SceneType.None)
        {
            CPrint.Error("LoadingScene의 NextScene이 설정되지 않았습니다.");
            return;
        }

        StartCoroutine(LoadSceneAsync(Managers.Scene.NextScene));
    }

    protected override void OnUpdate()
    {
    }

    private IEnumerator LoadSceneAsync(ScenesManager.SceneType nextScene)
    {
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene.ToString());
        if (operation == null)
        {
            CPrint.Error($"Scene을 비동기로 로드하지 못했습니다. [{nextScene}]");
            yield break;
        }

        operation.allowSceneActivation = false;

        // 실제 Scene 로딩 진행
        while (operation.progress < 0.9f)
        {
            UpdateLoadingProgress(operation.progress);
            yield return null;
        }

        // 실제 로딩은 완료되었지만 UI 게이지를 100%까지 자연스럽게 채운다.
        while (_loadingHUD != null && _loadingHUD.LoadingProgress < 1f)
        {
            UpdateLoadingProgress(1f);
            yield return null;
        }

        // 사용자가 계속 진행할 때까지 대기
        while (!CheckLoadable())
        {
            yield return null;
        }

        Managers.Scene.CurrentScene.Clear();

        operation.allowSceneActivation = true;
    }

    private void UpdateLoadingProgress(float progress)
    {
        if (_loadingHUD == null)
        {
            return;
        }

        float normalizedProgress = Mathf.Clamp01(progress / 0.9f);
        float currentProgress = _loadingHUD.LoadingProgress;
        float nextProgress = Mathf.MoveTowards(currentProgress, normalizedProgress, Time.deltaTime);

        _loadingHUD.SetLoadingProgress(nextProgress);
    }

    private bool CheckLoadable()
    {
        switch (Managers.Scene.NextScene)
        {
            case ScenesManager.SceneType.GameScene:
                return _loadingHUD?.ContinueRequested ?? false;

            default:
                return false;
        }
    }
}