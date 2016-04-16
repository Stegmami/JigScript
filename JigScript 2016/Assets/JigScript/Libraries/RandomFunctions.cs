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
    public class RandomFunctions : JigExtension
    {
        object rlock = new object();
        
        private IEnumerator RandomFunction(List<Value> values)
        {
            if (values.Count != 2)
            {
                Debug.LogError("Random(min, max);");
            }
            else
            {
                Value rc;
                
                lock( rlock )
                {
                    if (values [0].T == Value.ValueType.Float)
                    {
                        values [1].ConvertTo(Value.ValueType.Float);
                        rc = new Value(UnityEngine.Random.Range(values [0].F, values [1].F), "Random.Float");
                    }
                    else
                    {
                        values [0].ConvertTo(Value.ValueType.Integer);
                        values [1].ConvertTo(Value.ValueType.Integer);
                        rc = new Value(UnityEngine.Random.Range(values [0].I, values [1].I), "Random.Integer");
                    }
                    values.Add(rc);
                }
            }
            
            yield return 0;
        }
    
        public override void Initialize(JigCompiler compiler)
        {
            compiler.AddFunction("Random", RandomFunction);
        }
    }
}