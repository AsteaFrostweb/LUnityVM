
using System.IO;

public class LuaFileStreamHandler
{
    public LuaFileStreamHandler(ref string rootPath)
    {
        
    }

    public FileStream fileStream { get; private set; }
    private string rootPath;

    public bool OpenFile(string path, string readWrite)
    {

        Utility.FormatPathPreCombine(ref path, () => { }, 0);
        string localPath = Path.Combine(rootPath, path);

        //Checking if the file exists
        if (File.Exists(localPath))
        {
            //If there is a currently active stream then close it 
            if (fileStream != null) fileStream.Close();
            fileStream = null;

            switch (readWrite)
            {
                case "read":
                    try
                    {
                        fileStream = File.Open(localPath, FileMode.Open, FileAccess.Read);
                    }
                    catch (System.Exception e)
                    {
                        //Debug.Log("Exception while trying to open file:" + e.ToString());
                        return false;
                    }
                    break;
                case "write":
                    try
                    {
                        fileStream = File.Open(localPath, FileMode.Open, FileAccess.Write);
                    }
                    catch (System.Exception e)
                    {
                        //Debug.Log("Exception while trying to open file:" + e.ToString());
                        return false;
                    }
                    break;
                case "readwrite":
                    try
                    {
                        fileStream = File.Open(localPath, FileMode.Open, FileAccess.ReadWrite);
                    }
                    catch (System.Exception e)
                    {
                        //Debug.Log("Exception while trying to open file:" + e.ToString());
                        return false;
                    }
                    break;
                default:
                    return false;
            }
            return (fileStream != null);
        }
        return false;
    }
    public bool CloseStream()
    {
        if (fileStream != null)
        {
            fileStream.Close();
            return true;
        }
        return false;
    }

}
