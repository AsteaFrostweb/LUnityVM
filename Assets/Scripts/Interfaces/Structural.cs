using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public abstract class Structural : MonoBehaviour, IAPILoader
{
    public float maxHealth { get; set; }
    //Repersents the health of the structure. 0 being destroyed and 1 being perfect condition
    public float condition { get { return health / maxHealth; } }
    public float health { get; set; }



    public bool Damage(float damage)
    {
        if (condition == 0) return true;

        if (damage >= health)
        {
            health = 0f;

            return true;
        }

        health -= damage;
        return false;
    }
    public bool Repair(float damage)
    {
        if (condition == 1f) return true;

        if (damage >= (maxHealth - health))
        {
            health = maxHealth;
            return true;
        }

        health += damage;
        return false;
    }
    
    public float GetCondition()
    {
        return condition;
    }
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    public float GetHealth()
    {
        return health;
    }
    public virtual void AddAPI(Lua lua)
    {
        new LuaAPI(lua, "structure")
            .RegisterFunction("getCondition", this, nameof(GetCondition))
            .RegisterFunction("getMaxHealth", this, nameof(GetMaxHealth))
            .RegisterFunction("getHealth", this, nameof(GetHealth));      
    }

}
