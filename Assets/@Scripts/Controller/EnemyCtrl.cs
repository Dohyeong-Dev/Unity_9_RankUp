using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCtrl : MonoBehaviour
{
    private PlayerCtrl _player; 

    #region -----Dissolve-----
    
    [Header("Dissolve")]
    [SerializeField] private float _dissolveDuration = 0.5f;

    [Tooltip("디졸브 진행 속도")]
    [SerializeField] private Ease _dissolveEase = Ease.InOutQuad;

    private readonly List<Material> _materials = new();

    private Tween _dissolveTween;
    private float _dissolveValue;

    #endregion -----Dissolve-----
    
        
    private void Awake()
    {
        GameScene gameScene = Managers.Scene.CurrentScene as GameScene;
        if (gameScene == null)
        {
            CPrint.Error("GameScene no found!");
            return;
        }

        _player = gameScene.Player;
        
        InitializeMaterials();
        SetDissolveValue(0f);
    }
    
    private void OnDestroy()
    {
        _dissolveTween?.Kill();
    }
    
    private void InitializeMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_Dissolve"))
                {
                    _materials.Add(material);
                }
            }
        }
    }

    public void Spawn(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            return;
        }

        // 위치 설정
        transform.position = spawnPoint.position;

        // 플레이어 방향 바라보기
        if (_player != null)
        {
            Vector3 direction = _player.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        // 등장 시 디졸브 상태 초기화
        _dissolveTween?.Kill();

        SetDissolveValue(1f);

        // 사라진 상태 → 나타나는 상태
        Dissolve(false);
    }
    
    private void SetDissolveValue(float value)
    {
        _dissolveValue = value;

        foreach (Material material in _materials)
        {
            material.SetFloat("_Dissolve", value);
        }
    }
    
    private void Dissolve(bool isDissolve)
    {
        float targetValue = isDissolve ? 1f : 0f;

        _dissolveTween?.Kill();

        _dissolveTween = DOTween.To(() => _dissolveValue, SetDissolveValue, targetValue, _dissolveDuration)
            .SetEase(_dissolveEase);
    }
}