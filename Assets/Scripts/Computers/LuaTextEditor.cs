using System.IO;
using Unity.VisualScripting;

public class LuaTextEditor
{
    public LuaTextEditor(Shell _hostShell) 
    {       
        hostShell = _hostShell;
        initialLines = hostShell.lines;
    }
    private string[] initialLines;
    private Shell hostShell;
    public string activeSavePath { get; set; }

    public void Edit(string path) 
    {
        Utility.TrimAndRemoveAllFirst(ref path, '.', () => hostShell.GotoParentDirectory(), 1); //handling if the patrh starts with . chars
        Utility.FormatPathPreCombine(ref path, () => { }, 0);//handling if the patrh starts with / or \ chars
        string fullPath = Path.Combine(hostShell.currentDirectoryFullPath, path); //getting the absolute path to the file

        initialLines = hostShell.lines;
        if (File.Exists(fullPath))
        {
            
            //Load and display current file
            //Goto edditing function
        }
        else 
        {
            
            //Create file if possible 
            //load it onto screen althougb blank and then goto editiing function
        }
    }
}

