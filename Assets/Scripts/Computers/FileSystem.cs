using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NLua;

public class FileSystem 
{
    private string rootPath;
    private FileStream currentStream;

    public FileSystem(string rootpath) 
    {
        rootPath = rootpath;
        if (!Directory.Exists(rootpath)) 
        {
            Debug.Log("Creating computer file system at location: " + rootpath);         
            Directory.CreateDirectory(rootpath + "rom/programs");
            Debug.Log("Copying base rom programs from game install");
        }        
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
        string localPath = Path.Combine(rootPath, path);
        if (!Directory.Exists(localPath)) return null;

      
        string[] files = Directory.GetFiles(localPath);

        //regex to extract only the file names
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Regex.Match(files[i], @"[^/\\]+$").Value;
        }

        return files;
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

    public void AddAPI(Lua lua)
    {
        //register the file system api for file related querys
        LuaTable fsAPI = Utility.CreateTable(lua, "fs");
        fsAPI["getDirectories"] = lua.RegisterFunction("fs.getDirectories", this, typeof(FileSystem).GetMethod("GetDirectories"));
        fsAPI["getFiles"] = lua.RegisterFunction("fs.getFiles", this, typeof(FileSystem).GetMethod("GetFiles"));

        //register the io api for Inpput output to files
        
    }
   
}
