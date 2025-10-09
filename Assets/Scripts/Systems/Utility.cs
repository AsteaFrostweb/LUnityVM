using Microsoft.Win32.SafeHandles;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;

public static class Utility 
{
    public static LuaTable CreateTable(Lua lua) => CreateTable(lua, "");
    public static LuaTable CreateTable(Lua lua, string name)
    {
        lua.NewTable(name);
        return lua.GetTable(name);
    }


    public static T[] ShiftArray<T>(T[] arr, T append)
    {
        if (arr.Length <= 1) return new T[0];  // Handle cases with 1 or fewer elements

        // Shift elements backward
        for (int i = 1; i < arr.Length; i++)
        {
            arr[i - 1] = arr[i];
        }

        arr[arr.Length - 1] = append;
        return arr;
    }


    //Some of the chars are cunstom to help represent non character keys.
    // uE000 - LeftAlt
    
    public static char KeyCodeToChar(KeyCode key, bool isUpperCase)
    {
        if (isUpperCase)
        {
            switch (key)
            {
                case KeyCode.Backslash:
                    return '|';
                case KeyCode.Space:
                    return ' ';
                case KeyCode.Alpha1:
                    return '!';
                case KeyCode.Alpha2:
                    return '@';
                case KeyCode.Alpha3:
                    return '#';
                case KeyCode.Alpha4:
                    return '$';
                case KeyCode.Alpha5:
                    return '%';
                case KeyCode.Alpha6:
                    return '^';
                case KeyCode.Alpha7:
                    return '&';
                case KeyCode.Alpha8:
                    return '*';
                case KeyCode.Alpha9:
                    return '(';
                case KeyCode.Alpha0:
                    return ')';
                case KeyCode.Minus:
                    return '_';
                case KeyCode.Equals:
                    return '+';
                case KeyCode.LeftBracket:
                    return '{';
                case KeyCode.RightBracket:
                    return '}';                
                case KeyCode.Semicolon:
                    return ':';
                case KeyCode.Quote:
                    return '"';
                case KeyCode.Comma:
                    return '<';
                case KeyCode.Period:
                    return '>';
                case KeyCode.Slash:
                    return '?';

                    // Handle other symbols if needed
            }
        }
        else
        {
            switch (key)
            {
                case KeyCode.Space:
                    return ' ';
                case KeyCode.Comma:
                    return ',';
                case KeyCode.Period:
                    return '.';
                case KeyCode.Semicolon:
                    return ';';
                case KeyCode.Colon:
                    return ':';
                case KeyCode.Exclaim:
                    return '!';
                case KeyCode.Question:
                    return '?';
                case KeyCode.Ampersand:
                    return '&';
                case KeyCode.Quote:
                    return '\'';
                case KeyCode.DoubleQuote:
                    return '"';
                case KeyCode.LeftBracket:
                    return '[';
                case KeyCode.RightBracket:
                    return ']';
                case KeyCode.LeftParen:
                    return '(';
                case KeyCode.RightParen:
                    return ')';
                case KeyCode.Plus:
                    return '+';
                case KeyCode.Minus:
                    return '-';
                case KeyCode.Equals:
                    return '=';
                case KeyCode.Underscore:
                    return '_';
                case KeyCode.Slash:
                    return '/';
                case KeyCode.Backslash:
                    return '\\';
                case KeyCode.Tilde:
                    return '~';
                case KeyCode.Return:
                    return '\r';
                case KeyCode.Backspace:
                    return '\b';
               
                    // Handle other symbols if needed
            }
        }

        // Handle alphanumeric keys
        if (key >= KeyCode.A && key <= KeyCode.Z)
        {
            // Convert KeyCode.A to 'a' (or 'A' if isUpperCase is true)
            char baseChar = (char)('a' + (key - KeyCode.A));
            return isUpperCase ? char.ToUpper(baseChar) : baseChar;
        }
        else if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
        {
            // Convert KeyCode.Alpha0 to '0', KeyCode.Alpha1 to '1', etc.
            return (char)('0' + (key - KeyCode.Alpha0));
        }

        // If no matching KeyCode, return a default or an empty char
        return '\0';
    }

    //trims the first character from a string and preforns an action, repeats this until the "trimmed" char istn the first char of the string
    public static void TrimAndRemoveAllFirst(ref string str, char trimmed, Action onTrim) 
    {
        if (str.Substring(0, 1) == trimmed.ToString())      
        {
            onTrim.Invoke();
            str = str.Substring(1);  //remove the first letter

            if (str.Length <= 0) return; //Check if string is now empty

            while (str.Substring(0, 1) == trimmed.ToString())
            {
                onTrim.Invoke();      //while its stillt he first letter invoke the ontrim event

               
                str = str.Substring(1);
                if (str.Length <= 0) return;
            }
        }
    }
    //same as above but you can decide which index you want to start preforming the ontrim action
    public static void TrimAndRemoveAllFirst(ref string str, char trimmed, Action onTrim, int onTrimStartIndex)
    {
        int counter = 0;

        if (str.Length <= 0) return;
        if (str.Substring(0, 1) == trimmed.ToString())
        {
            Debug.Log(str.Substring(0, 1) + " == " + trimmed.ToString());
            if (counter >= onTrimStartIndex) 
            {
                onTrim.Invoke();
            }
            counter++;

            str = str.Substring(1);  //remove the first letter

            if (str.Length <= 0) return; //Check if string is now empty

            while (str.Substring(0, 1) == trimmed.ToString())
            {
                Debug.Log(str.Substring(0, 1) + " == " + trimmed.ToString());
                if (counter >= onTrimStartIndex) //if the counter is creater than the start index or equal then preform the ionvoke
                {
                    Debug.Log("Invoking onTrim");
                    onTrim.Invoke();
                }
                counter++; // increment counter

                str = str.Substring(1);
                if (str.Length <= 0) return;
            }
        }
    }
    public static void TrimAbsolutePrecedingCharacters(ref string str, Action onTrim, int onTrimStartIndex) 
    {
        if (str.Length <= 0) return;

        Utility.TrimAndRemoveAllFirst(ref str, '/', onTrim, onTrimStartIndex);
        if (str == "") return;

        Utility.TrimAndRemoveAllFirst(ref str, '\\', onTrim, onTrimStartIndex);
        if (str == "") return;      
    }

    public static void FormatPathPreCombine(ref string str, Action onTrim, int onTrimStartIndex) 
    {
        //remove the abolute markers to avoid leaving the simulated computer root
        TrimAbsolutePrecedingCharacters(ref str, onTrim, onTrimStartIndex);

        //replace user entered foreward slashed with backslashed for aesthetics
        str = str.Replace('/', '\\');
    }


    public static bool GetKeyOrDown(KeyCode k)
    { 
        return Input.GetKeyDown(k) || Input.GetKey(k);
    }
}
