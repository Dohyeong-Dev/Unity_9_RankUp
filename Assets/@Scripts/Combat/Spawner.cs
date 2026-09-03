using UnityEngine;

public class Spawner : MonoBehaviour
{
    /// <summary> 모든 자식 위치에 적을 생성 </summary>
    public void SpawnEnemies()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform spawnPoint = transform.GetChild(i);

            if (!spawnPoint.gameObject.activeSelf)
            {
                continue;
            }
            
            SpawnEnemy(spawnPoint);
        }
    }

    /// <summary> 지정된 위치에 적 생성 </summary>
    private void SpawnEnemy(Transform spawnPoint)
    {
        PoolObj enemyObj = Managers.Pool.Get(PoolKey.Path.EnemyMelee);

        if (enemyObj == null)
        {
            CPrint.Warning("Enemy no found!");
            return;
        }

        if (enemyObj.TryGetComponent(out EnemyCtrl enemy))
        {
            enemy.Spawn(spawnPoint);
        }
    }
}