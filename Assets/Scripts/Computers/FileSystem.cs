using NLua;
using NLua.Exceptions;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.Handles;

namespace Computers
{
    public class FileSystem : IAPILoader
    {
        private Shell host;
        private string rootPath;
        private FileStream currentStream;

        public FileSystem(string rootpath, Shell _host)
        {
            host = _host;
            Debug.Log("FS const: directory exists?");
            rootPath = Path.GetFullPath(rootpath);
            if (!Directory.Exists(rootPath))
            {
                Debug.Log("Creating computer file system at location: " + rootPath);
                Directory.CreateDirectory(rootPath + "rom/programs");
                Debug.Log("Copying base rom programs from game install");
            }
            else { Debug.Log("FS const: directory exists: YES,   rootpath:" + rootPath); }
        }

        public bool CreateDirectory(string path)
        {

            if (!Directory.Exists(Path.Combine(rootPath, path)))
            {

            }
            else
            {
            }
            return false;
        }

        public string[] GetFiles(string path)
        {
            Debug.Log("PreFormatting path: " + path);
            Utility.FormatPathPreCombine(ref path, () => { }, 0);
            Debug.Log("PreFormatted Path Sucesffuly: " + path);
            string localPath = path == "" ? rootPath : Path.Combine(rootPath, path);
            Debug.Log("LS Local path:" + localPath);
            localPath = Path.GetFullPath(localPath);
            Debug.Log("LS Local path(full):" + localPath);
            if (!Directory.Exists(localPath)) return null;


            string[] files = Directory.GetFiles(localPath);

            //regex to extract only the file names
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Regex.Match(files[i], @"[^/\\]+$").Value;
            }

            return files;
        }

        public LuaTable GetDirectoriesExposed(string path)
        {
            return LuaAPI.ArrayToTable<string>(host.enviroment, GetDirectories(path), "");
        }
        public string[] GetDirectories(string path)
        {

            Utility.FormatPathPreCombine(ref path, () => { }, 0);
            string localPath = Path.Combine(rootPath, path);
            if (!Directory.Exists(localPath)) return null;


            string[] directories = Directory.GetDirectories(localPath);

            //regex to extract only the last part of each directory path
            for (int i = 0; i < directories.Length; i++)
            {
                directories[i] = Regex.Match(directories[i], @"[^/\\]+$").Value;
            }

            return directories;
        }

        public bool OpenFile(string path, string readWrite)
        {

            Utility.FormatPathPreCombine(ref path, () => { }, 0);
            string localPath = Path.Combine(rootPath, path);

            //Checking if the file exists
            if (File.Exists(localPath))
            {
                //If there is a currently active stream then close it 
                if (currentStream != null) currentStream.Close();
                currentStream = null;

                switch (readWrite)
                {
                    case "read":
                        try
                        {
                            currentStream = File.Open(localPath, FileMode.Open, FileAccess.Read);
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("Exception while trying to open file:" + e.ToString());
                            host.WriteLineError("Exception while trying to open file:" + e.ToString());
                            return false;
                        }
                        break;
                    case "write":
                        try
                        {
                            currentStream = File.Open(localPath, FileMode.Open, FileAccess.Write);
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("Exception while trying to open file:" + e.ToString());
                            host.WriteLineError("Exception while trying to open file:" + e.ToString());
                            return false;
                        }
                        break;
                    case "readwrite":
                        try
                        {
                            currentStream = File.Open(localPath, FileMode.Open, FileAccess.ReadWrite);
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("Exception while trying to open file:" + e.ToString());
                            host.WriteLineError("Exception while trying to open file:" + e.ToString());
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
                return (currentStream != null);
            }
            return false;
        }
        public bool CloseStream()
        {
            if (currentStream != null)
            {
                currentStream.Close();
                return true;
            }
            return false;
        }

        //Returns "___NONE___" if the stream cannot be read from or doesnt exist. 
        private string ReadLine()
        {
            string line = "___NONE___";

            if (currentStream != null && currentStream.CanRead)
            {
                line = new StreamReader(currentStream).ReadLine();
            }
            return line;
        }

        public void AddAPI(Lua lua)
        {
            new LuaAPI(lua, "fs")
                .RegisterFunction("getDirectories", this, nameof(GetDirectoriesExposed))
                .RegisterFunction("getFiles", this, nameof(GetFiles))
                .RegisterFunction("open", this, nameof(OpenFile))
                .RegisterFunction("close", this, nameof(CloseStream))
                .RegisterFunction("readLine", this, nameof(ReadLine));

        }



    }
}