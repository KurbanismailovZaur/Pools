using Redcode.Pools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolObject
{
    public enum State { Alive, Died }
    public int Health { get; private set; }

    private State _state;

    // Called when getting this object from pool.
    public void OnGettingFromPool()
    {
        _state = State.Alive;
        Health = 100;

        print(_state);
    }

    public void OnCreatedInPool()
    {
        throw new System.NotImplementedException();
    }
}

