using NLua;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public static class GameData
{
    public static string currentSaveExtension = "/ComputerData/";
    public static bool playerRotationInputLocked = false;    
    public static GameObject firstPersonCamera;
    public static GameObject thirdPersonCamera;
    public static float scrollDelta = 0f;
    public static int currentFocusedMachine = -1;
    public static Selectable currentSelectable;
    public static string selectionText = "";
    public static UnityMainThreadDispatcher mainThreadDispatcher;

    private static string dataPath = null;
    public static string ActiveSavePath() 
    {
        if (dataPath == null) dataPath = Application.persistentDataPath;
        return dataPath + currentSaveExtension;

        //return Path.Combine(Application.persistentDataPath, currentSaveExtension);
    } 
}
