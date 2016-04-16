// Author: Kelly Rey Wilson kelly@MolecularJig.com
//
// Copyright (c) 2014, NightPen, LLC and MolecularJig
//
// All rights reserved.
//
// when() statement Patent Pending
//
// While the source to JigScript is copyrighted, any JigScript
// add on function libraries you create or any JigScript script
// files you create are yours to do with as you please! If you
// develop a really cool game or function library, we would
// love to see it. You can contact us at MolecularJig.com
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using NightPen.JigScript;

public class JigScriptMenu : EditorWindow
{
    [MenuItem("Tools/JigScript/Add JigScript Prefab %p", false, 11)]
    public static void AddJigScriptPrefab(MenuCommand command)
    {
        GameObject go1 = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/JigScript/Prefabs/JigScript.prefab", typeof(GameObject));
        GameObject go = (GameObject)Instantiate(go1);
        go.name = "JigScript";

        PrefabUtility.DisconnectPrefabInstance(go);
    }

    [MenuItem("Tools/JigScript/Add JigScript Prefab %p", true)]
    public static bool ValidateAddJigScriptPrefab()
    {
        return GameObject.Find("JigScript") == false;
    }
    
    [MenuItem("Tools/JigScript/Add Script %e", false, 11)]
    public static void AddScript(MenuCommand command)
    {
        string path = EditorUtility.SaveFilePanel("Create JigScript", "Assets/Resources/JigScript", "JigScript", "txt");
        if ( !string.IsNullOrEmpty(path) )
        {
            StreamWriter sw = new StreamWriter(path);
            sw.Close();
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Tools/JigScript/Add Script %e", true)]
    public static bool ValidateAddScript()
    {
        return GameObject.Find("JigScript");
    }
    
    [MenuItem("Tools/JigScript/Add Material %g", false, 12)]
    public static void AddMaterial(MenuCommand command)
    {
        GameObject go1 = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/JigScript/Prefabs/JigMat.prefab", typeof(GameObject));
        GameObject go = (GameObject)Instantiate(go1);
        go.name = "JigMaterial";
        PrefabUtility.DisconnectPrefabInstance(go);
    }

    [MenuItem("Tools/JigScript/Add Material %g", true)]
    public static bool ValidateAddMaterial()
    {
        if ( GameObject.Find("JigScript") == null )
        {
            return false;
        }
        return true;
    }

    [MenuItem("Tools/JigScript/Run %r", false, 13)]
    public static void RunScript(MenuCommand command)
    {
        GameObject js = GameObject.Find("JigScript");
        if ( js != null )
        {
            JigCompiler compiler = js.GetComponent<JigCompiler>();
            if ( compiler != null )
            {
                compiler.RunScript(compiler.ScriptFile.ToString(), 0);
            }
        }
    }

    [MenuItem("Tools/JigScript/Run %r", true)]
    public static bool ValidateRunScript()
    {
        if ( Application.isPlaying == false )
        {
            return false;
        }
        
        GameObject js = GameObject.Find("JigScript");
        if ( js == null )
        {
            return false;
        }
        JigCompiler compiler = js.GetComponent<JigCompiler>();
        if ( compiler == null )
        {
            return false;
        }
        if ( compiler.ScriptFile == null )
        {
            return false;
        }
        return true;
    }
}