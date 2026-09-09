using System.Collections;
using Cinemachine;
using UnityEngine;

/// <summary> 보스 등장 컷신의 시작과 종료, 컷신 캐릭터 및 카메라 연출을 관리한다. </summary>
public class BossEncounter : MonoBehaviour
{
    #region ===== 참조 =====
    
    [Tooltip("컷신 가상 카메라")]
    [SerializeField] private CinemachineVirtualCamera _bossEncounterCamera;
    
    [Tooltip("컷신 카메라")]
    [SerializeField] private CutsceneCamera _cutsceneCamera;

    [Tooltip("컷신 플레이어")]
    [SerializeField] private CutscenePlayer _cutscenePlayer;
    
    [Tooltip("컷신 보스")]
    [SerializeField] private GameObject _cutsceneBoss;
    
    [Tooltip("컷신 프로젝타일")]
    [SerializeField] private GameObject _cutsceneProjectile;
    
    [Tooltip("실제 플레이어")]
    [SerializeField] private PlayerCtrl _player;
    
    #endregion ===== 참조 =====

    #region ===== 상태 =====

    [Header("딜레이 설정")]
    [Tooltip("컷신 시작 딜레이")]
    [SerializeField] private float _cutsceneStartDelay = 1f;

    [Tooltip("컷신 종료 딜레이")]
    [SerializeField] private float _cutsceneEndDelay = 2f;
    
    private Coroutine _cutsceneStartCoroutine;
    private Coroutine _cutsceneFinishCoroutine;

    #endregion ===== 상태 =====

    private void Awake()
    {
        SetCutsceneCameraActive(false);
        SetCutscenePlayerActive(false);
        SetCutsceneBossActive(false);
        SetCutsceneProjectileActive(false);
    }

    private void OnEnable()
    {
        Managers.Event.OnBossSpawned += StartCutscene;

        if (_cutscenePlayer != null)
        {
            _cutscenePlayer.OnBeforeTurn += HandlePlayerTurnStarted;
            _cutscenePlayer.OnTurnFinished += HandlePlayerTurnFinished;
        }

        if (_cutsceneCamera != null)
        {
            _cutsceneCamera.OnBossTargetReached += FinishCutscene;
        }
    }

    private void OnDisable()
    {
        if (Managers.Event != null)
        {
            Managers.Event.OnBossSpawned -= StartCutscene;
        }

        if (_cutscenePlayer != null)
        {
            _cutscenePlayer.OnBeforeTurn -= HandlePlayerTurnStarted;
            _cutscenePlayer.OnTurnFinished -= HandlePlayerTurnFinished;
        }

        if (_cutsceneCamera != null)
        {
            _cutsceneCamera.OnBossTargetReached -= FinishCutscene;
        }

        StopCutsceneCoroutines();
    }

    #region ===== 컷신 =====

    /// <summary> 보스 등장 이벤트 발생 후 설정된 시간만큼 대기한 뒤 컷신을 시작한다. </summary>
    private void StartCutscene()
    {
        StopCutsceneCoroutines();

        _cutsceneStartCoroutine = StartCoroutine(Co_StartCutscene());
    }

    /// <summary> 설정된 시작 대기 시간 후 컷신에 필요한 게임 오브젝트를 활성화한다. </summary>
    private IEnumerator Co_StartCutscene()
    {
        yield return new WaitForSeconds(_cutsceneStartDelay);

        _cutsceneStartCoroutine = null;

        CPrint.Log("[BossEncounter] 컷신 시작");

        Managers.Input.SetInputEnabled(false);
        Managers.UI.CurrentHUD?.SetVisible(false);

        SetRealPlayerActive(false);
        SetCutsceneCameraActive(true);
        SetCutscenePlayerActive(true);

        _cutsceneCamera?.SetCameraTargetToPlayer();
        _cutscenePlayer?.StartCutscene();
    }

    /// <summary> 카메라가 보스 방향의 도착 위치에 도달하면 컷신을 종료한다. </summary>
    private void FinishCutscene()
    {
        StopCutsceneFinishCoroutine();

        _cutsceneFinishCoroutine = StartCoroutine(Co_FinishCutscene());
    }

    /// <summary> 보스 등장 연출 시간을 기다린 후 컷신을 종료한다. </summary>
    private IEnumerator Co_FinishCutscene()
    {
        yield return new WaitForSeconds(_cutsceneEndDelay);

        _cutsceneFinishCoroutine = null;

        CPrint.Log("[BossEncounter] 컷신 종료");

        // 실제 게임용 보스 및 플레이어를 활성화
        SpawnBoss();
        SetRealPlayerActive(true);

        _cutsceneCamera?.StopTargetMove();

        SetCutsceneCameraActive(false);
        SetCutscenePlayerActive(false);
        SetCutsceneBossActive(false);
        SetCutsceneProjectileActive(false);

        Managers.UI.CurrentHUD?.SetVisible(true);
        Managers.Input.SetInputEnabled(true);
    }

    /// <summary> 진행 중인 컷신 시작과 종료 코루틴을 모두 중지한다. </summary>
    private void StopCutsceneCoroutines()
    {
        StopCutsceneStartCoroutine();
        StopCutsceneFinishCoroutine();
    }
    
    /// <summary> 진행 중인 컷신 시작 코루틴을 중지한다. </summary>
    private void StopCutsceneStartCoroutine()
    {
        if (_cutsceneStartCoroutine == null)
        {
            return;
        }

        StopCoroutine(_cutsceneStartCoroutine);
        _cutsceneStartCoroutine = null;
    }

    /// <summary> 진행 중인 컷신 종료 코루틴을 중지한다. </summary>
    private void StopCutsceneFinishCoroutine()
    {
        if (_cutsceneFinishCoroutine == null)
        {
            return;
        }

        StopCoroutine(_cutsceneFinishCoroutine);
        _cutsceneFinishCoroutine = null;
    }
    
    #endregion ===== 컷신 =====

    #region ===== 오브젝트 활성화/비활성화 =====

    /// <summary> 컷신 가상 카메라와 카메라 타겟 관리 오브젝트의 활성 상태를 변경한다. </summary>
    private void SetCutsceneCameraActive(bool isActive)
    {
        if (_bossEncounterCamera != null)
        {
            _bossEncounterCamera.gameObject.SetActive(isActive);
        }

        if (_cutsceneCamera != null)
        {
            _cutsceneCamera.gameObject.SetActive(isActive);
        }
    }
    
    /// <summary> 실제 플레이어의 활성 상태를 변경한다. </summary>
    private void SetRealPlayerActive(bool isActive)
    {
        if (_player == null)
        {
            return;
        }

        if (isActive && _cutscenePlayer != null)
        {
            _player.TeleportToTarget(_cutscenePlayer.transform);
        }
        
        _player.gameObject.SetActive(isActive);
    }

    /// <summary> 컷신용 플레이어의 활성 상태를 변경한다. </summary>
    private void SetCutscenePlayerActive(bool isActive)
    {
        if (_cutscenePlayer == null)
        {
            return;
        }

        _cutscenePlayer.gameObject.SetActive(isActive);
    }

    /// <summary> 컷신용 보스의 활성 상태를 변경한다. </summary>
    private void SetCutsceneBossActive(bool isActive)
    {
        if (_cutsceneBoss == null)
        {
            return;
        }

        _cutsceneBoss.SetActive(isActive);
    }

    /// <summary> 컷신용 프로젝타일의 활성 상태를 변경한다. </summary>
    private void SetCutsceneProjectileActive(bool isActive)
    {
        if (_cutsceneProjectile == null)
        {
            return;
        }

        _cutsceneProjectile.SetActive(isActive);
    }
    
    #endregion ===== 오브젝트 활성화/비활성화 =====

    #region ===== 스폰 =====
    
    /// <summary> 컷신용 보스와 동일한 위치와 회전으로 실제 게임용 보스를 생성한다. </summary>
    private void SpawnBoss()
    {
        if (_cutsceneBoss == null)
        {
            return;
        }

        PoolObj poolObj = Managers.Pool.Get(PoolKey.Path.EnemyBoss);

        if (poolObj == null)
        {
            return;
        }

        poolObj.transform.SetPositionAndRotation(_cutsceneBoss.transform.position, _cutsceneBoss.transform.rotation);
    }
    
    #endregion ===== 스폰 =====
    
    #region ===== 이벤트 =====
    
    /// <summary> 플레이어 회전 연출 전에 현재 카메라 타겟 위치를 고정한다. </summary>
    private void HandlePlayerTurnStarted()
    {
        SetCutsceneProjectileActive(true);
        
        _cutsceneCamera?.SetCameraTargetToTransition();
    }

    /// <summary> 플레이어 회전 연출이 끝난 후 보스 방향으로 카메라 이동을 시작한다. </summary>
    private void HandlePlayerTurnFinished()
    {
        SetCutsceneBossActive(true);
        
        _cutsceneCamera?.MoveToBossTarget();
    }
    
    #endregion ===== 이벤트 =====
}