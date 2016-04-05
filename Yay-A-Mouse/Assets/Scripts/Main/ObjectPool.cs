using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generic class for handling object pooling.
/// The type of game object to pool can be specified
/// by assigning the public member obj in the inspector panel.
/// Keeps track of active objects in the pool.
/// Has methods to get object from pool and return object to the pool.
/// Pool is maintained as a list and can grow dynamically.
/// </summary>
public class ObjectPool : MonoBehaviour {

    private GameObject obj; //!< Specify which object to pool in inspector panel
    private int activeObjects; //!< Tracks the number of active objects (removed from the pool)
    private List<GameObject> pool = new List<GameObject>(); //<! Pool maintained as a dynamic list

    /// <summary>
    /// Gets an object from the pool.
    /// If the pool is empty, instantiates a new object 
    /// and dynamically grows the pool.
    /// </summary>
    /// <returns></returns>
    public GameObject GetObj()
    {
        // Make sure to spawn off-screen
        float posX = CameraController.MinXUnits - 2;
        float posY = CameraController.MinYUnits - 2;

        if(pool.Count == 0)
        {
            GameObject clone = (GameObject)Instantiate(obj, new Vector2(posX, posY), Quaternion.identity);
            clone.transform.parent = transform;
            clone.SetActive(false);
            pool.Add(clone);
            // Attach PoolMember script component so object remembers the pool it belongs to
            PoolMember poolMember = clone.AddComponent<PoolMember>();
            poolMember.setPool(this);
        }

        GameObject nextObj = pool[0];
        nextObj.transform.position = new Vector2(posX, posY); // make sure to move off screen before reactivating
        pool.RemoveAt(0);
        nextObj.SetActive(true);
        activeObjects++;
        return nextObj;
    }

    /// <summary>
    /// Returns a specified object to the pool.
    /// To be called by PoolMember::Deactivate()
    /// </summary>
    /// <param name="obj"></param>
    public void ReturnObj(GameObject obj)
    {
        obj.SetActive(false);
        pool.Add(obj);
        activeObjects--;
    }

    /// <summary>
    /// Gets the number of active objects that were released from the pool.
    /// </summary>
    /// <returns></returns>
    public int ActiveObjects 
    { 
        get { return activeObjects; }
    }

    /// <summary>
    /// Property to get and set object of the pool
    /// </summary>
    public GameObject PoolObject 
    {
        get { return obj; }
        set { obj = value; }
    }

}
