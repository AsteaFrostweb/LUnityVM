
using NLua;
using NLua.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

namespace Computers
{
    public class Shell : IAPILoader
    {
        // CONSTANTS
        public const int DEFAULT_LINE_NUMBER = 20;
        public const int DEFAULT_FONT_SIZE = 20;

        //Random Unique shell ID
        public string UID { get; private set; } = "NULL";


        public Lua enviroment { get; private set; }
        public Computer host { get; private set; }
        private FileSystem fileSystem;


        public string[] lines;
        private int cursorY = 0;

        public int lineNumber { get; private set; } = DEFAULT_LINE_NUMBER;

        //TASK SHIZZ
        private Thread mainThread;
        public bool isRunning => mainThread != null;
        CancellationTokenSource cts;


        private string shellInitializationString;

        private string currentDirectory = "";
        public string CurrentDirectoryExposed() => currentDirectory;
        public string currentDirectoryFullPath { get { return Path.Combine(host.localPath, currentDirectory); } }
        private Dictionary<string, Action<string[]>> shellBaseCommands;

        public object[] defaultAPILoaders;

        //represent weather the _ is current added to the end of lines[cursorY]
        private bool cursorActive = false;


        // ------------------------ CONSTRUCTORS -----------------------

        public Shell(string _shellInitializationString, Computer _host)
        {

            host = _host;
            UID = UIDGenerator.GenerateUID();

            InitializeFileSystem(_host);
            InitializeDefualtAPILoaders();
            InitializeLuaEnviroment(defaultAPILoaders);

            lineNumber = 10; // default if not set
            lines = new string[lineNumber];
            for (int i = 0; i < lineNumber; i++)
                lines[i] = "";

            shellInitializationString = _shellInitializationString;
            InitializeShellBaseCommands();
        }
        // Constructor with API loaders, host, and line count
        public Shell(string _shellInitializationString, object[] apiLoaders, Computer _host, int lineCount)
            : this(_shellInitializationString, _host) // calls previous constructor
        {
            host = _host;
            lineNumber = lineCount;
            lines = new string[lineCount];
            for (int i = 0; i < lineCount; i++)
                lines[i] = "";
        }


        // ------------------------ STOP/START -----------------------
        public Shell Start()
        {
            cts = new CancellationTokenSource();

            mainThread = new Thread(Main);
            mainThread.IsBackground = true;
            mainThread.Start();

            return this;
        }
        public void Stop()
        {
            Debug.Log($"[{UID}]Stopping shell!");

            fileSystem.CloseStream();

            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();

            if (mainThread != null && mainThread.IsAlive)
            {
                // Wait up to 500ms for cooperative exit
                if (!mainThread.Join(500))
                {
                    Debug.LogWarning($"[{UID}] Shell thread didn’t stop, aborting...");
                    try { mainThread.Abort(); } catch { }
                }
            }

            mainThread = null;
        }



        // ------------------------ SHELL MAIN LOOP -----------------------
        public void Main()
        {
            Sleep(0.5f);
            WriteLine(shellInitializationString);
            while (!cts.IsCancellationRequested)
            {
                Write($"<color=yellow>{currentDirectory}\\:</color>");
                Sleep(0.2f); //Small delay for preventing re-reading of previos keydown events
                string cmd = ReadLine();
                ParseCommand(cmd);
            }
        }


        // ------------------------ INITIALIZATION -----------------------
        private void InitializeDefualtAPILoaders()
        {
            defaultAPILoaders = new object[] { fileSystem, this, host.network };
        }
        private void InitializeFileSystem(Computer _host)
        {

            fileSystem = new FileSystem(_host.localPath, this);
            _host.visibleSavePath = _host.localPath;
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
        private void InitializeLuaEnviroment(object[] apiLoaders)
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
            enviroment = lua;
        }




        // ------------------------ DIRECTORY TRAVERSAL -----------------------
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
            Debug.Log(host == null);
            Debug.Log(fileSystem == null);
            string[] files = fileSystem.GetFiles(currentDirectory);
            Debug.Log("Got files for directory:" + currentDirectory);
            Debug.Log("Attemptiong to get Directorys for directory:" + currentDirectory);
            string[] dirs = fileSystem.GetDirectories(currentDirectory);
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



        // ------------------------ WRITE FUNCTIONS -----------------------

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
            Debug.Log($"[{UID}][isMainShell:{this == host.currentShell}]writing :" + s + " to line: " + cursorY.ToString());

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


        // ------------------------ READ FUCTIONS -----------------------

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

            while (!cts.IsCancellationRequested)
            {

                bool keyDown = false;
                char c = Read(out keyDown); //Get a chacter key_down or key_hold from the event queue

                DateTime currentTime = DateTime.Now;
                //Getting a copy of current time to ensure all math done is consisitent despite possible thread stoppages

                //Resetting the key repeat rate if the keypress is a keydown event
                if (keyDown) repeatRateHold = repeatRateHoldMax;

                // Returning if the user pressed enter and it is a keydown event. continuing if the Enter was a subsiquent keyhold event
                // Also continuing if the key returned is '\0' as this represents a null character. soemtimes returned if cts is canceled or if shit goes south [Change for more rhobust solution plz]
                if (c == '\r' || c == '\0')
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
            while (!cts.IsCancellationRequested)
            {
                if (host.IsFocus())
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
            key_down = false;
            return '\0';
        }
        public char ReadChar()
        {
            while (true)
            {
                if (host.IsFocus())
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



        // ------------------------ CLEAR / DELETE FUNCTIONS ---
        //args param only used for binding to action<string[]> for shell commands dictionary
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



        // ------------------------ UTILITY -----------------------

        // LOOKS FOR FILES WITH PATH OF FILEPATH AND ALSO APPENDS THE FILEPATH TO EACH SECTION OF THE "PATH" VARIABLE
        // AND ALSO LOOKS IN THOSE LOCATOINS FOR FILES TO RUN
        private bool FindAndRunFile(string filePath)
        {

            string dirPath = currentDirectoryFullPath;

            if ((File.Exists(Path.Combine(dirPath, filePath))) && (Path.GetExtension(filePath) == ".lua"))
            {
                Run(Path.Combine(currentDirectory, filePath));
                return true;
            }
            //Debug.Log("Splitting host path:" + host.PATH);
            string[] paths = host.PATH.Split(":");
            foreach (string s in paths)
            {
                string path = Path.Combine(s, filePath);
                //Debug.Log("Checking if file exists: " + Path.Combine(host.localPath,path));
                if (Path.GetExtension(filePath) == ".lua" && File.Exists(Path.Combine(host.localPath, path)))
                {
                    Debug.Log("Runnig file at path: " + path);
                    Run(path);
                    return true;
                }
            }

            return false;
        }


        //Runs a given filename via the Lua.DoFIle functoin handling exceptions and printing them to screen where necissary
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
                    if (ex.InnerException != null)
                        WriteLineError(ex.InnerException.Message.ToString());
                    //
                    //WriteLineError(ex.InnerException.StackTrace.ToString());
                }

            }
        }

        //Sleeps for seconds
        public void Sleep(float seconds)
        {
            System.Threading.Thread.Sleep((int)(seconds * 1000));
        }

        //Parses shell command using shellBaseCommands dictionary
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
            else if (FindAndRunFile(args[0]))
            {
                return true;
            }
            else
            {
                WriteLineError($"No command found: {args[0]}");
                return false;
            }
        }


        // ----------------------- LUA API -----------------------
        public void AddAPI(Lua lua)
        {
            new LuaAPI(lua, "shell")
                .RegisterFunction("currentDirectory", this, nameof(CurrentDirectoryExposed))
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




    }
}
