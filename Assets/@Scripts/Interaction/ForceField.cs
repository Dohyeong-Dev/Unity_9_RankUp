using UnityEngine;

public class ForceField : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material _redMaterial;
    [SerializeField] private Material _blueMaterial;

    [Header("Challenge")]
    [SerializeField] private Transform _spawner;
    
    private PlayerCtrl _player;

    private Renderer _renderer;

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
    }

    private void Start()
    {
        GameScene gameScene = Managers.Scene.CurrentScene as GameScene;
        if (gameScene == null)
        {
            CPrint.Log("GameScene no found!");
        }
        else
        {
            _player = gameScene.Player;
        }
        
        SetBlue();
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

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(TagKey.Player))
        {
            return;
        }

        if (!_isStarted)
        {
            SetBlue();
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

        popup.Set("시련을 극복하시겠습니까?", true, StartChallenge);
    }


    /// <summary> AlertPopup에서 Yes를 눌렀을 때 실행 </summary>
    private void StartChallenge()
    {
        if (_isStarted)
        {
            return;
        }

        _isStarted = true;

        SetRed();

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

        _player.TeleportToTarget(_spawner, () =>
        {
            GameScene gameScene = Managers.Scene.CurrentScene as GameScene;

            if (gameScene == null)
            {
                CPrint.Error("GameScene no found!");
            }
            else
            {
                int forceFieldIndex = transform.GetSiblingIndex();

                gameScene.SpawnerCtrl.SpawnEnemies(forceFieldIndex);
            }
        });
    }


    public void SetRed()
    {
        if (_renderer == null)
        {
            return;
        }

        _renderer.material = _redMaterial;
    }


    public void SetBlue()
    {
        if (_renderer == null)
        {
            return;
        }

        _renderer.material = _blueMaterial;
    }
}