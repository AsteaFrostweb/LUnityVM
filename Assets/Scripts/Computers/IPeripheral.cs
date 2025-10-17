using NLua;
using System;
using UnityEngine;

public interface IPeripheral 
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string UID { get; set; }

   //Must be called to generate UID when the perhipheral is created either in constructor or in Start if MonoBehaviour
    public void Inititalize() 
    {
        UID = UIDGenerator.GenerateUID();
    }

    public virtual void ToLuaTable(Lua lua, out LuaTable table) 
    {
        table = Utility.CreateTable(lua);
        table["uid"] = new Func<string>(() => UID);
        table["name"] = new Func<string>(() => Name);
        table["description"] = new Func<string>(() => Description);
    }  
}
