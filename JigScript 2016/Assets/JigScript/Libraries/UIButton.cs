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
    public class UIButton : UIControlsBase
    {
        Value v;
        GUIStyle style;
        
        public UIButton(Value v)
        {
            this.v = new Value(v);
            style = null;
        }
        
        public void SetStyle(GUIStyle style)
        {
            this.style = style;
        }
        
        public void Set(Value vNew)
        {
            if ( vNew.T == Value.ValueType.GObject )
            {
                v.T = Value.ValueType.GObject;
                v.G = vNew.G;
                Variables.Store(v, 1, v);
            }
            else
            {
                vNew.ConvertTo(Value.ValueType.String);
                v.S = vNew.S;
                Variables.Store(v, 1, v);
            }
        }
        
        public Value Get()
        {
            Value v1 = new Value(0, Variables.GetUniqueName());
            if ( v.T == Value.ValueType.GObject )
            {
                v1.T = Value.ValueType.GObject;
                v1.G = v.G;
            }
            else
            {
                v1.T = Value.ValueType.String;
                v1.S = v.S;
            }
            
            return v1;
        }
        
        public UIButton(UIControlsBase.Location L, Rect rect, Value v, bool active) : base(L, rect, active)
        {
            this.v = v;
        }
        
        private bool Process()
        {
            bool rc;
            
            if ( style != null )
            {
                rc = GUI.Button(GetRect (), GetContent(v), style);
            }
            else
            {
                rc = GUI.Button(GetRect (), GetContent(v));
            }
            
            return rc;
        }
        
        public void Update()
        {
            if (active)
            {
                v = Variables.Read(v.index, 1);
                
                if (this.Process())
                {
                    v.B = true;
                    Variables.Store(v, v.arrayIndex, v);
                    Clicked = true;
                    Changed = true;
                    CPU.whensNeeded = true;
                }
            }
        }               
    };
}