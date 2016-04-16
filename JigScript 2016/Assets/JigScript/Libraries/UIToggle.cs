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

namespace NightPen.JigScript
{
    public class UIToggle : UIControlsBase
    {
        Value v;
        
        GUIStyle style;
        
        public UIToggle(Value v)
        {
            this.v = new Value(v);
            style = new GUIStyle();
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
            }
            else if ( vNew.T == Value.ValueType.String )
            {
                v.T = Value.ValueType.String;
                v.S = vNew.S;
            }
            else
            {
                vNew.ConvertTo(Value.ValueType.Bool);
                v.B = vNew.B;
            }
            Changed = true;
            Variables.Store(v, 1, v);
        }
        
        public UIToggle(UIControlsBase.Location L, Rect rect, Value v, bool active) : base(L, rect, active)
        {
            this.v = v;
        }
        
        public Value Get()
        {
            return new Value(v.B, Variables.GetUniqueName());
        }
        
        private bool Process(bool B)
        {
            bool rc;
            
            if ( style != null )
            {
                rc = GUI.Toggle(GetRect(), B, GetContent(v), style);
            }
            else
            {
                rc = GUI.Toggle(GetRect(), B, GetContent(v));
            }
            
            return rc;
        }
        
        public void Update()
        {
            if (active)
            {
                v = Variables.Read(v.index, 1);
                bool b = Process(v.B);
                if (b != v.B)
                {
                    v.B = b;
                    Variables.Store(v, v.arrayIndex, v);
                    Clicked = true;
                    Changed = true;
                    CPU.whensNeeded = true;
                }
            }
        }               
    };
}