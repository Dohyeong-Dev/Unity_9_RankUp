using UnityEngine;

public class Bag : MonoBehaviour
{
    private BagSpawnCtrl _spawnCtrl;
    
    [SerializeField] private Transform _model;
    [SerializeField] private Transform _effect;

    private bool isDestroyed = false;
    
    public void Initialize(BagSpawnCtrl spawnCtrl)
    {
        _effect.gameObject.SetActive(false);
        _spawnCtrl = spawnCtrl;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestroyed)
        {
            return;
        }
        
        CPrint.Log(other.gameObject.name);
        if (!other.CompareTag(TagKey.Player))
        {
            return;
        }

        Collect();
    }

    private void Collect()
    {
        _spawnCtrl.OnBagCollected(transform.position);

        PlayDestroyEffect();
        gameObject.DestroyGO(2f);
    }
    
    private void PlayDestroyEffect()
    {
        if (isDestroyed)
        {
            return;
        }
        
        isDestroyed = true;
        _effect.gameObject.SetActive(true);
        _model.gameObject.SetActive(false);
        
        if (Managers.Scene.CurrentScene?.Type == SceneType.GameScene)
        {
            GameScene gameScene = Managers.Scene.CurrentScene as GameScene;
            if (gameScene != null)
            {
                gameScene.HUD.IncreaseHp();
            }
        }
    }
}