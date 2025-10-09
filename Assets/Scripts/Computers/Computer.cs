using JetBrains.Annotations;
using NLua;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
//using System.Runtime.InteropServices.WindowsRuntime;


namespace Computers
{

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
        public LuaNet network { get; private set; }

        private Screen screen;
        public EventSystem eventSystem { get; private set; } 

        public ConcurrentDictionary<string, object> sharedMemory = new ConcurrentDictionary<string, object>();

        private bool restartLocked = false;
        private bool inputLocked = false;

        public string localPath { get { return GameData.ActiveSavePath() + ID.ToString() + "/"; } }

        public string PATH { get; private set; } = "rom/programs/:rom/programs/ship/";

        public object[] defaultAPILoaders;



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
        private void InitializeScreen()
        {
            screen = new Screen(screenMesh, screenSize);
        }

        private void InitializeEventSystem()
        {
            eventSystem = new EventSystem(this);
        }
        private void InitializeNetworking()
        {
            network = new LuaNet(this);
        }

        private void InitializeShell()
        {
            currentShell = new Shell(shellInitializationString, defaultAPILoaders, this, shellLines).Start();
            shells = new List<Shell> { currentShell };

        }

        //---MONOBEHAVIOR FUNCTIONS---
        private void Start()
        {
            //Assign and register computer to main computer dictionary
            ID = inspectorID;
            RegisterComputer();

            InitializeEventSystem();
            InitializeStructural();

            InitializeNetworking();

            //Creates the screen of the computer (render texture, Canvas and camera)
            InitializeScreen();

            //Define an object array with the api loader interface to call "AddAPI" on them. Shell then creates lua enviroment
            //Would need a new "CreateLuaEnviroment" function that takes a bool as to whether or not to include shell lib


            InitializeShell(); //Finally intialize the shell         
        }

        private void Update()
        {
            if (currentShell != null)
            {
                screen.UpdateScreen(currentShell.lines, shellLines); //update screen
                shellLinesArr = currentShell.lines;   //Update inspector visible shellLines array
            }

            // --- If the player has the computer selected and is pressing: Ctrl + R ---
            if
            (
                !restartLocked && //Computer isnt locked from restarting
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
                return;
            }
            else restartInputElapsedTime = 0f;


            // Update after checking for ctrl+r press.
            // If ctrl+r is pressed then we return and do not update the event system.
            // This avoides inputting "rrrrrrrr" to shell whilst trying to restart. also blocks & hangs other events. can find better solution if it becomes a problem 
            if (!inputLocked)
                eventSystem.Update(Time.deltaTime);
        }

        //----API FUNCTIONS ----

        public override void AddAPI(Lua lua)
        {
            base.AddAPI(lua);
            new LuaAPI(lua, "os")
                .RegisterFunction("getID", this, nameof(GetID))
                .RegisterFunction("getPosition", this, nameof(GetPosition))
                .RegisterFunction("tryStore", this, nameof(TryStore))
                .RegisterFunction("tryRead", this, nameof(TryRead));    
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
        public bool TryStore(string key, object value) 
        {
            return sharedMemory.TryAdd(key, value);
        }
        public object TryRead(string key) 
        {
            object value = null;
            sharedMemory.TryGetValue(key, out value);

            return value;
        }


        private void Restart()
        {
            currentShell.Clear();
            currentShell.WriteLine("Restarting Computer...");

            restartLocked = true;
            inputLocked = true;
            StartCoroutine(RestartCoroutine());

        }
        private IEnumerator RestartCoroutine()
        {


            yield return new WaitForSeconds(0.05f);


            Stop();


            InitializeStructural(); //structural


            sharedMemory = new ConcurrentDictionary<string, object>();


            //Start new shell

            InitializeShell(); //Shell

            //Make sure we dont immediatley re-restart once we unlock the restart
            restartInputElapsedTime = 0f;

            restartLocked = false;  //Unlock the restart
            inputLocked = false; //Unlock the input
            yield return new WaitForSeconds(0.05f);
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

        private void Stop()
        {
            foreach (Shell s in shells)
            {
                s.Stop();
            }
            eventSystem.Purge();
            sharedMemory = null;
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}