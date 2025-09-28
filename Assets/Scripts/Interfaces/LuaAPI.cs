using NLua;
using System;
using System.Reflection;

public class LuaAPI
{
    private LuaTable table;
    private Lua lua;

    private string tableName = "";

    public LuaAPI(Lua lua, string _tableName)
    {
        this.lua = lua;
        this.tableName = _tableName;
        table = Utility.CreateTable(lua, tableName);
    }

    // Register a method by name using nameof
    public LuaAPI RegisterFunction(string funcName, object target, string methodName)
    {
        MethodInfo mi = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mi == null) throw new Exception($"Method not found: {methodName} on {target.GetType().Name}");

        // Register in Lua
        lua.RegisterFunction($"{tableName}.{funcName}", target, mi);

        // Store in table
        table[funcName] = lua[$"{tableName}.{funcName}"];

        return this; // allow chaining
    }


    public LuaAPI RegisterFunction(string funcName, Delegate del)
    {
        //EXAMPLE: tableName:"shell", funcName:"clear" maps the delegate method to the "shell.clear()" function in Lua Enviroment that implements this API.
        lua.RegisterFunction($"{tableName}.{funcName}", del.Target, del.Method);
        table[funcName] = lua[$"{tableName}.{funcName}"];
        return this;
    }
}
