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
using UnityEngine.Windows.WebCam;
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
                        if (IsROMPath(path))
                        {
                            host.WriteLineError("Unable to open stream with write permissions to file within ROM");
                            return false;
                        }
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
                        if (IsROMPath(path))
                        {
                            host.WriteLineError("Unable to open stream with write permissions to file within ROM");
                            return false;
                        }
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


        // Writes text (without newline)
        public bool Write(string text)
        {
            if (currentStream == null || !currentStream.CanWrite)
                return false;

          

            try
            {
                using (StreamWriter writer = new StreamWriter(currentStream, System.Text.Encoding.UTF8, 1024, true))
                {
                    writer.Write(text);
                    writer.Flush();
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error writing to file: {e}");
                host.WriteLineError("Error writing to file: " + e.Message);
                return false;
            }
        }

        // Writes text with newline
        public bool WriteLine(string text)
        {
            if (currentStream == null || !currentStream.CanWrite)
                return false;

            try
            {
                using (StreamWriter writer = new StreamWriter(currentStream, System.Text.Encoding.UTF8, 1024, true))
                {
                    writer.WriteLine(text);
                    writer.Flush();
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error writing line to file: {e}");
                host.WriteLineError("Error writing line to file: " + e.Message);
                return false;
            }
        }
        public bool CreateFile(string path)
        {
            if (IsROMPath(host.currentDirectory + "/" + path))
            {
                host.WriteLineError("Access denied: /rom/ is read-only");
                return false;
            }
            path = host.ToGlobalPath(path); //After checkign if its a rom path covert into global path for file creation
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = File.Create(path)) { }
                return true;
            }
            catch (Exception e)
            {
                host.WriteLineError("Error creating file: " + e.Message);
                return false;
            }
        }

        public bool DeleteFile(string path)
        {
            if (IsROMPath(host.currentDirectory + "/" + path))
            {
                host.WriteLineError("Access denied: Cannot delete files in /rom/");
                return false;
            }
            path = host.ToGlobalPath(path); //After checkign if its a rom path covert into global path for file creation

            try
            {
                if (!File.Exists(path)) return false;
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                host.WriteLineError("Error deleting file: " + e.Message);
                return false;
            }
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        // --- DIRECTORY FUNCTIONS ---

        public bool CreateDirectory(string path)
        {
            Debug.Log("Atemtpting to create directory at local path:" + path);
            if (IsROMPath(host.currentDirectory+ "/" + path))
            {                
                host.WriteLineError("Access denied: Cannot create directories in /rom/");
                return false;
            }
            path = host.ToGlobalPath(path); //After checkign if its a rom path covert into global path for file creation

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception e)
            {
                host.WriteLineError("Error creating directory: " + e.Message);
                return false;
            }
        }

        public bool DeleteDirectory(string path)
        {
            if (IsROMPath(host.currentDirectory + "/" + path))
            {
                host.WriteLineError("Access denied: Cannot delete directories in /rom/");
                return false;
            }
            path = host.ToGlobalPath(path); //After checkign if its a rom path covert into global path for file creation

            try
            {
                if (!Directory.Exists(path)) return false;
                Directory.Delete(path, true);
                return true;
            }
            catch (Exception e)
            {
                host.WriteLineError("Error deleting directory: " + e.Message);
                return false;
            }
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void AddAPI(Lua lua)
        {
            new LuaAPI(lua, "fs")
                .RegisterFunction("getDirectories", this, nameof(GetDirectoriesExposed))
                .RegisterFunction("getFiles", this, nameof(GetFiles))
                .RegisterFunction("open", this, nameof(OpenFile))
                .RegisterFunction("close", this, nameof(CloseStream))
                .RegisterFunction("readLine", this, nameof(ReadLine))
                .RegisterFunction("write", this, nameof(Write))
                .RegisterFunction("writeLine", this, nameof(WriteLine))
                .RegisterFunction("createFile", this, nameof(CreateFile))
                .RegisterFunction("deleteFile", this, nameof(DeleteFile))
                .RegisterFunction("fileExists", this, nameof(FileExists))
                .RegisterFunction("createDirectory", this, nameof(CreateDirectory))
                .RegisterFunction("deleteDirectory", this, nameof(DeleteDirectory))
                .RegisterFunction("directoryExists", this, nameof(DirectoryExists));
        }

        public static bool IsROMPath(string path) 
        {
            if (path == null) return false;
            string[] seperatedPath = path.Split('/', '\\');
            if (seperatedPath != null && seperatedPath.Length > 0 && seperatedPath[0] == "rom") return true;

            return false;
        }

    }
}