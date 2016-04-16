using UnityEngine;
using System.Collections;
using NightPen.JigScript;

public class JigConsole : MonoBehaviour
{
    [Multiline(10)]
    public string
        script;

    void Start()
    {
    }
    
    void OnGUI()
    {
        GUILayout.Window(1, new Rect(0, Screen.height - 186, Screen.width, 160), func, "Console");
    }

    void func(int id)
    {
        if (GUILayout.Button("Run", GUILayout.Width(64), GUILayout.Height(32)))
        {
            JigCompiler compiler = GameObject.Find("JigScript").GetComponent<JigCompiler>();
            compiler.RunScript(script, 1);
        }
        script = GUILayout.TextArea(script, GUILayout.Height(120));
    }
}