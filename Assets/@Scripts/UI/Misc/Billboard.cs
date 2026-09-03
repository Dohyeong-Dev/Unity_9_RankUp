using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camTr;

    private void Start()
    {
        if (Managers.Scene.CurrentScene is GameScene gameScene)
        {
            _camTr = gameScene.Cam.transform;
        }
    }

    private void LateUpdate()
    {
        if (_camTr == null)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(_camTr.forward, _camTr.up);
    }
}