using UnityEngine;

/// <summary> 페이즈별 시련 시작 지점과 시련 진행 상태를 관리한다. </summary>
public class ForceField : MonoBehaviour
{
    #region ===== 참조 =====

    private PlayerCtrl _player;
    private Renderer _renderer;
    private Spawner _spawner;

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    [Header("마테리얼")]
    [SerializeField] private Material _redMaterial;
    [SerializeField] private Material _blueMaterial;

    [Header("가이드")]
    [SerializeField] private ParticleSystem _arrow;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private int _phase;
    private bool _isStarted;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializePhase();

        if (_renderer == null)
        {
            CPrint.Error("[ForceField] Renderer를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        _renderer.enabled = true;

        SubscribeEvents();
    }

    private void Start()
    {
        TryResolvePlayer();
        UpdateVisualState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagKey.Player) || _isStarted)
        {
            return;
        }

        if (!TryResolvePlayer(other))
        {
            CPrint.Error("[ForceField] Player를 찾을 수 없습니다.");
            return;
        }

        OpenAlertPopup();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    #region ===== 초기화 =====

    /// <summary> ForceField가 사용하는 컴포넌트 참조를 초기화한다. </summary>
    private void InitializeComponents()
    {
        _renderer = GetComponentInChildren<Renderer>(true);
        _spawner = GetComponentInChildren<Spawner>(true);
    }

    /// <summary> Transform 계층 구조를 기준으로 현재 페이즈 번호를 초기화한다. </summary>
    private void InitializePhase()
    {
        _phase = transform.GetSiblingIndex();
    }

    #endregion ===== 초기화 =====

    #region ===== 참조 확인 =====

    /// <summary> 기존 플레이어 참조를 반환하고 없으면 충돌 오브젝트나 현재 씬에서 찾는다. </summary>
    private bool TryResolvePlayer(Collider other = null)
    {
        if (_player != null)
        {
            return true;
        }

        if (other != null && other.TryGetComponent(out PlayerCtrl player))
        {
            _player = player;
            return true;
        }

        if (Managers.Scene != null && Managers.Scene.TryGetCurrentScene(out GameScene gameScene) &&
            gameScene.Player != null)
        {
            _player = gameScene.Player;
            return true;
        }

        return false;
    }

    #endregion ===== 참조 확인 =====

    #region ===== 시련 =====

    /// <summary> 시련 시작 여부를 확인하는 팝업을 표시한다. </summary>
    private void OpenAlertPopup()
    {
        AlertPopup popup = Managers.UI.OpenPopup<AlertPopup>();

        if (popup == null)
        {
            CPrint.Error("[ForceField] AlertPopup을 열 수 없습니다.");
            return;
        }

        popup.Set("시련을 극복하시겠습니까?", true, StartChallenge);
    }

    /// <summary> 시련을 시작하고 플레이어 이동이 완료되면 적 스폰을 시작한다. </summary>
    private void StartChallenge()
    {
        if (_isStarted)
        {
            return;
        }

        if (!TryResolvePlayer())
        {
            CPrint.Error("[ForceField] Player를 찾을 수 없습니다.");
            return;
        }

        if (_spawner == null)
        {
            CPrint.Error("[ForceField] Spawner를 찾을 수 없습니다.");
            return;
        }

        _isStarted = true;

        UpdateVisualState();

        _player.TeleportToTargetWithLoading(_spawner.transform, _spawner.SpawnEnemies);
    }

    /// <summary> 현재 시련을 종료하고 다음 페이즈로 진행한다. </summary>
    private void CompleteChallenge()
    {
        gameObject.SetActive(false);

        if (!Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            CPrint.Error("[ForceField] GameScene을 찾을 수 없습니다.");
            return;
        }

        gameScene.AdvancePhase();
    }

    #endregion ===== 시련 =====

    #region ===== 시각 효과 =====

    /// <summary> 시련 시작 상태에 따라 마테리얼과 가이드 표시를 갱신한다. </summary>
    private void UpdateVisualState()
    {
        if (_renderer != null)
        {
            _renderer.material = _isStarted ? _redMaterial : _blueMaterial;
        }

        if (_arrow != null)
        {
            _arrow.gameObject.SetActive(!_isStarted);
        }
    }

    #endregion ===== 시각 효과 =====

    #region ===== 이벤트 =====

    /// <summary> 필요한 이벤트를 구독한다. </summary>
    private void SubscribeEvents()
    {
        if (_spawner != null)
        {
            _spawner.OnAllEnemiesDead += HandleAllEnemiesDead;
        }
        else
        {
            CPrint.Warning("[ForceField] Spawner를 찾을 수 없습니다.");
        }

        if (Managers.Event != null)
        {
            Managers.Event.OnPhaseUpdated += HandlePhaseUpdated;
        }
    }

    /// <summary> 구독한 이벤트를 해제한다. </summary>
    private void UnsubscribeEvents()
    {
        if (Managers.Event != null)
        {
            Managers.Event.OnPhaseUpdated -= HandlePhaseUpdated;
        }

        if (_spawner != null)
        {
            _spawner.OnAllEnemiesDead -= HandleAllEnemiesDead;
        }
    }

    /// <summary> 현재 페이즈가 변경되면 자신의 활성 상태를 갱신한다. </summary>
    private void HandlePhaseUpdated()
    {
        if (!Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            CPrint.Error("[ForceField] GameScene을 찾을 수 없습니다.");

            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(gameScene.CurrentPhase == _phase);
    }

    /// <summary> 모든 적이 처치되면 현재 시련 완료 처리를 수행한다. </summary>
    private void HandleAllEnemiesDead()
    {
        CompleteChallenge();
    }

    #endregion ===== 이벤트 =====
}