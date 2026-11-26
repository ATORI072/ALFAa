using UnityEngine;
using System.Collections;

/// <summary>
/// 刷怪点：按间隔生成敌人，场上数量到上限就暂停。
/// 挂在 EnemySpawn 标记物体上即可。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;  // 敌人预制体
    public float interval = 5f;     // 刷怪间隔秒数
    public int maxAlive = 5;        // 场上最多同时存在几个敌人

    void Start() { StartCoroutine(SpawnLoop()); }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            int alive = FindObjectsOfType<EnemyAI>().Length;
            if (enemyPrefab != null && alive < maxAlive)
                Instantiate(enemyPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            yield return new WaitForSeconds(interval);
        }
    }
}
