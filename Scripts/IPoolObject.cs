using UnityEngine;

namespace Redcode.Pools
{
    /// <summary>
    /// Interface for pool's objects. <br/>
    /// Your classes can implement this interface if you want some additional processing for your pool's objects.
    /// </summary>
    public interface IPoolObject
    {
        /// <summary>
        /// Called by pool when object instantiated.
        /// </summary>
        void OnCreatedInPool();

        /// <summary>
        /// Called by pool before gets you pool's object. Useful for state resetting.
        /// </summary>
        void OnGettingFromPool();
    }
}