using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    // 현재 씬
    private BaseScene _currentScene;
    
    // 로드될 씬
    private SceneType _nextScene = SceneType.GameScene;
    public SceneType NextScene => _nextScene;

    public void SetCurrentScene(BaseScene scene)
    {
        CPrint.Log($"SetCurrentScene ({scene.GetType().Name})");
        _currentScene = scene;
    }
    
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
    
    public bool IsCurrentScene<T>() where T : BaseScene
    {
        return _currentScene is T;
    }
    
    public bool TryGetCurrentScene<T>(out T scene) where T : BaseScene
    {
        scene = _currentScene as T;

        return scene != null;
    }

    public void ClearCurrentScene()
    {
        _currentScene.Clear();
    }
}