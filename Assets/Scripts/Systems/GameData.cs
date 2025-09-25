using NLua;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum CameraState { FIRST_PERSON,  THIRD_PERSON }
public enum InputFocus { FLIGHT, COMPUTER }

public struct Focus 
{

    public InputFocus inputFocus;
    public string identifier;
    public Focus(InputFocus input, string ident)
    {
        identifier = ident;
        inputFocus = input;
    }

    public bool Equals(Focus f)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
    {
        return ((f.inputFocus == inputFocus) && (f.identifier == identifier));
    }
    public bool Equals(InputFocus inf, string i)
    {
        return ((inf == inputFocus) && (i == identifier));
    }


    public static Focus BASE_FLIGHT = new Focus(InputFocus.FLIGHT, "");
}

public static class GameData
{
    public static string currentSaveExtension = "/.spacegame/Saves/Test/";
    public static bool playerRotationInputLocked = false;    
    public static GameObject firstPersonCamera;
    public static GameObject thirdPersonCamera;
    public static float scrollDelta = 0f;
    public static CameraState cameraState = CameraState.THIRD_PERSON;
    public static Focus currentFocus = Focus.BASE_FLIGHT;
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
