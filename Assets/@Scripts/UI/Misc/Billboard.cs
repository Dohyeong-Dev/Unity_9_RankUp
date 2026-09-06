using UnityEngine;

/// <summary> 오브젝트가 항상 게임 카메라와 같은 방향을 바라보도록 회전시키는 컴포넌트다. </summary>
public class Billboard : MonoBehaviour
{
    private Transform _cameraTransform;

    private void Start()
    {
        TryResolveCameraTransform();
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null)
        {
            return;
        }

        transform.rotation = _cameraTransform.rotation;
    }

    /// <summary> 현재 GameScene에서 카메라 Transform 참조를 가져온다. </summary>
    private bool TryResolveCameraTransform()
    {
        if (_cameraTransform != null)
        {
            return true;
        }

        if (!Managers.Scene.TryGetCurrentScene(out GameScene gameScene))
        {
            return false;
        }

        _cameraTransform = gameScene.Cam.transform;
        return true;
    }
}