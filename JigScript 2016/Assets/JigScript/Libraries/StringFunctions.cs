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
using System.Text;

namespace NightPen.JigScript
{
    public class StringFunctions : JigExtension
    {
        public IEnumerator SubstringFunction( List<Value> values )
        {
            if ( values.Count != 2 && values.Count != 3 )
            {
                Debug.LogError("string = Substring(string, start) | string = Substring(string, start, length);");
            }
            else
            {
                
                values[0].ConvertTo(Value.ValueType.String);
                values[1].ConvertTo(Value.ValueType.Integer);
                string s = values[0].S;
                int index = values[1].I;
                if ( index < 0 )
                {
                    index = 0;
                }
                int length = 0;
                if ( values.Count == 3 )
                {
                    values[2].ConvertTo(Value.ValueType.Integer);
                    length = values[2].I;
                }
                else
                {
                    length = s.Length - index;
                }
                if ( index < s.Length )
                {
                    if ( index + length > s.Length )
                    {
                        length = s.Length - index;
                    }
                    if ( length < 0 )
                    {
                        s = string.Empty;
                    }
                    else
                    {
                        s = s.Substring(index, length);
                    }
                }
                else
                {
                    s = string.Empty;
                }
                values.Add(new Value(s, values[0].name));
            }
    
            yield return 0;
        }
    
        public IEnumerator TrimFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("string = Char(string);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.String);
                string s = values[0].S;
                s = s.Trim();
                values.Add(new Value(s, values[0].name));
            }
            yield return 0;
        }
    
        public IEnumerator TrimEndFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("string = Char(string);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.String);
                string s = values[0].S;
                s = s.TrimEnd();
                values.Add(new Value(s, values[0].name));
            }
            yield return 0;
        }
    
        public IEnumerator TrimStartFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("string = Char(string);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.String);
                string s = values[0].S;
                s = s.TrimStart();
                values.Add(new Value(s, values[0].name));
            }
            yield return 0;
        }
    
        public IEnumerator LengthFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("int = Length(string);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.String);
                string s = values[0].S;
                values.Add(new Value(s.Length, values[0].name));
            }
            yield return 0;
        }
    
        public override void Initialize( JigCompiler compiler )
        {
            compiler.AddFunction("Strings.Substring", SubstringFunction);
            compiler.AddFunction("Strings.Trim", TrimFunction);
            compiler.AddFunction("Strings.TrimEnd", TrimEndFunction);
            compiler.AddFunction("Strings.TrimStart", TrimStartFunction);
            compiler.AddFunction("Strings.Length", LengthFunction);
        }
    }
}