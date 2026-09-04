using UnityEngine;

public class GameScene : BaseScene
{
    private GameHUD _hud;

    [SerializeField] private PlayerCtrl _player;
    public PlayerCtrl Player => _player;

    [SerializeField] private CamCtrl _camera;
    public CamCtrl Cam => _camera;

    public int CurrentPhase { get; private set; }

    protected override void OnAwake()
    {
        _hud = GetComponentInChildren<GameHUD>(true);
        if (_hud == null)
        {
            CPrint.Error("GameHUD를 찾을 수 없습니다.");
        }

        Managers.UI.OpenPopup<StartPopup>();

        _player.SetCamera(_camera);
        _camera.SetTarget(_player.transform);
    }

    protected override void OnStart()
    {
        CreatePool();

        Managers.Event.RaisePhaseUpdated();
    }

    protected override void OnUpdate()
    {
    }

    private void CreatePool()
    {
        Managers.Pool.CreatePool(PoolKey.Path.EnemyMelee, 10);
        Managers.Pool.CreatePool(PoolKey.Path.PlayerHitEffect, 10);
    }

    public void AdvancePhase()
    {
        CurrentPhase++;

        Managers.Event.RaisePhaseUpdated();
    }
}