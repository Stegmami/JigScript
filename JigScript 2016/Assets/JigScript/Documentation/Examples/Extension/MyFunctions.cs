using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NightPen.JigScript;

public class MyFunctions : JigExtension
{
    IEnumerator Trace(List<Value> values)
    {
        if (values.Count > 0)
        {
            for (int ii=0; ii<values.Count; ++ii)
            {
                values [0].ConvertTo(Value.ValueType.String);
                Debug.Log(values [0].S);
            }
            values.Add(new Value(1.0f, "Trace.Return"));
        }
        yield return 0;
    }
    
    public override void Initialize(JigCompiler compiler)
    {
        compiler.AddFunction("MyFunctions.Trace", Trace, true);
    }
}
