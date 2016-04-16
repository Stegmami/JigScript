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
using System;
using System.Collections;
using System.Collections.Generic;


namespace NightPen.JigScript
{
    public class Array : JigExtension
    {
        int NumberOfValuesRequired = 0;
        
        IEnumerator MyFunctionName( List<Value> values )
        {
            if ( values.Count != NumberOfValuesRequired )
            {
                Debug.LogError("return value = MyFunction(value1, value2, ...);");
            }
            else
            {
                //Process my function
            }
            yield return 0;
        }
        
        IEnumerator LengthFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("Arrays.Length(variable);");
            }
            else
            {
                Value v = new Value(Variables.Length(values[0]), values[0].name + ".Length");
                
                values.Add(v);
            }
            yield return 0;
        }
    
        IEnumerator CopyFunction( List<Value> values )
        {
            if ( values.Count != 2 )
            {
                Debug.LogError("Arrays.Copy(destination, source);");
            }
            else
            {
                Variables.Copy(values[0], values[1]);
            }
            yield return 0;
        }
    
        IEnumerator FillFunction( List<Value> values )
        {
            if ( values.Count != 3 )
            {
                Debug.LogError("Arrays.Fill(variable, start, value);");
            }
            else
            {
                values[1].ConvertTo(Value.ValueType.Integer);
    
                Variables.FillArray(values[0], values[1].I, values[2]);
            }
            yield return 0;
        }
    
        IEnumerator ClearFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("Arrays.Clear(variable);");
            }
            else
            {
                Variables.Clear(values[0]);
            }
            yield return 0;
        }
    
        IEnumerator DeleteFunction( List<Value> values )
        {
            if ( values.Count != 3 )
            {
                Debug.LogError("Arrays.Delete(variable, firstRow, rows to delete);");
            }
            else
            {
                values[1].ConvertTo(Value.ValueType.Integer);
                values[2].ConvertTo(Value.ValueType.Integer);
                Variables.Delete(values[0], values[1].I, values[2].I);
            }
            yield return 0;
        }
    
        private Value GetLength(Value v)
        {
            v.I = Variables.Length(v);
            v.T = Value.ValueType.Integer;
            
            return v;
        }
        
        private void SetLength(Value dest, int arrayIndex, Value source)
        {
            if ( dest.isVariable )
            {
                source.ConvertTo(Value.ValueType.Integer);
                
                int len = Variables.Length(dest);
                int index = source.I;
                if ( index <= 0 )
                {
                    index = 0;
                }
                Variables.Delete(dest, index, len - index);
            }
        }
    
        public override void Initialize(JigCompiler compiler)
        {
            compiler.AddFunction("Arrays.Length", LengthFunction);
            compiler.AddFunction("Arrays.Copy", CopyFunction, true);
            compiler.AddFunction("Arrays.Clear", ClearFunction, true);
            compiler.AddFunction("Arrays.Fill", FillFunction, true);
            compiler.AddFunction("Arrays.Delete", DeleteFunction, true);
            
            Variables.CreateCustomProperty("length", GetLength, SetLength);
        }
    }
}