using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    // 현재 씬
    private BaseScene _currentScene;
    public BaseScene CurrentScene => _currentScene;
    public SceneType CurrentSceneType => CurrentScene.Type;
    
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
        if (CurrentScene == null)
        {
            CPrint.Error("현재 Scene에 BaseScene이 존재하지 않습니다.");
            return;
        }
        
        _nextScene = sceneType;
        CurrentScene.Clear();

        SceneManager.LoadScene(nameof(SceneType.LoadingScene));
    }
}