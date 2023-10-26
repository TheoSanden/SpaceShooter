using Unity.Collections;
using System.Collections.Generic;
using Unity.Burst;
[BurstCompile]
public class NativeArrayPooler<T> where T: struct
{
    Queue<NativeArray<T>> queue = new Queue<NativeArray<T>>();
    int size;
    private NativeArrayPooler(){ }
    public NativeArrayPooler(int size)
    {
        this.size = size;
    }

    public NativeArray<T> Pop() 
    {
        if (queue.Count == 0) 
        {
            return new NativeArray<T>(size,Allocator.Persistent);
        }
        return queue.Dequeue();
    }
    public void Queue(NativeArray<T> obj) 
    {
        queue.Enqueue(obj);
    }

    public void ClearAll() 
    {
        foreach (NativeArray<T> obj in queue) 
        {
            obj.Dispose();
        }
        queue.Clear();
    }
}
