using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyCtrl : MonoBehaviour, IDamageable
{
    private PlayerCtrl _player; 
    public PlayerCtrl Player => _player;
    
    private NavMeshAgent _navMeshAgent;
    public NavMeshAgent NavMeshAgent => _navMeshAgent;
    private Animator _animator;
    public Animator Animator => _animator;
    
    
    #region ===== 타겟팅 ======
    
    private FieldOfView _fieldOfView;

    public Transform Target => _fieldOfView.CurrentTarget;
    
    #endregion ===== 타겟팅 =====
    
    
    private readonly EnemyStateMachine _stateMachine = new(); 
    
    #region ===== 상태 =====
    
    public bool IsDead { get; }
    
    public bool CanAttack = false;
    
    #endregion ===== 상태 =====
    
    
    #region ===== 디졸브 =====
    
    [Header("디졸브")]
    [SerializeField] private float _dissolveDuration = 0.5f;

    [Tooltip("디졸브 진행 속도")]
    [SerializeField] private Ease _dissolveEase = Ease.InOutQuad;

    private readonly List<Material> _materials = new();

    private Tween _dissolveTween;
    private float _dissolveValue;

    #endregion ===== 디졸브 =====
    
        
    private void Awake()
    {
        GameScene gameScene = Managers.Scene.CurrentScene as GameScene;
        
        if (gameScene == null)
        {
            CPrint.Error("GameScene no found!");
        }
        else
        {
            _player = gameScene.Player;
        }

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        
        _fieldOfView = GetComponent<FieldOfView>();
        
        InitializeMaterials();
        SetDissolveValue(0f);
        
        InitializeStateMachine();
    }

    private void Update()
    {
        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    private void OnDestroy()
    {
        _dissolveTween?.Kill();
    }
    
    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new IdleState(_stateMachine, this));
        _stateMachine.RegisterState(new ChaseState(_stateMachine, this));
        _stateMachine.RegisterState(new AttackState(_stateMachine, this));
        _stateMachine.RegisterState(new ReturnState(_stateMachine, this));
    }

    public void Spawn(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            return;
        }

        // 위치 설정
        transform.position = spawnPoint.position;

        // 처음 스테이트 Idle 시작
        _stateMachine.ChangeState<IdleState>();
        
        // 등장 시 디졸브 상태 초기화
        _dissolveTween?.Kill();
        SetDissolveValue(1f);

        // 사라진 상태 → 나타나는 상태
        Dissolve(false);
    }
    
    public void TakeDamage(float damage)
    {
    }

    public void Die()
    {
    }
    
    #region ===== 디졸브 =====
    
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
    
    #endregion ===== 디졸브 =====
}