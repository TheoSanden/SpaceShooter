using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameObjectEvent : GameEvent
{
    private readonly List<GameObjectEventListener> eventListeners =
        new List<GameObjectEventListener>();

    public void Raise(GameObject obj)
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(obj);
    }

    public void RegisterListener(GameObjectEventListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(GameObjectEventListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}
