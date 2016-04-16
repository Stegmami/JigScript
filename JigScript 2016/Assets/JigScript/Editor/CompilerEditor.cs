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

using UnityEngine;
using UnityEditor;
using System.Collections;
using NightPen.JigScript;

[CustomEditor(typeof(JigCompiler))]
public class CompilerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (Application.isPlaying)
        {
            GameObject go = GameObject.Find("JigScript");
            if (go != null)
            {
                JigCompiler compiler = go.GetComponent<JigCompiler>();
                if (compiler != null )
                {
                    if (GUILayout.Button("Run"))
                    {
                        compiler.RunScript(compiler.ScriptFile.ToString(), 0);
                    }
                    if (GUILayout.Button("Show Dissassembly"))
                    {
                        compiler.Dissassemble(compiler.ScriptFile.ToString(), 0);
                    }
                }
            }
        }
    }
}