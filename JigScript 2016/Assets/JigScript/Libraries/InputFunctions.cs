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
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class InputFunctions : JigExtension
    {
        IEnumerator GetAxisFunction(List<Value> values)
        {
            if (values.Count != 1)
            {
                Debug.LogError("float = Inputs.GetAxis(\"axis\");");
            } else
            {
                values [0].ConvertTo(Value.ValueType.String);
                values.Add(new Value(Input.GetAxis(values [0].S), "Inputs.GetAxis"));
            }
            yield return 0;
        }
        
        IEnumerator GetButtonFunction(List<Value> values)
        {
            if (values.Count != 1)
            {
                Debug.LogError("bool = Inputs.GetButton(\"button\");");
            } else
            {
                values [0].ConvertTo(Value.ValueType.String);
                values.Add(new Value(Input.GetButton(values [0].S), "Inputs.GetButton"));
            }
            yield return 0;
        }
        
        IEnumerator AnyFunction(List<Value> values)
        {
            if (values.Count != 0)
            {
                Debug.LogError("bool = Inputs.Any();");
            } else
            {
                values.Add(new Value((Input.anyKey), "Inputs.Any"));
            }
            yield return 0;
        }
    
        IEnumerator KeyFunction(List<Value> values)
        {
            if (values.Count != 1)
            {
                Debug.LogError("bool = Inputs.Key(key name);");
            } else
            {
                values [0].ConvertTo(Value.ValueType.String);
                values.Add(new Value(Input.GetKey(values [0].S), "Inputs.Key"));
            }
            yield return 0;
        }
        
        public override void Initialize(JigCompiler compiler)
        {
            compiler.AddFunction("Inputs.GetAxis", GetAxisFunction);
            compiler.AddFunction("Inputs.GetButton", GetButtonFunction);
            compiler.AddFunction("Inputs.Any", AnyFunction);
            compiler.AddFunction("Inputs.Key", KeyFunction);
        }
    }
}