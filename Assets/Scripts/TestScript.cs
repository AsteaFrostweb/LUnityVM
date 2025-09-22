using UnityEngine;
using NLua;
public class TestScript : MonoBehaviour
{
    public string LuaScript = "return 2 + 2";
    Lua luaEnviroment;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        luaEnviroment = new Lua();
    }

    // Update is called once per frame
    void Update()
    {
        if (luaEnviroment == null) 
        {
            Debug.LogError("Lua enviroment not initialized");
            return;
        }
        var val = luaEnviroment.DoString(LuaScript);
        Debug.Log(val[0]);
    }
}
