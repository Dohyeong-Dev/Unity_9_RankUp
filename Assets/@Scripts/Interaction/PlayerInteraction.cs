using System.Collections.Generic;
using UnityEngine;

/// <summary> 플레이어 주변의 상호작용 대상을 관리하고 입력에 따라 상호작용을 실행한다. </summary>
public class PlayerInteraction : MonoBehaviour
{
    private GameHUD _gameHud;

    private readonly List<IInteractable> _interactables = new();

    private void Start()
    {
        if (Managers.Scene.TryGetCurrentScene(out GameScene scene))
        {
            _gameHud = scene.HUD;
        }
        else
        {
            CPrint.Error("[PlayerInteraction] no game hud found");
        }
    }

    private void Update()
    {
        if (Managers.Input.KeyDown_F)
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable))
        {
            return;
        }

        if (_interactables.Contains(interactable))
        {
            return;
        }

        _interactables.Add(interactable);

        UpdateInteractionUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable))
        {
            return;
        }

        _interactables.Remove(interactable);

        UpdateInteractionUI();
    }

    /// <summary> 현재 상호작용 가능한 대상과 상호작용한다. </summary>
    private void Interact()
    {
        if (_interactables.Count == 0)
        {
            return;
        }

        IInteractable interactable = _interactables[0];
        _interactables.RemoveAt(0);

        Managers.Sound.PlaySfx(ResourceKey.Name.SfxType.PickUpItem, 0.6f);

        interactable.Interact();

        UpdateInteractionUI();
    }

    /// <summary> 현재 상호작용 가능한 대상에 맞춰 HUD를 갱신한다. </summary>
    private void UpdateInteractionUI()
    {
        if (_gameHud == null)
        {
            return;
        }
        
        if (_interactables.Count > 0)
        {
            _gameHud.SetInteractionTextActive(true, _interactables[0].Type);
        }
        else
        {
            _gameHud.SetInteractionTextActive(false);
        }
    }
}