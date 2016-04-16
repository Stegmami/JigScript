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
    public class UILabel : UIControlsBase
    {
        Value v;
        GUIStyle style;
        
        UILabel(Value v)
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
                Changed = true;
                Variables.Store(v, 1, v);
            }
            else
            {
                vNew.ConvertTo(Value.ValueType.String);
                v.S = vNew.S;
                Changed = true;
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
        
        public UILabel(UIControlsBase.Location L, Rect rect, Value v, bool active) : base(L, rect, active)
        {
            this.v = v;
        }
        
        public void Update()
        {
            if (active)
            {
                v = Variables.Read(v.index, 1);
                
                if ( style != null )
                {
                    GUI.Label(GetRect(), GetContent(v), style);
                }
                else
                {
                    GUI.Label(GetRect(), GetContent(v));
                }
                    
            }
        }               
    };
}