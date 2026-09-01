using UnityEngine;

public class SpawnerCtrl : MonoBehaviour
{
    private Spawner[] _spawners;

    private void Awake()
    {
        _spawners = GetComponentsInChildren<Spawner>();
    }

    public void SpawnEnemies(int index)
    {
        if (index < 0 || index >= _spawners.Length)
        {
            CPrint.Warning($"잘못된 Spawner Index : {index}");
            
            return;
        }

        _spawners[index].SpawnEnemies();
    }
}