using NLua;
using UnityEngine;

public class DoorController : MonoBehaviour, IAPILoader
{
    public Transform doorModel;
    public Vector3 rotationScalar = new Vector3(0f,0f,90f);
    public bool isOpen;

    private Vector3 initialEuelrs;

    private void Start()
    {
        initialEuelrs =  doorModel.transform.rotation.eulerAngles;
    }

    //OPEN DOOR
    public void OpenExposed() => UnityMainThreadDispatcher.Enqueue(Open); //Public Exposed and calls on main thread for transform changes
    private void Open() 
    {
        if (isOpen) return;

        doorModel.transform.rotation = Quaternion.Euler(initialEuelrs + rotationScalar);    

        isOpen = true;
    }

    //CLOSE DOOR    
    public void CloseExposed() => UnityMainThreadDispatcher.Enqueue(Close); //Public Exposed and calls on main thread for transform changes
    private void Close() 
    {
        if (!isOpen) return;


        doorModel.transform.rotation = Quaternion.Euler(initialEuelrs);       

        isOpen = false;
    }

    //API 
    public void AddAPI(Lua lua)
    {
        new LuaAPI(lua, "door")
            .RegisterFunction("open", this, nameof(OpenExposed))
            .RegisterFunction("close", this, nameof(CloseExposed));
    }
}
