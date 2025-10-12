using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//UNITY MAIN THREAD DISPATCHER ACTION AWAITER -  Used by UMTD AwaitAction function.
public class UMTDActionAwaiter 
{
    public bool finished { get; private set; } = false;
    private Action action;

    public UMTDActionAwaiter(Action _action) 
    {
        action = _action;

        UnityMainThreadDispatcher.Enqueue(() => Finish());
    }
    public void Finish() 
    {
        action.Invoke();
        finished = true;
    }
}
