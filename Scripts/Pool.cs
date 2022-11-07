using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Redcode.Pools
{
    /// <summary>
    /// Helper class to create generic pools.
    /// </summary>
    public static class Pool
    {
        /// <summary>
        /// <inheritdoc cref="Create{T}(T, int)"/>
        /// </summary>
        /// <typeparam name="T"><inheritdoc cref="Create{T}(T, int)"/></typeparam>
        /// <param name="source"><inheritdoc cref="Create{T}(T, int)" path="/param[@name='source']"/></param>
        /// <param name="count"><inheritdoc cref="Create{T}(T, int)" path="/param[@name='count']"/></param>
        /// <param name="container">Container object for pool objects.</param>
        /// <returns></returns>
        public static Pool<T> Create<T>(T source, int count = 0, Transform container = null) where T : Component => Pool<T>.Create(source, count, container);
    }

    /// <summary>
    /// Represent <typeparamref name="T"/>'s pool.
    /// </summary>
    /// <typeparam name="T">Pool object's type.</typeparam>
    public class Pool<T> : IPool<T> where T : Component
    {
        #region Fields and properties
        private T _source;

        Component IPool.Source => _source;

        /// <summary>
        /// Source of the pool, which will be used for instantiate new clones.
        /// </summary>
        public T Source => _source;

        private int _count;

        public int Count => _count;

        private Transform _container;

        public Transform Container => _container;

        private List<T> _clones;

        private readonly List<T> _busyObjects = new();
        #endregion

        private Pool() { }

        internal static Pool<T> Create(T source, int count, Transform container)
        {
            var pool = new Pool<T>
            {
                _source = source,
                _count = Math.Max(count, 0),
                _container = container,
                _clones = new(count)
            };


            return pool;
        }

        #region SetCount
        IPool IPool.SetCount(int count, bool destroyClones) => SetCount(count, destroyClones);

        IPool<T> IPool<T>.SetCount(int count, bool destroyClones) => SetCount(count, destroyClones);

        /// <summary>
        /// <inheritdoc cref="IPool.SetCount(int, bool)"/>
        /// </summary>
        /// <param name="count"><inheritdoc cref="IPool.SetCount(int, bool)" path="/param[@name='count']"/></param>
        /// <param name="destroyClones"><inheritdoc cref="IPool.SetCount(int, bool)" path="/param[@name='destroyClones']"/></param>
        /// <returns><inheritdoc cref="IPool.SetCount(int, bool)"/></returns>
        public Pool<T> SetCount(int count, bool destroyClones = true)
        {
            count = Math.Max(count, 0);

            if (count == 0)
            {
                _count = count;
                return this;
            }

            if (_count != 0 && count > _count)
            {
                _clones.Capacity = _busyObjects.Capacity = _count = count;
                return this;
            }

            if (destroyClones)
            {
                for (int i = count; i < _clones.Count; i++)
                    Object.Destroy(_clones[i].gameObject);
            }

            _clones.RemoveRange(count, _clones.Count - count);
            _count = count;
            _clones.Capacity = _busyObjects.Capacity = _count;

            return this;
        }
        #endregion

        #region SetContainer
        IPool IPool.SetContainer(Transform container, bool worldPositionStays) => SetContainer(container, worldPositionStays);

        IPool<T> IPool<T>.SetContainer(Transform container, bool worldPositionStays) => SetContainer(container, worldPositionStays);

        /// <summary>
        /// <inheritdoc cref="IPool.SetContainer(Transform, bool)"/>
        /// </summary>
        /// <param name="container"><inheritdoc cref="IPool.SetContainer(Transform, bool)" path="/param[@name='container']"/></param>
        /// <param name="worldPositionStays"><inheritdoc cref="IPool.SetContainer(Transform, bool)" path="/param[@name='worldPositionStays']"/></param>
        /// <returns><inheritdoc cref="IPool.SetContainer(Transform, bool)"/></returns>
        public Pool<T> SetContainer(Transform container, bool worldPositionStays = true)
        {
            _container = container;
            _clones.ForEach(c => c.transform.SetParent(_container, worldPositionStays));

            return this;
        }
        #endregion

        #region Clear
        IPool IPool.Clear(bool destroyClones) => Clear(destroyClones);

        IPool<T> IPool<T>.Clear(bool destroyClones) => Clear(destroyClones);

        /// <summary>
        /// <inheritdoc cref="IPool.Clear(bool)"/>
        /// </summary>
        /// <param name="destroyClones"><inheritdoc cref="IPool.Clear(bool)" path="/param[@name='destroyClones']"/></param>
        /// <returns><inheritdoc cref="IPool.Clear(bool)"/></returns>
        public Pool<T> Clear(bool destroyClones = true)
        {
            if (destroyClones)
            {
                for (int i = 0; i < _clones.Count; i++)
                {
                    var clone = _clones[i];

                    if (clone != null)
                        Object.Destroy(clone.gameObject);
                }
            }

            _clones.Clear();

            return this;
        }
        #endregion

        #region NonLazy
        IPool IPool.NonLazy() => NonLazy();

        IPool<T> IPool<T>.NonLazy() => NonLazy();

        /// <summary>
        /// <inheritdoc cref="IPool.NonLazy()"/>
        /// </summary>
        /// <returns><inheritdoc cref="IPool.NonLazy()"/></returns>
        public Pool<T> NonLazy()
        {
            while (_clones.Count < _count)
            {
                var clone = Object.Instantiate(_source, _container);
                clone.gameObject.SetActive(false);

                _clones.Add(clone);
            }

            return this;
        }
        #endregion

        #region Get
        Component IPool.Get() => Get();

        /// <summary>
        /// Try get object from pool. If all objects are busy, then <see langword="null"/> will be returned.<br/>
        /// You need to remember to call <see cref="Take"/> method to return the object to the pool.
        /// </summary>
        /// <returns>Object (its <b><typeparamref name="T"/></b> component) from pool.</returns>
        public T Get()
        {
            T clone = null;

            for (int i = 0; i < _clones.Count; i++)
            {
                if (!_busyObjects.Contains(_clones[i]))
                {
                    clone = _clones[i];
                    break;
                }
            }

            if (clone == null)
            {
                if (_count != 0 && _clones.Count >= _count)
                    return null;

                _clones.Add(clone = Object.Instantiate(_source, _container));

                if (clone is IPoolObject obj)
                    obj.OnCreatedInPool();
            }

            _busyObjects.Add(clone);

            clone.gameObject.SetActive(true);
            if (clone is IPoolObject resetable)
                resetable.OnGettingFromPool();

            return clone;
        }
        #endregion

        #region Take
        void IPool.Take(Component clone) => Take((T)clone);

        /// <summary>
        /// Marks object as free. If object does not exist in pool's internal list, then it will be added (if it possible).
        /// </summary>
        /// <param name="clone"><inheritdoc cref="IPool.Take(Component)" path="/param[@name='clone']"/></param>
        /// <exception cref="ArgumentException">Throwed if object already free.</exception>
        /// <exception cref="Exception">Throwed if pool is full.</exception>
        public void Take(T clone)
        {
            if (!_clones.Contains(clone))
            {
                //throw new ArgumentException("Passed object does not exist in pool's clones list.");

                if (_count != 0 && _clones.Count >= _count)
                    throw new Exception("Can't add clone to pool, because pool is full");

                clone.transform.SetParent(_container, true);
                _clones.Add(clone);
            }
            else
            {
                if (!_busyObjects.Contains(clone))
                    throw new ArgumentException("Passed object already free.");

                _busyObjects.Remove(clone);
            }

            clone.gameObject.SetActive(false);
        }
        #endregion

        public CustomYieldInstruction WaitForFreeObject() => new WaitWhile(() => _busyObjects.Count == _clones.Count);
    }
}