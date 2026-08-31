using UnityEngine;

public class ForceField : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material _redMaterial;
    [SerializeField] private Material _blueMaterial;

    private Renderer _renderer;

    private bool _isStarted;


    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>(true);

        if (_renderer == null)
        {
            CPrint.Error("[ForceField] Renderer를 찾을 수 없습니다.");
            return;
        }

        _renderer.enabled = true;
    }


    private void Start()
    {
        SetBlue();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagKey.Player))
        {
            return;
        }

        // 이미 시련이 시작됐다면 다시 팝업을 띄우지 않는다.
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

        // 아직 시련을 시작하지 않았다면
        // 다시 파란색 상태로 유지한다.
        if (!_isStarted)
        {
            SetBlue();
        }
    }


    /// <summary>
    /// 시련 시작 여부를 묻는 팝업
    /// </summary>
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


    /// <summary>
    /// AlertPopup에서 Yes를 눌렀을 때 실행
    /// </summary>
    private void StartChallenge()
    {
        if (_isStarted)
        {
            return;
        }

        _isStarted = true;

        SetRed();

        // TODO
        // 몬스터 생성
        // 시련 시작
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