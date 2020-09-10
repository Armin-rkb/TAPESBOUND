using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectId : MonoBehaviour {

    public string Id => id; 

    private static readonly Dictionary<string, ObjectId> instances = new Dictionary<string, ObjectId>();

    [SerializeField] private string id;

    public void GenerateId() 
    {
        id = Guid.NewGuid().ToString();
    }

    public static GameObject Find(string objectId, UnityEngine.Object obj = null) 
    {
        if (!instances.ContainsKey(objectId)) 
        {
            Debug.LogError("Can't find ObjectId.\n" + objectId, obj);
            return null;
        }
        return instances[objectId].gameObject;
    }

    private void Awake() 
    {
        instances.Add(id, this);
    }

    private void OnDestroy() 
    {
        instances.Remove(id);
    }

    private void Reset() 
    {
        GenerateId();
    }
}
