using NLua;
using static System.Convert;
using Unity.VisualScripting;
using UnityEngine;

public class LuaNet : IAPILoader
{
    private Computer host;

    public LuaNet(Computer _host) 
    {
        host = _host;
    }

    public void AddAPI(Lua lua)
    {
        new LuaAPI(lua, "lnet")
           .RegisterFunction("send", this, nameof(Send))
           .RegisterFunction("receive", this, nameof(ReceiveExposed));
    }    

    //Adds a "luanet" ComputerEvent to the computer with a given ID
    private void Send(int receiverID, int port, object data)
    {
        //Check is host is network capable
        if (!host.HasNetworkDevice()) 
        {
            host.currentShell.WriteLineError("Unable to send LNet packet: No network device detected");
            return;
        }

        //Check if the receiver is network capable
        Computer receiver = Computer.FindByID(receiverID);
        if (!receiver.HasNetworkDevice())
        {
            host.currentShell.WriteLineError("Unable to send LNet packet: No network device detected on recipient");
            return;
        }


        
        //Add a "luanet_receive" event to the receiver computers event system.
        receiver.eventSystem.AddEvent(new ComputerEvent("luanet_receive", port, data, host.ID, 5f));

        //Add a "luanet_send" event to the host's event system.
        host.eventSystem.AddEvent(new ComputerEvent("luanet_send", port, data, null, 2f));
    }



    private LuaTable ReceiveExposed(int port, double timeout) 
    {
        ComputerEvent ce = Receive(port, ToSingle(timeout));
        if (ce == null)
            return null;
        
        return LuaAPI.ComptuerEventToTable(host.currentShell.enviroment, ce);
    }
    private ComputerEvent Receive(int port, float timeout) 
    {
        float elapsed = 0f;
        ComputerEvent _event = null;

        if (!host.HasNetworkDevice()) 
        {
            host.currentShell.WriteLineError("Unable to reveice LNet packet: No network device detected");
            return null;
        }

        float waitTime = 0.1f; //how long to wait between while loop passes
        while (elapsed < timeout && _event == null)
        {
            //Attempt to get event of type luanet_receiv on the given port
            _event = host.eventSystem.PullEvent("luanet_receive", c => (int)c.data1 == port);

            //Continuer if the event was found
            if (_event != null)
                break;

            //Wait and increment timer before next loop
            host.currentShell.Sleep(waitTime);
            elapsed += waitTime;

        }

        if (_event == null) return null;

        return _event;
    }
}
