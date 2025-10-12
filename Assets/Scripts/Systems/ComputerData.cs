using NLua;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace Computers
{
    public static class ComputerData
    {
        public static string currentSaveExtension = "/ComputerData/";      
        public static int currentFocusedMachine = -1;
        public static Selectable currentSelectable;    
        public static UnityMainThreadDispatcher mainThreadDispatcher;
        private static string dataPath = null;
        public static string ActiveSavePath()
        {
            if (dataPath == null) dataPath = Application.persistentDataPath;
            return dataPath + currentSaveExtension;

            //return Path.Combine(Application.persistentDataPath, currentSaveExtension);
        }
    } 
}
