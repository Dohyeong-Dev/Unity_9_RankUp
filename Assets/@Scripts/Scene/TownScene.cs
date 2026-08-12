using UnityEngine;

public class TownScene : BaseScene
{
    [SerializeField] private PlayerCtrl _player;
    [SerializeField] private CamCtrl _camera;

    protected override void OnAwake()
    {
        Managers.Input.SetCursorLock(true);

        _player.SetCamera(_camera.GetComponent<Camera>());
        _camera.SetTarget(_player.transform);
    }
}