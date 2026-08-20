using UnityEngine;

public class GameScene : BaseScene
{
    private GameHUD _hud;
    public GameHUD HUD => _hud;
    
    [SerializeField] private PlayerCtrl _player;
    [SerializeField] private CamCtrl _camera;

    private float _remainingTime = 20f;
    private int _lastDisplayedSecond = -1;

    private bool _isStart;
    public bool IsStart => _isStart;
    
    protected override void OnAwake()
    {
        _hud = GetComponentInChildren<GameHUD>(true);
        if (_hud == null)
        {
            CPrint.Error("GameHUD를 찾을 수 없습니다.");
        }
        Managers.UI.OpenPopup<StartPopup>();

        _player.SetCamera(_camera.GetComponent<Camera>());
        _camera.SetTarget(_player.transform);
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdate()
    {
        if (!_isStart)
        {
            return;
        }
        
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 0f)
        {
            SetStart(false);
            _remainingTime = 0f;
            _hud.UpdateTimerText(_remainingTime);
            Managers.UI.OpenScreen<EndScreen>().Open(_hud.HpProgress <= 0);

            return;
        }

        int currentSecond = Mathf.CeilToInt(_remainingTime);
        if (currentSecond != _lastDisplayedSecond)
        {
            _lastDisplayedSecond = currentSecond;
            _hud.UpdateTimerText(_remainingTime);
        }
    }

    public void SetStart(bool isStart)
    {
        _isStart = isStart;
    }
}