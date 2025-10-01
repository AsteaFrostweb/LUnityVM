using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;
using Unity.VisualScripting;
using System.IO.Enumeration;
using System.IO;
using System.Runtime.CompilerServices;
using System;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;
using TMPro;
using JetBrains.Annotations;
using UnityEngine.InputSystem;
//using System.Runtime.InteropServices.WindowsRuntime;




public class Computer : Structural
{   
    [Serializable]
    public enum State { ON, OFF }    
 
    public int inspectorID;
    public float inspectorMaxHealth;
    public Computer.State state;   
    public MeshRenderer screenMesh;
    public int2 screenSize;
    public int shellLines = Shell.DEFAULT_LINE_NUMBER;
    public string shellInitializationString = "Welcome to LUnity OS v1.0.1";
    public string visibleSavePath = "NONE";
    public float restartTime = 2.0f;
    private float restartInputElapsedTime = 0f;

    [Header("Read Only")]
    public string[] shellLinesArr;

    //---NON INSPECTOR---

    public static Dictionary<int, Computer> computers = new Dictionary<int, Computer>();

    public int ID { get; private set; } 
    private List<Shell> shells;
    public Shell currentShell { get; private set; }
    public  FileSystem fileSystem { get; private set; }

    public LuaNet network { get; private set; }

    private ComputerScreen screen;  
    public ComputerEventSystem eventSystem { get; private set; }

    private bool restartLocked = false;

    public string localPath { get { return GameData.ActiveSavePath() + ID.ToString() + "/"; } }

    public string PATH { get; private set; } = "rom/programs/:rom/programs/ship/";

    //Function to set this pc as the current input focus
    public void SetFocus()
    {
        GameData.currentFocus = new Focus(InputFocus.COMPUTER, ID.ToString());
    }

    //---Initalizing methods---
    private void InitializeStructural() 
    {              
        maxHealth = inspectorMaxHealth;
        health = maxHealth;
    }
    private void InitializeFileSystem() 
    {
        fileSystem = new FileSystem(localPath, this);
        visibleSavePath = localPath;
    }

    private void InitializeEventSystem() 
    {
        eventSystem = new ComputerEventSystem(this);
    }
    private void InitializeNetworking() 
    {
        network = new LuaNet(this);
    }

    //---MONOBEHAVIOR FUNCTIONS---
    private void Start()
    {
        //Assign and register computer to main computer dictionary
        ID = inspectorID;
        RegisterComputer();

        InitializeEventSystem();       
        InitializeStructural();
        InitializeFileSystem();
        InitializeNetworking();

        //Creates the screen of the computer (render texture, Canvas and camera)
        Debug.Log(screenMesh.ToString() + screenSize.ToString());
        screen = new ComputerScreen(screenMesh, screenSize);

        //Define an object array with the api loader interface to call "AddAPI" on them. Shell then creates lua enviroment
        //Would need a new "CreateLuaEnviroment" function that takes a bool as to whether or not to include shell lib
        object[] defaultAPILoaders = { fileSystem, this, network};

        currentShell = new Shell(shellInitializationString, defaultAPILoaders, this, shellLines).Start();
        shells = new List<Shell> { currentShell };

       
         
    }

    private void Update()
    {
        shellLinesArr = currentShell.lines;
        eventSystem.Update(Time.deltaTime);
   
        //Debug.Log("Shell: " + currentShell.ToString() + "  screen: " + screen.ToString());
        screen.UpdateScreen(currentShell.lines, shellLines);

        //If the player has the computer selected and is pressing: Ctrl + R
        if
        (
            IsCurrentFocus() && //If the player is focusing this computer 
            Input.GetKey(KeyCode.R) && //Is holding R
           (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) //Is holding ctrl
        )
        {
            restartInputElapsedTime += Time.deltaTime;
            if (restartInputElapsedTime >= restartTime)
            {
                Restart();
            }
        }
        else restartInputElapsedTime = 0f;
    }

    //----API FUNCTIONS ----

    public override void AddAPI(Lua lua)
    {
        base.AddAPI(lua);
        LuaTable osAPI = Utility.CreateTable(lua, "os");
        //
        osAPI["ID"] = lua.RegisterFunction("os.getID", this, typeof(Computer).GetMethod("GetID"));
        osAPI["getPosition"] = lua.RegisterFunction("os.getPosition", this, typeof(Computer).GetMethod("GetPosition"));
    }

    public int GetID()
    {
        return ID;
    }
    public LuaTable GetPosition()
    {
        Vector3 position = new Vector3();

        ActionAwaiter.AwaitAction(() => { position = transform.position; });   //Waits for the main thread to preform the action specified before continuing. like a baws


        LuaTable table = Utility.CreateTable(currentShell.enviroment, "position");
        table["x"] = position.x;
        table["y"] = position.y;
        table["z"] = position.z;
        return table;
    }

    private void Restart() 
    {
        currentShell.WriteLine("Restarting Computer...");
    }


    public bool RegisterComputer() 
    {
        return computers.TryAdd(ID, this);
    }
    public static Computer FindByID(int id) 
    {
        Computer ans = null;
        computers.TryGetValue(id, out ans);
        return ans;
    }

    public bool IsCurrentFocus() => GameData.currentFocus.Equals(InputFocus.COMPUTER, ID.ToString());

    public bool HasNetworkDevice() 
    {
        return true;
    }


    private void OnDestroy()
    {
        foreach (Shell s in shells) 
        {
            s.Stop();
        }
    }
}
