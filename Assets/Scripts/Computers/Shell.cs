
using NLua;
using NLua.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.Rendering;

public class Shell : IAPILoader
{
    public const int DEFAULT_LINE_NUMBER = 20;
    
    public const int DEFAULT_FONT_SIZE = 20;


    

    public Lua enviroment { get; private set; }
    public Computer host { get; private set; }    

    public string[] lines;
    private int cursorY = 0;
    private bool cursorBlink = true;
    public int lineNumber { get; private set; } = DEFAULT_LINE_NUMBER;

    //TASK SHIZZ
    private Task mainTask;
    CancellationTokenSource cts;
    CancellationToken cancelationToken => cts.Token;

    private string shellInitializationString;

    private string currentDirectory = "";
    public string currentDirectoryFullPath { get { return Path.Combine(host.localPath, currentDirectory); } }
    private Dictionary<string, Action<string[]>> shellBaseCommands;

    //represent weather the _ is current added to the end of lines[cursorY]
    private bool cursorActive = false;
    public Shell(string _shellInitializationString)
    {
        enviroment = new Lua();
        host = null;
        lineNumber = 10; // default if not set
        lines = new string[lineNumber];
        for (int i = 0; i < lineNumber; i++)
            lines[i] = "";

        shellInitializationString = _shellInitializationString;
        InitializeShellBaseCommands();
    }

    // Constructor with API loaders and host
    public Shell(string _shellInitializationString, object[] apiLoaders, Computer _host)
        : this(_shellInitializationString) // calls the base constructor first
    {
        enviroment = CreateLuaEnviroment(apiLoaders);
        host = _host;
    }

    // Constructor with API loaders, host, and line count
    public Shell(string _shellInitializationString, object[] apiLoaders, Computer _host, int lineCount)
        : this(_shellInitializationString, apiLoaders, _host) // calls previous constructor
    {
        lineNumber = lineCount;
        lines = new string[lineCount];
        for (int i = 0; i < lineCount; i++)
            lines[i] = "";
    }

    public Shell Start() 
    {
        //start the main task
        cts = new CancellationTokenSource();
        mainTask = Task.Run(() => Main(), cancelationToken);
        
        return this;
    } 
    public void Stop() 
    {
        cts.Cancel();      
    }


    public void Main() 
    {
        WriteLine(shellInitializationString);
        while (true) 
        {
            Write($"<color=yellow>{currentDirectory}\\:</color>");
            string cmd = ReadLine();
            ParseCommand(cmd);
        }
    }


    private void InitializeShellBaseCommands() 
    {
        shellBaseCommands = new Dictionary<string, Action<string[]>>()
        {
            {"ls", ListDirectory },
            {"cd", ChangeDirectory },
            { "cls", CLS }
        };
    }
    private bool ParseCommand(string s) 
    {   

        string[] args = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);       
        if (args == null) return true;
        if (args.Length <= 0) return true;

        //If the shellBaseCommands dictionary contains the command entered
        if (shellBaseCommands != null && shellBaseCommands.ContainsKey(args[0]))
        {
            Action<string[]> action = shellBaseCommands[args[0]];
            action?.Invoke(args);
            return true;
        }
        else if(FindAndRunFile(args[0]))
        {
            return true;
        }
        else
        {
            WriteLineError($"No command found: {args[0]}");
            return false;
        }       
    }

    private bool FindAndRunFile(string filePath) 
    {

        string dirPath = currentDirectoryFullPath;
        
        if ((File.Exists(Path.Combine(dirPath, filePath))) && (Path.GetExtension(filePath) == ".lua")) 
        {
            Run(Path.Combine(currentDirectory, filePath));
            return true;
        }
        Debug.Log("Splitting host path:" + host.PATH);
        string[] paths = host.PATH.Split(":");
        foreach (string s in paths) 
        {            
            string path = Path.Combine(s, filePath);
            Debug.Log("Checking if file exists: " + Path.Combine(host.localPath,path));
            if (Path.GetExtension(filePath) == ".lua" && File.Exists(Path.Combine(host.localPath, path)))
            {
                Debug.Log("Runnig file at path: " + path);
                Run(path);
                return true;
            }         
        }

        return false;
    }

    public void GotoParentDirectory() 
    {
       
        if (currentDirectory == "") return;//returnm if current directoryt is empty (we're at the root)

        //get the parent directory of our current directory
        string trimmedPath = Path.GetDirectoryName(currentDirectory);
       
        //If the parent doesnt equal null then set it to be our currenbt directory
        if (trimmedPath != null) currentDirectory = trimmedPath;
    }

    private void ListDirectory(string[] args)
    {
        Debug.Log("Attemptiong to get files for directory:" + currentDirectory);
        string[] files = host.fileSystem.GetFiles(currentDirectory);
        Debug.Log("Got files for directory:" + currentDirectory);
        Debug.Log("Attemptiong to get Directorys for directory:" + currentDirectory);
        string[] dirs = host.fileSystem.GetDirectories(currentDirectory);
        Debug.Log("Got Directorys for directory:" + currentDirectory);
        // Combine directories and files into a single list
        var combinedList = dirs.Concat(files).ToArray();

        // Sort the combined list alphabetically
        var sortedList = combinedList.OrderBy(item => item, StringComparer.OrdinalIgnoreCase).ToArray();


        foreach (string s in sortedList) 
        {
            if (dirs.Contains(s))
            {
                Write($"<color=green>{s}</color>    ");
            }
            else 
            {
                Write($"<color=blue>{s}</color>    ");
            }            
        }
        WriteLine("");
    }
    private void ChangeDirectory(string[] args)
    {
        
        if (args.Length < 2) 
        {
            WriteLineError($"syntax: cd [path]");
            return;
        }

        if (args[1].Substring(0, 1) == "\"") 
        {
            //debug log to explain current limitation of shell program I don't intend to extend right now.
            WriteLineError($"Unable to accept quote encapsulated arguments.\n" +
                      $"Arguments are split by space characters in shell. \n" +
                      $"If you MUST do space seperated files/directory names you can use the \"shell\" API in the lua terminal.");

            return;
        }

        Utility.TrimAndRemoveAllFirst(ref args[1], '.', () => GotoParentDirectory(), 1);
        if (args[1] == "") return;

        Utility.FormatPathPreCombine(ref args[1], () => { currentDirectory = ""; }, 0);
        if (args[1] == "") return;


        string localPath = currentDirectoryFullPath;
        string path = Path.Combine(localPath, args[1]);
       
        if (Directory.Exists(path))
        {
            currentDirectory = Path.Combine(currentDirectory, args[1]);
        }
        else 
        {
            WriteLineError($"Unable to locate directory at path: [{Path.Combine(currentDirectory, args[1])}]");
        }
    }




    private Lua CreateLuaEnviroment(object[] apiLoaders)
    {
        //Create lua enviroment
        Lua lua = new Lua();
    

        //Loop through and add each APILoaders object library to the lua enviroment
        foreach (object loader in apiLoaders)
        {
            if (loader is IAPILoader)
            {
                (loader as IAPILoader).AddAPI(lua);
            }
        }

        AddAPI(lua);

        //return created enviroment
        return lua;
    }


    public void Run(string fileName) 
    {
        string filePath = Path.Combine(host.localPath, fileName);
        if (File.Exists(filePath) && Path.GetExtension(filePath) == ".lua")
        {
            //enviroment.DoFile(host.localPath + fileName);
            try
            {
                enviroment.DoFile(host.localPath + fileName);
            }
            catch (LuaException ex) 
            {
                //remove the root path from message error
                //Then append it onto out current virtual directory so error message appears local to machine
                
                int lastSlashIndex = ex.Message.LastIndexOfAny(new char[] { '/', '\\' });
                string trimmedMessage = lastSlashIndex > 0 ? ex.Message.Substring(lastSlashIndex + 1) : ex.Message;
                string finalMessage = currentDirectory != "" ? currentDirectory + "/" + trimmedMessage : trimmedMessage;
                WriteLineError("LuaScriptException: " + finalMessage);
                WriteLineError(ex.InnerException.Message.ToString());
                //
                WriteLineError(ex.InnerException.StackTrace.ToString());
            }
            
        }      
    }


    public void Write(string s) 
    {
        //Append s to line[cursorY]
        if (cursorActive)
        {
            lines[cursorY] = lines[cursorY].Substring(0, Mathf.Max(0, lines[cursorY].Length - 1));
            cursorActive = false;
        }
        lines[cursorY] += s;     
    }
    public void WriteLine(string s) 
    {
        Debug.Log("writing :" + s + " to line: " + cursorY.ToString());
      
        if (cursorActive)
        {
           
            lines[cursorY] = lines[cursorY].Substring(0, Mathf.Max(0, lines[cursorY].Length - 1));
            cursorActive = false;
        }
        //Append s to line[cursorY] and increment cursorY by +1 
        if (cursorY >= lines.Length - 1)
        {
            lines = Utility.ShiftArray<string>(lines, "");
            Debug.Log("Setting line :" + (lines.Length - 2).ToString() + " to value: " + s);
            lines[lines.Length - 2] += s;
        }
        else 
        {
            lines[cursorY] += s;
            cursorY++;
        }      
    }
    public void WriteLineError(string s) 
    {
        WriteLine($"<color=red>Error</color> - " + s);
    }



    public string ReadLine()
    {
        char previousKey = '\0';

        string total = "";
        
        float repeatRateHoldMin = 0.05f;
        float repeatRateHoldMax = 0.3f;
        float repeatRateDecrease = 0.05f;
        float repeatRateHold = repeatRateHoldMax;  
        float repeatRateDown = 0.05f; // Time between repeated keypresses (in seconds)        



        // Dictionary to track the state and last press time of each key
        Dictionary<char, DateTime> keyStates = new Dictionary<char, DateTime>();

        while (true)
        {
            
            bool keyDown = false;
            char c = Read(out keyDown); //Get a chacter key_down or key_hold from the event queue

            DateTime currentTime = DateTime.Now;
            //Getting a copy of current time to ensure all math done is consisitent despite possible thread stoppages
       
            //Resetting the key repeat rate if the keypress is a keydown event
            if (keyDown) repeatRateHold = repeatRateHoldMax;

            // Returning if the user pressed enter and it is a keydown event. continuing if the Enter was a subsiquent keyhold event
            if (c == '\r')
            {
                if (keyDown)
                {
                    WriteLine("");
                    break;
                }
                else continue;
            }


            // Handle other keys
            if (keyStates.ContainsKey(c) || keyStates.ContainsKey(char.ToUpper(c)))
            {

                // Check if enough time has elapsed for a repeat                
                bool ltimeElapsedHold = keyStates.ContainsKey(c) ? (currentTime - keyStates[c]).TotalSeconds > repeatRateHold : true;
                bool ltimeElapsedDown = keyStates.ContainsKey(c) ? (currentTime - keyStates[c]).TotalSeconds > repeatRateDown : true;
                bool utimeElapsedHold = keyStates.ContainsKey(char.ToUpper(c)) ? (currentTime - keyStates[char.ToUpper(c)]).TotalSeconds > repeatRateHold : true;
                bool utimeElapsedDown = keyStates.ContainsKey(char.ToUpper(c)) ? (currentTime - keyStates[char.ToUpper(c)]).TotalSeconds > repeatRateDown : true;

                bool timeElapsedHold = ltimeElapsedHold && utimeElapsedHold;
                bool timeElapsedDown = ltimeElapsedDown && utimeElapsedDown;

                //if enough time has elapsed between the last key pres and now or it is a keydown 
                if ((timeElapsedHold && !keyDown) || (timeElapsedDown && keyDown))
                {
                    
                    //reduce the repeat rate if the last key is the same as the current key
                    if (c == previousKey && !keyDown)
                    {
                        repeatRateHold = Mathf.Max(repeatRateHold - repeatRateDecrease, repeatRateHoldMin);                   
                    }
                    else
                    {
                        repeatRateHold = repeatRateHoldMax;
                    }

                    //Update previous key       
                    previousKey = c;

                    //Update the current characters assositated last press time
                    keyStates[c] = currentTime;

                    //Handling logging and appending of char
                    switch (c) 
                    {
                        case '\b':
                                if (total.Length > 0)
                                {
                                    total = total.Substring(0, total.Length - 1);
                                    lines[cursorY] = lines[cursorY].Substring(0, lines[cursorY].Length - 1);
                                }
                            break;

                        default:
                                total += c;
                                Write(c.ToString());
                            break;
                    }
                  
                }
            }
            else
            {

                //Update previous key       
                previousKey = c;

                //Update the current characters assositated last press time
                keyStates[c] = currentTime;

                //Handling logging and appending of char
                switch (c)
                {
                    case '\b':
                        if (total.Length > 0)
                        {
                            total = total.Substring(0, total.Length - 1);
                            lines[cursorY] = lines[cursorY].Substring(0, lines[cursorY].Length - 1);
                        }
                        break;

                    default:
                        total += c;
                        Write(c.ToString());
                        break;
                }
            }
        }

        return total;
    }


    public char Read(out bool key_down)
    {
        while (true)
        {
            if (GameData.currentFocus.inputFocus == InputFocus.COMPUTER && GameData.currentFocus.identifier == host.ID.ToString())
            {
                ComputerEvent ev = host.eventSystem.PullEvent("key_down", "key_hold");
                if (ev == null) continue;

                if ((char)ev.data1 != '\0')
                {
                    //Debug.Log("Returning char from computer event: " + ev.eventType + "  " + ev.data1);
                    key_down = ev.eventType == "key_down";
                    return (char)ev.data1;
                }
            }
            Sleep(0.01f);
        }
    }
    public char ReadChar() 
    {  
        while (true)
        {
            if (GameData.currentFocus.inputFocus == InputFocus.COMPUTER && GameData.currentFocus.identifier == host.ID.ToString())
            {
                ComputerEvent ev = host.eventSystem.PullEvent("key_down");
                if (ev == null) continue;
                                
                if ((char)ev.data1 != '\0') 
                {
                    //Debug.Log("Returning char from computer event: " + ev.eventType + "  " + ev.data1);
                    return (char)ev.data1;                    
                }
            }
            Sleep(0.01f);
        }
    }

    //used for binding to action<string[]> for shell commands dictionary
    private void CLS(string[] args) => Clear();
    public void Clear() 
    {
        int lineCount = lines.Length;
        lines = new string[lineCount];
        cursorY = 0;
    }

    public void ClearLine(int index)
    {
        int trueIndex = index - 1;
        lines[trueIndex] = "";  
    }


    public void Sleep(float seconds)
    {
        System.Threading.Thread.Sleep((int)(seconds * 1000));
    }

    public void AddAPI(Lua lua)
    {
        new LuaAPI(lua, "shell")
            .RegisterFunction("clear", this, nameof(Clear))
            .RegisterFunction("clearLine", this, nameof(ClearLine))
            .RegisterFunction("read", this, nameof(ReadChar))
            .RegisterFunction("readLine", this, nameof(ReadLine))
            .RegisterFunction("run", this, nameof(Run));      

        // Global functions outside the shell table
        lua.RegisterFunction("print", this, typeof(Shell).GetMethod(nameof(Write)));
        lua.RegisterFunction("printLine", this, typeof(Shell).GetMethod(nameof(WriteLine)));
        lua.RegisterFunction("sleep", this, typeof(Shell).GetMethod(nameof(Sleep)));
    }



    private bool IsCharacterKey(KeyCode keyCode)
    {
        return CharacterKeys.Contains(keyCode);
    }

    private readonly HashSet<KeyCode> CharacterKeys = new HashSet<KeyCode>
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
        KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
        KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
        KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
        KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
    };
}
