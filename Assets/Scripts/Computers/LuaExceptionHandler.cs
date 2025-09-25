using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaExceptionHandler<T> where T : struct
{
    public string message { get; private set; }
    public T? returnValue;
}
