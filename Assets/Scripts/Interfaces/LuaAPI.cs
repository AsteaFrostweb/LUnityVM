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

    public static LuaTable ArrayToTableNamed<T>(Lua lua, T[] arr,string[] elementNames, string name)
    {
        if (arr == null || arr.Length == 0)
            return null;
        LuaTable table = Utility.CreateTable(lua, name);

        //Count from 1 for as lua counts from 1... >:C
        for (int i = 1; i <= arr.Length; i++)
        {
            table[elementNames[i - 1]] = arr[i - 1];
        }

        return table;
    }
    public static LuaTable ArrayToTable<T>(Lua lua, T[] arr, string name) 
    {
        if(arr == null || arr.Length == 0)
            return null;
        LuaTable table = Utility.CreateTable(lua, name);

        //Count from 1 for as lua counts from 1... >:C
        for (int i = 1; i <= arr.Length; i++) 
        {
            table[i.ToString()] = arr[i - 1];
        }

        return table;
    }


    //Converts Comptuer Event into a LuaTable 
    public static LuaTable ComptuerEventToTable(Lua lua, ComputerEvent ev)
    {        if (ev == null) return null;


        var table = Utility.CreateTable(lua, "event");
        table["data1"] = ev.data1;
        table["data2"] = ev.data2;
        table["data3"] = ev.data3;
        table["type"] = ev.eventType;


        return table;
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
