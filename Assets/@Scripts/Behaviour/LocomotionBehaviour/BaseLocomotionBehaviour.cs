using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCtrl))]
public abstract class BaseLocomotionBehaviour : MonoBehaviour
{
    [SerializeField] private float _requiredSpRate;
    public float RequiredSpRate => _requiredSpRate;
    
    protected PlayerCtrl Player;

    public int BehaviourHash { get; private set; }

    private void Awake()
    {
        Player = GetComponent<PlayerCtrl>();
        BehaviourHash = GetType().GetHashCode();
    }

    public virtual void OnFixedUpdate()
    {
    }
}