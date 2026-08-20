using UnityEngine;

public class GameScene : BaseScene
{
    private GameHUD _gameHUD;
    
    [SerializeField] private PlayerCtrl _player;
    [SerializeField] private CamCtrl _camera;

    protected override void OnAwake()
    {
        _gameHUD = GetComponentInChildren<GameHUD>(true);
        if (_gameHUD == null)
        {
            CPrint.Error("GameHUD를 찾을 수 없습니다.");
        }
        Managers.UI.OpenPopup<StartPopup>();
        
        Managers.Input.SetCursorLock(true);

        _player.SetCamera(_camera.GetComponent<Camera>());
        _camera.SetTarget(_player.transform);
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdate()
    {
    }
}