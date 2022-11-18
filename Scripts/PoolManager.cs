using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Redcode.Pools
{
    [Serializable]
    internal struct PoolData
    {
        [SerializeField]
        private string _name;

        public string Name => _name;

        [SerializeField]
        private Component _component;

        public Component Component => _component;

        [SerializeField]
        [Min(0)]
        private int _count;

        public int Count => _count;

        [SerializeField]
        private Transform _container;

        public Transform Container => _container;

        [SerializeField]
        private bool _nonLazy;

        public bool NonLazy => _nonLazy;
    }

    /// <summary>
    /// Pool manager. You can set options for it in editor and then use in game. <br/>
    /// It creates specified pools in Awake method, which then you can find with <b>GetPool</b> methods and call its methods.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        [SerializeField]
        private List<PoolData> _pools;

        private readonly List<IPool<Component>> _poolsObjects = new();

        private void Awake()
        {
            var namesGroups = _pools.Select(p => p.Name).GroupBy(n => n).Where(g => g.Count() > 1);

            if (namesGroups.Count() > 0)
                throw new Exception($"Pool Manager already contains pool with name \"{namesGroups.First().Select(g => g).First()}\"");

            var poolsType = typeof(List<IPool<Component>>);
            var poolsAddMethod = poolsType.GetMethod("Add");
            var genericPoolType = typeof(Pool<>);

            foreach (var poolData in _pools)
            {
                var poolType = genericPoolType.MakeGenericType(poolData.Component.GetType());
                var createMethod = poolType.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);

                var pool = createMethod.Invoke(null, new object[] { poolData.Component, poolData.Count, poolData.Container });

                if (poolData.NonLazy)
                {
                    var nonLazyMethod = poolType.GetMethod("NonLazy");
                    nonLazyMethod.Invoke(pool, null);
                }

                poolsAddMethod.Invoke(_poolsObjects, new object[] { pool });
            }
        }

        #region Get pool
        /// <summary>
        /// Find pool by <paramref name="index"/>.
        /// </summary>
        /// <typeparam name="T">Pool's objects type.</typeparam>
        /// <param name="index">Pool index.</param>
        /// <returns>Finded pool.</returns>
        public IPool<T> GetPool<T>(int index) where T : Component => (IPool<T>)_poolsObjects[index];

        /// <summary>
        /// Find pool by type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Pool's objects type.</typeparam>
        /// <returns>Finded pool.</returns>
        public IPool<T> GetPool<T>() where T : Component => (IPool<T>)_poolsObjects.Find(p => p.Source is T);

        /// <summary>
        /// Find pool by <paramref name="name"/>
        /// </summary>
        /// <typeparam name="T">Pool's objects type.</typeparam>
        /// <param name="name">Pool name.</param>
        /// <returns>Finded pool.</returns>
        public IPool<T> GetPool<T>(string name) where T : Component => (IPool<T>)_poolsObjects[_pools.FindIndex(p => p.Name == name)];
        #endregion

        #region Get from pool
        /// <summary>
        /// Find pool by <paramref name="index"/> and gets object from it.
        /// </summary>
        /// <typeparam name="T"><inheritdoc cref="GetPool{T}"/></typeparam>
        /// <param name="index"><inheritdoc cref="GetPool{T}"/></param>
        /// <returns>Pool's object (or <see langword="null"/> if free object was not finded).</returns>
        public T GetFromPool<T>(int index) where T : Component => GetPool<T>(index).Get();

        /// <summary>
        /// Find pool by type <typeparamref name="T"/> and gets object from it.
        /// </summary>
        /// <typeparam name="T"><inheritdoc cref="GetPool{T}"/></typeparam>
        /// <returns>Pool's object (or <see langword="null"/> if free object was not finded).</returns>
        public T GetFromPool<T>() where T : Component => GetPool<T>().Get();

        /// <summary>
        /// Find pool by name <paramref name="name"/> and gets object from it.
        /// </summary>
        /// <typeparam name="T"><inheritdoc cref="GetPool{T}"/></typeparam>
        /// <param name="name"><inheritdoc cref="GetPool{T}(string)"/></param>
        /// <returns>Pool's object (or <see langword="null"/> if free object was not finded).</returns>
        public T GetFromPool<T>(string name) where T : Component => GetPool<T>(name).Get();
        #endregion

        #region Take to pool
        /// <summary>
        /// Returns object back to pool and marks it as free.
        /// </summary>
        /// <param name="index"><inheritdoc cref="GetPool{T}"/></param>
        /// <param name="component">Object (its component) which returns back.</param>
        public void TakeToPool(int index, Component component) => _poolsObjects[index].Take(component);

        /// <summary>
        /// Returns object back to pool and marks it as free.
        /// </summary>
        /// <typeparam name="T">Pool type.</typeparam>
        /// <param name="component">Object (its component) which returns back.</param>
        public void TakeToPool<T>(Component component) where T : Component => GetPool<T>().Take(component);

        /// <summary>
        /// Returns object back to pool and marks it as free.
        /// </summary>
        /// <typeparam name="T">Pool type.</typeparam>
        /// <param name="name"><inheritdoc cref="GetPool{T}(string)"/></param>
        /// <param name="component">Object (its component) which returns back.</param>
        public void TakeToPool<T>(string name, Component component) where T : Component => GetPool<T>(name).Take(component);
        #endregion
    }
}
