using NLua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IAPILoader
{
    //Example Implementation
    /*
        public void AddAPI(Lua lua)
        {    
            new LuaAPI(lua, "shell")
                .RegisterFunction("clear", this, nameof(Clear))
                .RegisterFunction("clearLine", this, nameof(ClearLine))     

            // Global functions outside the shell table
            lua.RegisterFunction("globalName", this, typeof(Shell).GetMethod(nameof(SomeFunction)));    
        }
     */
    public void AddAPI(Lua lua);
}
