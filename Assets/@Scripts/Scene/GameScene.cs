using UnityEngine;

public class GameScene : BaseScene
{
    private GameHUD _hud;
    
    [SerializeField] private PlayerCtrl _player;
    public PlayerCtrl Player => _player;
    
    [SerializeField] private CamCtrl _camera;
    public CamCtrl Cam => _camera;
    
    [SerializeField] private SpawnerCtrl _spawnerCtrl;
    public SpawnerCtrl SpawnerCtrl => _spawnerCtrl;
    
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
        Managers.Pool.CreatePool(PoolKey.Path.EnemyMelee, 5);
    }

    protected override void OnUpdate()
    {
    }
}