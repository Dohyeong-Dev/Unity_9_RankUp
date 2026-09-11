using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary> 게임 플레이 씬의 초기화와 페이즈 진행을 관리한다. </summary>
public class GameScene : BaseScene
{
    #region ===== 참조 =====

    private GameHUD _hud;
    public GameHUD HUD => _hud;

    [SerializeField] private PlayerCtrl _player;
    [SerializeField] private CamCtrl _camera;
    
    public PlayerCtrl Player => _player;
    public CamCtrl Cam => _camera;

    #endregion ===== 참조 =====

    #region ===== 페이즈 =====
    
    public int CurrentPhase { get; private set; }
    private const int MaxPhase = 2;
    
    #endregion ===== 페이즈 =====
    
    protected override void OnAwake()
    {
        _hud = GetComponentInChildren<GameHUD>(true);
        
        if (_hud == null)
        {
            CPrint.Error("GameHUD를 찾을 수 없습니다.");
        }
        
        InitializePlayerAndCamera();

        Managers.UI.OpenPopup<StartPopup>();
    }

    protected override void OnStart()
    {
        CreatePool();

        Managers.Event.RaisePhaseUpdated();

        Managers.Event.OnBossClear += OpenClearScreen;
    }

    protected override void OnUpdate()
    {
    }

    private void OnDestroy()
    {
        if (Managers.Event != null)
        {
            Managers.Event.OnBossClear -= OpenClearScreen;
        }
    }

    #region ===== 초기화 =====

    /// <summary> 플레이어와 카메라의 상호 참조를 설정한다. </summary>
    private void InitializePlayerAndCamera()
    {
        if (_player == null)
        {
            CPrint.Error("PlayerCtrl을 찾을 수 없습니다.");
            return;
        }

        if (_camera == null)
        {
            CPrint.Error("CamCtrl을 찾을 수 없습니다.");
            return;
        }

        _player.SetCamera(_camera);
        _camera.SetTarget(_player.transform);
    }

    /// <summary> 게임 플레이에서 사용하는 오브젝트 풀을 생성한다. </summary>
    private void CreatePool()
    {
        Managers.Pool.CreatePool(PoolKey.Path.EnemyMelee, 10);
        Managers.Pool.CreatePool(PoolKey.Path.EnemyRange, 5);
        Managers.Pool.CreatePool(PoolKey.Path.EnemyBoss, 1);
        Managers.Pool.CreatePool(PoolKey.Path.GetProjectilePath(PoolKey.ProjectileType.Fireball), 10);
        Managers.Pool.CreatePool(PoolKey.Path.GetProjectilePath(PoolKey.ProjectileType.Iceball), 10);
        Managers.Pool.CreatePool(PoolKey.Path.GetProjectilePath(PoolKey.ProjectileType.SparkSpear), 5);
        Managers.Pool.CreatePool(PoolKey.Path.PlayerHitEffect, 3);
    }
    
    #endregion ===== 초기화 =====

    #region ===== 페이즈 =====

    /// <summary> 현재 페이즈를 다음 단계로 진행하고 변경 이벤트를 호출한다. </summary>
    public void AdvancePhase()
    {
        CurrentPhase++;

        if (CurrentPhase >= MaxPhase)
        {
            Managers.Event.RaiseBossSpawned();
            CPrint.Success("보스 등장");
        }
        
        Managers.Event.RaisePhaseUpdated();
    }

    public void OpenClearScreen()
    {
        Managers.UI.OpenScreen<EndScreen>().Set(false);
    }
    
    #endregion ===== 페이즈 =====
}