using UnityEngine;

public class ForceField : MonoBehaviour
{
    [Header("마테리얼")]
    [SerializeField] private Material _redMaterial;
    [SerializeField] private Material _blueMaterial;

    [Header("가이드")]
    [SerializeField] private ParticleSystem _arrow;
    
    private PlayerCtrl _player;

    private Renderer _renderer;

    private Spawner _spawner;
    
    private int _phase;
    
    private bool _isStarted;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>(true);

        if (_renderer == null)
        {
            CPrint.Error("Renderer를 찾을 수 없습니다.");
            return;
        }

        _renderer.enabled = true;
        
        _phase = transform.GetSiblingIndex();
        Managers.Event.OnPhaseUpdated += HandlePhaseUpdated;
        
        if (_spawner == null)
        {
            _spawner = GetComponentInChildren<Spawner>();
            _spawner.OnAllEnemiesDefeated += HandleAllEnemiesDefeated;
        }
    }

    private void Start()
    {
        if (Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            _player = gameScene.Player;
        }

        _isStarted = false;
        UpdateAlertStarted();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagKey.Player))
        {
            return;
        }

        if (_isStarted)
        {
            return;
        }

        OpenAlertPopup();
    }

    private void OnDestroy()
    {
        if (Managers.Event != null)
        {
            Managers.Event.OnPhaseUpdated -= HandlePhaseUpdated;
        }

        if (_spawner != null)
        {
            _spawner.OnAllEnemiesDefeated -= HandleAllEnemiesDefeated;
        }
    }

    private void OpenAlertPopup()
    {
        AlertPopup popup = Managers.UI.OpenPopup<AlertPopup>();

        if (popup == null)
        {
            CPrint.Error("[ForceField] AlertPopup을 열 수 없습니다.");
            return;
        }

        if (_phase == 0)
        {
            popup.Set("시련을 극복하시겠습니까?", true, StartChallenge);
        }
        else // TODO 보스까지 완성
        {
            popup.Set("준비 중 입니다.");
        }
        
    }

    /// <summary> AlertPopup에서 Yes를 눌렀을 때 실행 </summary>
    private void StartChallenge()
    {
        if (_isStarted)
        {
            return;
        }

        _isStarted = true;
        UpdateAlertStarted();

        if (_player == null)
        {
            CPrint.Error("_player no found!");
            return;
        }

        if (_spawner == null)
        {
            CPrint.Error("Spawner no found!");
            
            return;
        }

        _player.TeleportToTarget(_spawner.transform, () =>
        {
            _spawner.SpawnEnemies();
        });
    }

    public void UpdateAlertStarted()
    {
        if (_renderer == null)
        {
            return;
        }

        _renderer.material = _isStarted ? _redMaterial : _blueMaterial;
        
        _arrow.gameObject.SetActive(!_isStarted);
    }

    #region ===== 핸들 =====
    
    private void HandlePhaseUpdated()
    {
        if (Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            if (gameScene.CurrentPhase != _phase)
            {
                gameObject.SetActive(false);
                
                return;
            }
        }
        else
        {
            CPrint.Error("Scene no found!");
            
            gameObject.SetActive(false);
            
            return;
        }
        
        gameObject.SetActive(true);
    }
    
    private void HandleAllEnemiesDefeated()
    {
        gameObject.SetActive(false);

        if (Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            gameScene.AdvancePhase();
        }
    }
    
    #endregion ===== 핸들 =====
}