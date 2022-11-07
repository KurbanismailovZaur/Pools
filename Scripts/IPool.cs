using UnityEngine;

namespace Redcode.Pools
{
    /// <summary>
    /// Represent nongeneralized objects pool.
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// Source of the pool, which will be used for instantiate new clones.
        /// </summary>
        Component Source { get; }

        /// <summary>
        /// Maximum objects in pool.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Container for pool objects. All pool's objects will be spawn in this container. 
        /// </summary>
        Transform Container { get; }

        /// <summary>
        /// Sets new maximum count for pool objects.
        /// </summary>
        /// <param name="count">Maximum objects count.</param>
        /// <param name="destroyClones">
        /// Do we need destroy already exists pool objects, or keep they live?<br/>
        /// These objects will no longer be controlled by the pool.
        /// </param>
        /// <returns>The pool.</returns>
        IPool SetCount(int count, bool destroyClones = true);

        /// <summary>
        /// Sets new container for pool objects. Already exists pool's objects will be reparented.
        /// </summary>
        /// <param name="container">Container for pool objects.</param>
        /// <param name="worldPositionStays">Do we need save pool objects world position?</param>
        /// <returns>The pool.</returns>
        IPool SetContainer(Transform container, bool worldPositionStays = true);

        /// <summary>
        /// Clears all pool's objects.
        /// </summary>
        /// <param name="destroyClones">
        /// Do we need destroy already exists pool's objects, or keep they live?<br/>
        /// These objects will no longer be controlled by the pool.
        /// </param>
        /// <returns>The pool.</returns>
        IPool Clear(bool destroyClones = true);

        /// <summary>
        /// Immediately create all pool's objects. Not works when <b>Count</b> is <b>0</b>./>
        /// </summary>
        /// <returns>The pool.</returns>
        IPool NonLazy();

        /// <summary>
        /// Try get object from pool. If all objects are busy, then <see langword="null"/> will be returned.<br/>
        /// You need to remember to call <see cref="Take"/> method to return the object to the pool.
        /// </summary>
        /// <returns>Object (its <b>Component</b>) from pool.</returns>
        Component Get();

        /// <summary>
        /// Marks object as free. If object does not exist in pool's internal list, then it will be added (if it possible).
        /// </summary>
        /// <param name="clone">Pool's object to mark as free or add.</param>
        /// <exception cref="ArgumentException">Throwed if object already free.</exception>
        /// <exception cref="Exception">Throwed if pool is full.</exception>
        void Take(Component clone);

        /// <summary>
        /// Creates object which know how await (with <b>yield</b>) the pool for next free object.
        /// </summary>
        /// <returns>Awaiter object.</returns>
        CustomYieldInstruction WaitForFreeObject();
    }

    public interface IPool<out T> : IPool where T : Component
    {
        /// <summary>
        /// Source of the pool, which will be used for instantiate new clones.
        /// </summary>
        new T Source { get; }

        /// <summary>
        /// <inheritdoc cref="IPool.SetCount(int, bool)"/>
        /// </summary>
        /// <param name="count"><inheritdoc cref="IPool.SetCount(int, bool)" path="/param[@name='count']"/></param>
        /// <param name="destroyClones"><inheritdoc cref="IPool.SetCount(int, bool)" path="/param[@name='destroyClones']"/></param>
        /// <returns><inheritdoc cref="IPool.SetCount(int, bool)"/></returns>
        new IPool<T> SetCount(int count, bool destroyClones = true);

        /// <summary>
        /// <inheritdoc cref="IPool.SetContainer(Transform, bool)"/>
        /// </summary>
        /// <param name="container"><inheritdoc cref="IPool.SetContainer(Transform, bool)" path="/param[@name='container']"/></param>
        /// <param name="worldPositionStays"><inheritdoc cref="IPool.SetContainer(Transform, bool)" path="/param[@name='worldPositionStays']"/></param>
        /// <returns><inheritdoc cref="IPool.SetContainer(Transform, bool)"/></returns>
        new IPool<T> SetContainer(Transform container, bool worldPositionStays = true);

        /// <summary>
        /// <inheritdoc cref="IPool.Clear(bool)"/>
        /// </summary>
        /// <param name="destroyClones"><inheritdoc cref="IPool.Clear(bool)" path="/param[@name='destroyClones']"/></param>
        /// <returns><inheritdoc cref="IPool.Clear(bool)"/></returns>
        new IPool<T> Clear(bool destroyClones = true);

        /// <summary>
        /// <inheritdoc cref="IPool.NonLazy()"/>
        /// </summary>
        /// <returns><inheritdoc cref="IPool.NonLazy()"/></returns>
        new IPool<T> NonLazy();

        /// <summary>
        /// Try get object from pool. If all objects are busy, then <see langword="null"/> will be returned.<br/>
        /// You need to remember to call <see cref="Take"/> method to return the object to the pool.
        /// </summary>
        /// <returns>Object (its <b><typeparamref name="T"/></b> component) from pool.</returns>
        new T Get();
    }
}