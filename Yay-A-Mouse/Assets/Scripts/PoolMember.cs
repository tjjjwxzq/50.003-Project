using UnityEngine;
using System.Collections;

/// <summary>
/// Script that is attached to members of an object pool
/// when they are first instantiated.
/// Allows object to keep track of the pool they belong to
/// and be deactivated and returned to that pool.
/// </summary>
public class PoolMember : MonoBehaviour {

    private ObjectPool pool;

    /// <summary>
    /// Deactivates and returns the object to its pool
    /// </summary>
    public void Deactivate()
    {
        pool.returnObj(gameObject);
    }

    public void setPool(ObjectPool pool) { this.pool = pool; }

}
