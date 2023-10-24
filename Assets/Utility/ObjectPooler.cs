using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler<T> where T: Object
{
    public delegate void OnChange(T obj);
    private OnChange onPop;
    private OnChange onEnqueu;
    private OnChange onCreate;
    T poolObject;
    Queue<T> pool = new Queue<T>();

    bool initialized = false;
    public void Initialize(T p_pooObject, OnChange p_onPop,OnChange p_onEnqueu) 
    {
        poolObject = p_pooObject;
        onPop = p_onPop;
        onEnqueu = p_onEnqueu;
        initialized = true;
    }
    public void Initialize(T p_pooObject, OnChange p_onPop, OnChange p_onEnqueu, OnChange p_onCreate)
    {
        poolObject = p_pooObject;
        onPop = p_onPop;
        onEnqueu = p_onEnqueu;
        onCreate = p_onCreate;
        initialized = true;
    }
    public void Populate(int amount)
    {
        if (!initialized) { return; }
        for (int i = 0; i < amount; i++) 
        {
            Create();
        }
    }
    public T Pop()
    {
        if (!initialized) { return null; }
        if(pool.Count == 0) 
        {
            Create();
        }
        T obj = pool.Dequeue();
        if(obj == null) 
        {
            Debug.LogError("Object you're trying to pop is null, make sure you're not destroying the object");
            return null;
        }
        onPop(obj);
        return obj;
    }
    public void Enqueu(T obj)
    {
        if (!initialized || obj == null) { return; }
        onEnqueu(obj);
        pool.Enqueue(obj);
    }
    public void Create()
    {
        var obj = Object.Instantiate(poolObject);
        pool.Enqueue(obj);
        if (onCreate != null) { onCreate(obj); }
    }
}
