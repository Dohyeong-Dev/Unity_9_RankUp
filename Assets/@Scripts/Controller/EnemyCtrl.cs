using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCtrl : MonoBehaviour
{
    [Header("Dissolve")]
    [SerializeField] private float _dissolveDuration = 0.5f;

    [Tooltip("디졸브 진행 속도")]
    [SerializeField] private Ease _dissolveEase = Ease.InOutQuad;

    private readonly List<Material> _materials = new();

    private Tween _dissolveTween;
    private float _dissolveValue;


    private void Awake()
    {
        InitializeMaterials();

        SetDissolveValue(0f);
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


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TagKey.Player))
        {
            return;
        }

        Dissolve(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(TagKey.Player))
        {
            return;
        }

        Dissolve(false);
    }


    private void Dissolve(bool isDissolve)
    {
        float targetValue = isDissolve ? 1f : 0f;

        _dissolveTween?.Kill();

        _dissolveTween = DOTween.To(() => _dissolveValue, SetDissolveValue, targetValue, _dissolveDuration)
            .SetEase(_dissolveEase);
    }


    private void SetDissolveValue(float value)
    {
        _dissolveValue = value;

        foreach (Material material in _materials)
        {
            material.SetFloat("_Dissolve", value);
        }
    }


    private void OnDestroy()
    {
        _dissolveTween?.Kill();
    }
}