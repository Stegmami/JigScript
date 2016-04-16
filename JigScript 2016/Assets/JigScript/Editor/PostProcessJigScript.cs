// Author: Kelly Rey Wilson kelly@MolecularJig.com
//
// Copyright (c) 2014, NightPen, LLC and MolecularJig
//
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

namespace NightPen.JigScript
{
    public class PostProcessJigScript : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths )
        {
            if ( Application.isPlaying )
            {
                GameObject obj = GameObject.Find("JigScript");
                JigCompiler compiler = obj.GetComponent<JigCompiler>();
                
                if ( compiler != null && compiler.ScriptFile != null && string.IsNullOrEmpty(compiler.ScriptFile.ToString()) == false && compiler.runOnImport )
                {
                    string path = AssetDatabase.GetAssetPath(compiler.ScriptFile.GetInstanceID());
                    foreach( string str in importedAssets )
                    {
                        if ( str == path )
                        {
                            TextAsset ta = (TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset));
                            compiler.RunScript(ta.ToString(), 0);
                            break;
                        }
                    }
                }
            }
        }
    }
}