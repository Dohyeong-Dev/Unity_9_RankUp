using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> 현재 게임 씬을 관리하고 로딩 씬을 통한 씬 전환을 처리한다. </summary>
public class ScenesManager : MonoBehaviour
{
    #region ===== 현재 씬 =====

    private BaseScene _currentScene;

    #endregion ===== 현재 씬 =====

    #region ===== 다음 씬 =====

    private SceneType _nextScene = SceneType.GameScene;
    public SceneType NextScene => _nextScene;

    #endregion ===== 다음 씬 =====

    #region ===== 씬 관리 =====

    /// <summary> 현재 활성화된 BaseScene을 등록한다. </summary>
    public void SetCurrentScene(BaseScene scene)
    {
        if (scene == null)
        {
            CPrint.Error("등록할 BaseScene이 null입니다.");
            return;
        }

        CPrint.Log($"SetCurrentScene ({scene.GetType().Name})");

        _currentScene = scene;
    }

    /// <summary> 로딩 씬을 거쳐 지정된 씬으로 전환한다. </summary>
    public void LoadSceneWithLoading(SceneType sceneType)
    {
        if (_currentScene == null)
        {
            CPrint.Error("현재 Scene에 BaseScene이 존재하지 않습니다.");
            return;
        }

        _nextScene = sceneType;

        _currentScene.Clear();

        SceneManager.LoadScene(nameof(SceneType.LoadingScene));
    }

    /// <summary> 현재 씬이 지정한 BaseScene 타입인지 확인한다. </summary>
    public bool IsCurrentScene<T>() where T : BaseScene
    {
        return _currentScene is T;
    }

    /// <summary> 현재 씬을 지정한 타입으로 반환한다. </summary>
    public bool TryGetCurrentScene<T>(out T scene) where T : BaseScene
    {
        scene = _currentScene as T;
        return scene != null;
    }

    /// <summary> 현재 등록된 씬을 정리한다. </summary>
    public void ClearCurrentScene()
    {
        if (_currentScene == null)
        {
            return;
        }

        _currentScene.Clear();
    }

    #endregion ===== 씬 관리 =====
}