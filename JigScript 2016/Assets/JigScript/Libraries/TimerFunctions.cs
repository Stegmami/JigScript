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
    public class TimerFunctions : JigExtension
    {
        JigCompiler jigCompiler;
        
        private IEnumerator WaitFunction( List<Value> values )
        {
            if ( values.Count != 1 && values.Count != 2 )
            {
                Debug.LogError("Timer.Delay(time in seconds, check whens = [true = default | false]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Float);
                bool checkWhens = true;
                if ( values.Count == 2 )
                {
                    values[1].ConvertTo(Value.ValueType.Bool);
                    checkWhens = values[1].B;
                }
    
                if ( checkWhens == false )
                {
                    yield return new WaitForSeconds(values[0].F);
                }
                else
                {
                    float currentTime = Time.time;
                    float endTime = values[0].F + currentTime;
    
                    while( Time.time < endTime )
                    {
                        yield return new WaitForFixedUpdate();
                        jigCompiler.ProcessWhens();
                    }
                }
            }
    
            yield return 0;
        }
    
        private IEnumerator TimeFunction( List<Value> values )
        {
            values.Add(new Value(Time.time, "Unity.Delta.Time"));
            yield return 0;
        }
    
        public override void Initialize(JigCompiler compiler)
        {
            this.jigCompiler = compiler;
            
            compiler.AddFunction("Timers.Wait", WaitFunction);
            compiler.AddFunction("Timers.Time", TimeFunction);
        }
    }
}