using UnityEngine;

/// <summary> 공격 범위를 Box 형태로 검사한다. </summary>
public class AttackRange : MonoBehaviour
{
    [SerializeField] private Color _gizmoColor = new(1f, 0f, 0f, 0.3f);

    /// <summary> 지정된 레이어 중 현재 공격 범위 안에 있는 Collider를 반환한다. </summary>
    public Collider[] GetColliders(LayerMask targetLayer)
    {
        Vector3 halfExtents = transform.lossyScale * 0.5f;

        return Physics.OverlapBox(transform.position, halfExtents, transform.rotation, targetLayer,
            QueryTriggerInteraction.Collide);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = _gizmoColor;

        Gizmos.DrawCube(Vector3.zero, transform.lossyScale);

        Gizmos.matrix = Matrix4x4.identity;
    }
}