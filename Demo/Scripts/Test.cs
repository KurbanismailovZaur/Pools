using Redcode.Pools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private PoolManager _poolManager;

    [SerializeField]
    private Enemy _enemyPrefab;

    private void Start()
    {
        // Getting pool from pool manager.
        // Also has other overloads.
        var enemyPool = _poolManager.GetPool<Enemy>();

        // Find pool of type Enemy and get object from it.
        // Also has other overloads.
        var enemy = _poolManager.GetFromPool<Enemy>();

        // Find pool of type Enemy and returns clone to it.
        // Also has other overloads.
        _poolManager.TakeToPool<Enemy>(enemy);
    }
}
