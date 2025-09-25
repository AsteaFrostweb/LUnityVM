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
using System.Runtime.InteropServices.WindowsRuntime;




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
   
    //---NON INSPECTOR---
    public int ID { get; private set; } 
    private List<Shell> shells;
    public Shell currentShell { get; private set; }
    public  FileSystem fileSystem { get; private set; }

    private ComputerScreen screen;  
    public ComputerEventSystem eventSystem { get; private set; }

   
    public string localPath { get { return GameData.ActiveSavePath() + ID.ToString() + "/"; } }

    public string PATH { get; private set; } = "rom/programs/:rom/programs/ship/";

    //Function to set this pc as the current input focus
    public void SetFocus()
    {
        GameData.currentFocus = new Focus(InputFocus.COMPUTER, ID.ToString());
    }

    //---Initalizing methods---
    public void InitializeStructural() 
    {              
        maxHealth = inspectorMaxHealth;
        health = maxHealth;
    }
    public void InitializeFileSystem() 
    {
        fileSystem = new FileSystem(localPath);
        visibleSavePath = localPath;
    }

    public void InitializeEventSystem() 
    {
        eventSystem = new ComputerEventSystem(this);
    }

    //---MONOBEHAVIOR FUNCTIONS---
    private void Start()
    {
        
        ID = inspectorID;

        InitializeEventSystem();       
        InitializeStructural();
        InitializeFileSystem();

        //Creates the screen of the computer (render texture, Canvas and camera)
        Debug.Log(screenMesh.ToString() + screenSize.ToString());
        screen = new ComputerScreen(screenMesh, screenSize);

        //Define an object array with the api loader interface to call "AddAPI" on them. Shell then creates lua enviroment
        //Would need a new "CreateLuaEnviroment" function that takes a bool as to whether or not to include shell lib
        object[] defaultAPILoaders = { fileSystem, this };

        currentShell = new Shell(shellInitializationString, defaultAPILoaders, this, shellLines).Start();
        shells = new List<Shell> { currentShell };

       
         
    }

    private void Update()
    {
        eventSystem.Update(Time.deltaTime);
   
        //Debug.Log("Shell: " + currentShell.ToString() + "  screen: " + screen.ToString());
        screen.UpdateScreen(currentShell.lines, shellLines);
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

}
