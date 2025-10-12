using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance { get; private set; }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInstance()
    {
        if (Instance == null)
        {
            Instance = new GameObject("MainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(Instance.gameObject);
        }   
    }

    public static void Enqueue(Action action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }

  
    public static void AwaitAction(Action a)
    {
        ActionAwaiter awaiter = new ActionAwaiter(a);

        while (!awaiter.finished) { }
    }
}
