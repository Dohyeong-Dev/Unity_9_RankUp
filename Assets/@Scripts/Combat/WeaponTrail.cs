using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class WeaponTrail : MonoBehaviour
{
    private TrailRenderer _trail;

    public void SetActive(bool active)
    {
        _trail.emitting = active;
    }

    private void Awake()
    {
        if (_trail == null)
        {
            _trail = GetComponent<TrailRenderer>();
        }

        _trail.emitting = false;
    }
}