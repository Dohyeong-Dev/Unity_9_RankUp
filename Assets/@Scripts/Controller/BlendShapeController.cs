using UnityEngine;

/// <summary> 캐릭터의 BlendShape를 제어하는 클래스 </summary>
public class BlendShapeController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    /// <summary> 지정한 BlendShape를 지정된 가중치로 설정한다. </summary>
    public void SetBlendShape(int index, float weight)
    {
        if (_skinnedMeshRenderer == null)
        {
            CPrint.Warning("[BlendShapeController] skinnedMeshRenderer is null");
            return;
        }
        
        if (index < 0 || index >= _skinnedMeshRenderer.sharedMesh.blendShapeCount)
        {
            CPrint.Warning("[BlendShapeController] Invalid blendshape index: " + index);
        }
        
        weight = Mathf.Clamp(weight, 0f, 100f);
            
        _skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
    }

    /// <summary> 모든 BlendShape를 기본 상태로 초기화한다. </summary>
    public void ResetBlendShape()
    {
        if (_skinnedMeshRenderer == null)
        {
            CPrint.Warning("[BlendShapeController] skinnedMeshRenderer is null");
            return;
        }

        for (int i = 0; i < _skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
        {
            _skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
        }
    }
}