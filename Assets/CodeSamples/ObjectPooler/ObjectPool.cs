using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple object pool implementation for gameobjects
/// </summary>
public class ObjectPool
{
    private GameObject _prefab;
    private int _size = 50;
    private bool _scale = false;

    private List<GameObject> _objects;
    private GameObject _parent;

    /// <summary>
    /// Initialises object pool
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="size"></param>
    /// <param name="scale"></param>
    public ObjectPool(GameObject prefab, int size, bool scale = false)
    {
        _prefab = prefab;
        _size = size;
        _scale = scale;

        _objects = new List<GameObject>();
        _parent = new GameObject(prefab.name + " Pool");

        for (int i = 0; i < _size; i++)
        {
            var obj = GameObject.Instantiate(_prefab);
            obj.transform.parent = _parent.transform;
            obj.SetActive(false);
            _objects.Add(obj);
        }
    }

    /// <summary>
    /// Get object from pool
    /// </summary>
    /// <returns></returns>
    public GameObject GetObject()
    {
        GameObject result = null;
        foreach (var obj in _objects)
        {
            if (obj.activeInHierarchy)
                continue;

            result = obj;
            break;
        }

        if (result == null && _scale)
        {
            var obj = GameObject.Instantiate(_prefab);
            obj.SetActive(false);
            obj.transform.parent = _parent.transform;
            _objects.Add(obj);
            result = obj;
        }

        if (result)
            result.SetActive(true);

        return result;
    }
}
