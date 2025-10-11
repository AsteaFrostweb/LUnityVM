using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAwaiter 
{
    public bool finished { get; private set; } = false;
    private Action action;

    public ActionAwaiter(Action _action) 
    {
        action = _action;

        UnityMainThreadDispatcher.Enqueue(() => Finish());
    }
    public void Finish() 
    {
        action.Invoke();
        finished = true;   }



}
